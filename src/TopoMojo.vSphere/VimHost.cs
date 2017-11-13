using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.vSphere
{
    public class VimHost
    {
        public VimHost(
            PodConfiguration options,
            ConcurrentDictionary<string, Vm> vmCache,
            BitArray master,
            ILogger<VimHost> logger
        )
        {
            _logger = logger;
            _config = (PodConfiguration)Helper.Clone(options);
            _vmCache = vmCache;
            _logger.LogDebug($"Instantiated vSphereHost { _config.Host }");
            _pgCache = new Dictionary<string,int>();
            _pgAllocation = new Dictionary<string, PortGroupAllocation>();
            _swAllocation = new Dictionary<string, int>();
            _tasks = new Dictionary<string, VimHostTask>();
            //_gcPortGroups = new List<string>();
            //_vlanMap = new BitArray(4096).Or(master);

            ResolveConfigMacros();
            Task sessionMonitorTask = MonitorSession();
            Task taskMonitorTask = MonitorTasks();
        }

        private readonly ILogger<VimHost> _logger;
        private ConcurrentDictionary<string, Vm> _vmCache;
        //List<string> _gcPortGroups;
        Dictionary<string,int> _pgCache;
        Dictionary<string,PortGroupAllocation> _pgAllocation;
        Dictionary<string, int> _swAllocation;
        Dictionary<string, VimHostTask> _tasks;
        //private BitArray _vlanMap;

        PodConfiguration _config = null;
        VimPortTypeClient _vim = null;
        ServiceContent _sic = null;
        UserSession _session = null;
        HostConfigManager _hcm = null;
        ManagedObjectReference _props, _vdm, _file;
        ManagedObjectReference _datacenter, _vms, _res, _pool;
        ManagedObjectReference _net, _opt, _svc, _dt, _dsb, _hauth, _ds;
        int _pollInterval = 1000;
        int _syncInterval = 30000;
        int _taskMonitorInterval = 3000;
        bool _disposing;

        public string Name
        {
            get { return _config.Host;}
        }

        public PodConfiguration Options
        {
            get { return _config; }
        }

        public Vlan[] PgCache
        {
            get { return _pgAllocation.Keys.Select(x=> new Vlan { Name = x, Id = _pgAllocation[x].VlanId }).ToArray(); }
        }

        //public BitArray VlanMap { get { return _vlanMap; }}

        public async Task<Vm[]> Find(string term)
        {
            await Connect();
            Vm[] list = await ReloadVmCache();
            if (term.HasValue())
                return list.Where(o=>o.Id.Contains(term) || o.Name.Contains(term)).ToArray();
            return list;
        }

        public async Task<Vm> Start(string id)
        {
            await Connect();
            Vm vm = _vmCache[id];
            _logger.LogDebug($"Starting vm {vm.Name}");
            if (vm.State != VmPowerState.running)
            {
                ManagedObjectReference task = await _vim.PowerOnVM_TaskAsync(vm.AsVim(), null);
                TaskInfo info = await WaitForVimTask(task);
                vm.State = (info.state == TaskInfoState.success)
                    ? VmPowerState.running
                    : vm.State;
                if (vm.State != VmPowerState.running)
                    throw new Exception(info.error.localizedMessage);

                //apply guestinfo for annotations
                await ReconfigureVm(id, "guest", "", "");

            }

            _vmCache.TryUpdate(vm.Id, vm, vm);
            return vm;
        }

        public async Task<Vm> Stop(string id)
        {
            await Connect();
            Vm vm = _vmCache[id];
            _logger.LogDebug($"Stopping vm {vm.Name}");
            if (vm.State == VmPowerState.running)
            {
                ManagedObjectReference task = await _vim.PowerOffVM_TaskAsync(vm.AsVim());
                TaskInfo info = await WaitForVimTask(task);
                vm.State = (info.state == TaskInfoState.success)
                    ? VmPowerState.off
                    : vm.State;
                if (vm.State == VmPowerState.running)
                    throw new Exception(info.error.localizedMessage);
            }
            _vmCache.TryUpdate(vm.Id, vm, vm);
            return vm;
        }

        public async Task<Vm> Save(string id)
        {
            await Connect();
            Vm vm = _vmCache[id];

            //protect stock disks; only save a disk if it is local to the topology
            //i.e. the disk folder matches the topologyId
            if (vm.Name.Tag().HasValue() && !vm.DiskPath.Contains(vm.Name.Tag()))
                throw new InvalidOperationException("External templates must be cloned into local templates in order to be saved.");

            _logger.LogDebug($"Save: get current snap for vm {vm.Name}");

            //Get the current snapshot mor
            ManagedObjectReference mor = null;
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.VmFilter(vm.AsVim(), "snapshot"));
            ObjectContent[] oc = response.returnval;
            if (oc.Length > 0 && oc[0].propSet.Length > 0 && oc[0].propSet[0].val != null)
                mor = ((VirtualMachineSnapshotInfo)oc[0].propSet[0].val).currentSnapshot;

            //add new snapshot
            _logger.LogDebug($"Save: add new snap for vm {vm.Name}");
            ManagedObjectReference task = await _vim.CreateSnapshot_TaskAsync(
                vm.AsVim(),
                "Root Snap",
                "Created by TopoMojo Save at " + DateTime.UtcNow.ToString("s") + "Z",
                false, false);
            TaskInfo info = await WaitForVimTask(task);

            //remove previous snapshot
            if (mor != null)
            {
                _logger.LogDebug($"Save: remove previous snap for vm {vm.Name}");
                task = await _vim.RemoveSnapshot_TaskAsync(mor, false, true);
                info = await GetVimTaskInfo(task);
                if (info.state == TaskInfoState.error)
                    throw new Exception(info.error.localizedMessage);

                if (info.progress < 100) {
                    var t = new VimHostTask { Task = task, Action = "saving", WhenCreated = DateTime.UtcNow };
                    vm.Task = new VmTask { Name= t.Action, WhenCreated = t.WhenCreated, Progress = t.Progress };
                    _tasks.Add(vm.Id, t);
                }
            }
            _vmCache.TryUpdate(vm.Id, vm, vm);
            return vm;
        }

        public async Task<Vm> Revert(string id)
        {
            await Connect();
            Vm vm = _vmCache[id];
            _logger.LogDebug($"Stopping vm {vm.Name}");
            ManagedObjectReference task = await _vim.RevertToCurrentSnapshot_TaskAsync(
                vm.AsVim(), null, false);
            TaskInfo info = await WaitForVimTask(task);
            if (vm.State == VmPowerState.running)
                await Start(id);
            _vmCache.TryUpdate(vm.Id, vm, vm);
            return vm;
        }

        public async Task<Vm> Delete(string id)
        {
            //Implemented by stopping vm (if necessary), unregistering vm, and deleting vm folder
            //This protects the base disk from deletion.  When we get vvols, and a data provider
            //with instance-clone of vvols, every vm will have its own disk, and we can just
            //delete the vm.
            await Connect();
            Vm vm = _vmCache[id];
            _logger.LogDebug($"Delete: stopping vm {vm.Name}");
            await Stop(id);
            vm.State = VmPowerState.off;
            _logger.LogDebug($"Delete: unregistering vm {vm.Name}");
            string[] vmnets = await LoadVmPortGroups(vm.AsVim());
            await _vim.UnregisterVMAsync(vm.AsVim());
            string folder = vm.Path.Substring(0, vm.Path.LastIndexOf('/'));
            _logger.LogDebug($"Delete: deleting vm folder {folder}");
            await _vim.DeleteDatastoreFile_TaskAsync(_sic.fileManager, folder, _datacenter);
            _vmCache.TryRemove(vm.Id, out vm);
            //remove vm nets
            await RemoveVmPortGroups(vmnets);
            await RemoveVirtualSwitch(new string[] { vm.Name.Tag().ToSwitchName() });
            vm.Status = "initialized";
            return vm;
        }

        public async Task<string> GetTicket(string id)
        {
            await Connect();
            Vm vm = _vmCache[id];
            _logger.LogDebug($"Aquiring mks ticket for vm {vm.Name}");
            return (await _vim.AcquireTicketAsync(vm.AsVim(), "webmks")).ticket;
        }

        public async Task<Vm> Deploy(Template template)
        {
            Vm vm = null;
            await Connect();

            _logger.LogDebug("deploy: validate portgroups...");
            await ProvisionPortGroups(template);

            _logger.LogDebug("deploy: transform template...");
            VirtualMachineConfigSpec vmcs = Transform.TemplateToVmSpec(template, _config.VmStore);

            _logger.LogDebug("deploy: create vm...");
            ManagedObjectReference task = await _vim.CreateVM_TaskAsync(_vms, vmcs, _pool, null);
            TaskInfo info = await WaitForVimTask(task);
            if (info.state == TaskInfoState.success)
            {
                _logger.LogDebug("deploy: load vm...");
                vm = await GetVirtualMachine((ManagedObjectReference)info.result);

                _logger.LogDebug("deploy: create snapshot...");
                task = await _vim.CreateSnapshot_TaskAsync(
                    vm.AsVim(),
                    "Root Snap",
                    "Created by TopoMojo Deploy at " + DateTime.UtcNow.ToString("s") + "Z",
                    false, false);
                info = await WaitForVimTask(task);
                if (info.state == TaskInfoState.success)
                {
                    _logger.LogDebug("deploy: start vm...");
                    vm = await Start(vm.Id);
                }
            }
            else
            {
                throw new Exception(info.error.localizedMessage);
            }
            return vm;
        }

        public async Task<Vm> Change(string id, KeyValuePair change)
        {
            return await ReconfigureVm(id, change.Key, "", change.Value);
        }

        //id, feature (iso, net, boot, guest), label, value
        public async Task<Vm> ReconfigureVm(string id, string feature, string label, string newvalue)
        {
            await Connect();
            Vm vm = _vmCache[id];
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.VmFilter(vm.AsVim(), "config"));
            ObjectContent[] oc = response.returnval;
            VirtualMachineConfigInfo config = (VirtualMachineConfigInfo)oc[0].propSet[0].val;

            VirtualMachineConfigSpec vmcs = new VirtualMachineConfigSpec();

            switch (feature)
            {
                case "iso":
                    VirtualCdrom cdrom = (VirtualCdrom)((label.HasValue())
                        ? config.hardware.device.Where(o => o.deviceInfo.label == label).SingleOrDefault()
                        : config.hardware.device.OfType<VirtualCdrom>().FirstOrDefault());

                    if (cdrom != null)
                    {
                        if (cdrom.backing.GetType() != typeof(VirtualCdromIsoBackingInfo))
                            cdrom.backing = new VirtualCdromIsoBackingInfo();

                        ((VirtualCdromIsoBackingInfo)cdrom.backing).fileName = newvalue;
                        cdrom.connectable = new VirtualDeviceConnectInfo {
                            connected = true,
                            startConnected = true
                        };

                        vmcs.deviceChange = new VirtualDeviceConfigSpec[] {
                            new VirtualDeviceConfigSpec {
                                device = cdrom,
                                operation = VirtualDeviceConfigSpecOperation.edit,
                                operationSpecified = true
                            }
                        };
                    }
                    break;

                case "net":
                case "eth":
                    VirtualEthernetCard card = (VirtualEthernetCard)((label.HasValue())
                        ? config.hardware.device.Where(o => o.deviceInfo.label == label).SingleOrDefault()
                        : config.hardware.device.OfType<VirtualEthernetCard>().FirstOrDefault());

                    if (card != null)
                    {
                        VirtualEthernetCardNetworkBackingInfo backing  = (VirtualEthernetCardNetworkBackingInfo)card.backing;
                        //cacheNet(backing.deviceName);
                        backing.deviceName = newvalue;
                        card.connectable = new VirtualDeviceConnectInfo() {
                            connected = true,
                            startConnected = true,
                        };

                        vmcs.deviceChange = new VirtualDeviceConfigSpec[] {
                            new VirtualDeviceConfigSpec {
                                device = card,
                                operation = VirtualDeviceConfigSpecOperation.edit,
                                operationSpecified = true
                            }
                        };
                    }
                    break;

                case "boot":
                    int delay = 0;
                    if (Int32.TryParse(newvalue, out delay))
                        vmcs.AddBootOption(delay);
                    break;

                case "guest":
                    if (newvalue.HasValue() && !newvalue.EndsWith("\n"))
                        newvalue += "\n";
                    vmcs.annotation = config.annotation + newvalue;
                    if (vm.State == VmPowerState.running && vmcs.annotation.HasValue())
                        vmcs.AddGuestInfo(Regex.Split(vmcs.annotation, "\r\n|\r|\n"));
                    break;

                default:
                    throw new Exception("Invalid change request.");
                    //break;
            }

            ManagedObjectReference task = await _vim.ReconfigVM_TaskAsync(vm.AsVim(), vmcs);
            TaskInfo info = await WaitForVimTask(task);
            if (info.state == TaskInfoState.error)
                throw new Exception(info.error.localizedMessage);
            return await GetVirtualMachine(vm.AsVim());
        }

        public async Task<int> TaskProgress(string id)
        {
            await Connect();
            int progress = -1;
            if (_taskMap.ContainsKey(id))
            {
                if (_taskMap[id] != null)
                {
                    _taskMap[id] = await GetVimTaskInfo(_taskMap[id].task);
                    switch (_taskMap[id].state)
                    {
                        case TaskInfoState.error:
                            string msg = _taskMap[id].description.message + " - " +
                                _taskMap[id].error.localizedMessage;
                            _taskMap.Remove(id);
                            throw new Exception(msg);
                            //break;

                        case TaskInfoState.success:
                            progress = 100;
                            _taskMap.Remove(id);
                            _logger.LogDebug($"TaskProgress: {id} {progress}");
                            break;

                        default:
                            progress = _taskMap[id].progress;
                            break;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return progress;
        }

        public async Task<bool> FolderExists(string path)
        {
            string[] files = await GetFiles(path + "/*", false);
            return files.Length > 0;
        }

        public async Task<bool> FileExists(string path)
        {
            string[] list = await GetFiles(path, false);
            return list.Length > 0;
        }

        public async Task<string[]> GetFiles(string path, bool recursive)
        {
            await Connect();
            List<string> list = new List<string>();
            DatastorePath dsPath = new DatastorePath(path);

            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props, FilterFactory.DatastoreFilter(_ds));
            ObjectContent[] oc = response.returnval;

            foreach (ObjectContent obj in oc)
            {
                ManagedObjectReference dsBrowser = (ManagedObjectReference)obj.propSet[0].val;
                string dsName = ((DatastoreSummary)obj.propSet[1].val).name;
                if (dsName == dsPath.Datastore)
                {
                    ManagedObjectReference task = null;
                    TaskInfo info = null;
                    HostDatastoreBrowserSearchSpec spec = new HostDatastoreBrowserSearchSpec {
                        matchPattern = new string[] { dsPath.File }
                    };
                    List<HostDatastoreBrowserSearchResults> results = new List<HostDatastoreBrowserSearchResults>();
                    if (recursive)
                    {
                        task = await _vim.SearchDatastoreSubFolders_TaskAsync(
                        dsBrowser, dsPath.FolderPath, spec);
                        info = await WaitForVimTask(task);
                        if (info.result != null)
                            results.AddRange((HostDatastoreBrowserSearchResults[])info.result);
                    }
                    else
                    {
                        task = await _vim.SearchDatastore_TaskAsync(
                        dsBrowser, dsPath.FolderPath, spec);
                        info = await WaitForVimTask(task);
                        if (info.result != null)
                            results.Add((HostDatastoreBrowserSearchResults)info.result);
                    }

                    foreach (HostDatastoreBrowserSearchResults result in results)
                    {
                        if (result != null && result.file != null && result.file.Length > 0)
                        {
                            list.AddRange(result.file.Select(o=>result.folderPath + "/" + o.path));
                        }

                    }
                }
            }
            return list.ToArray();
        }

        Dictionary<string, TaskInfo> _taskMap = new Dictionary<string, TaskInfo>();
        public async Task<int> CloneDisk(string templateId, string source, string dest)
        {
            await Connect();
            _taskMap.Add(templateId, null);
            await MakeDirectories(dest);
            ManagedObjectReference task = null;
            TaskInfo info = null;
            string pattern = @"blank-(\d+)([^\.]+)";
            Match match = Regex.Match(source, pattern);
            if (match.Success)
            {
                //create virtual disk
                int size = 0;
                Int32.TryParse(match.Groups[1].Value, out size);
                string[] parts = match.Groups[2].Value.Split('-');
                string adapter = (parts.Length > 1) ? parts[1] : "lsiLogic";
                FileBackedVirtualDiskSpec spec = new FileBackedVirtualDiskSpec {
                    diskType = "thin",
                    adapterType = adapter.Replace("lsilogic", "lsiLogic").Replace("buslogic", "busLogic"),
                    capacityKb = size * 1024 * 1024
                };
                _logger.LogDebug("creating new blank disk " + dest);
                task = await _vim.CreateVirtualDisk_TaskAsync(
                    _vdm, dest, _datacenter, spec);
            }
            else
            {
                //copy virtual disk
                _logger.LogDebug("cloning new disk " + source + " -> " + dest);
                task = await _vim.CopyVirtualDisk_TaskAsync(
                    _vdm, source, _datacenter, dest, _datacenter, null, false);
            }
            info = await GetVimTaskInfo(task);
            _taskMap[templateId] = info;
            _logger.LogDebug("clonedisk: progress " + info.progress);
            return info.progress;
        }

        public async Task DeleteDisk(string path)
        {
            await Connect();
            Task task = _vim.DeleteVirtualDisk_TaskAsync(_vdm, path, null);
        }

        public async Task<string[]> GetGuestIds(string term)
        {
            await Task.Delay(0);
            return Transform.OsMap;
        }

        public async Task<Vm> AnswerVmQuestion(string id, string question, string answer)
        {
            await Connect();
            Vm vm = _vmCache[id];
            await _vim.AnswerVMAsync(vm.AsVim(), question, answer);
            vm.Question = null;
            return vm;
        }

        private async Task MakeDirectories(string path)
        {
            try
            {
                if (!FolderExists(path).Result)
                    await _vim.MakeDirectoryAsync(_file, new DatastorePath(path).FolderPath, _datacenter, true);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("MakeDirectories: " + path + " " + ex.Message);
            }
        }

        private async Task ProvisionPortGroups(Template template)
        {
            await Task.Delay(0);
            lock(_pgAllocation)
            {
                string switchName = _config.Uplink;
                if (!template.UseUplinkSwitch)
                {
                    switchName = template.IsolationTag.ToSwitchName();
                    if (!_swAllocation.ContainsKey(switchName))
                    {
                        HostVirtualSwitchSpec swspec = new HostVirtualSwitchSpec();
                        swspec.numPorts = 48;
                        // swspec.policy = new HostNetworkPolicy();
                        // swspec.policy.security = new HostNetworkSecurityPolicy();

                        _vim.AddVirtualSwitchAsync(_net, switchName, swspec).Wait();
                        _swAllocation.Add(switchName, 0);
                    }
                }

                foreach (Eth eth in template.Eth)
                {
                    if (!_pgAllocation.ContainsKey(eth.Net))
                    {
                        try
                        {
                            _logger.LogDebug($"Adding portgroup {eth.Net} ({eth.Vlan})");

                            HostPortGroupSpec spec = new HostPortGroupSpec();
                            spec.vswitchName = switchName;
                            spec.vlanId = eth.Vlan;
                            spec.name = eth.Net;
                            spec.policy = new HostNetworkPolicy();
                            spec.policy.security = new HostNetworkSecurityPolicy();
                            spec.policy.security.allowPromiscuous = true;
                            spec.policy.security.allowPromiscuousSpecified = true;

                            _vim.AddPortGroupAsync(_net, spec).Wait();
                            _pgAllocation.Add(spec.name, new PortGroupAllocation { Net = spec.name, Counter = 1, VlanId = spec.vlanId });
                            if (_swAllocation.ContainsKey(switchName))
                                _swAllocation[switchName] += 1;

                        } catch {

                        }
                    }
                    else
                    {
                        _pgAllocation[eth.Net].Counter += 1;
                    }
                }
            }
        }
        private async Task<ObjectContent[]> LoadNetObject()
        {
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.NetworkFilter(_net));
            return response.returnval;
            // ObjectContent[] oc = response.returnval;
            // return (HostPortGroup[])oc[0].propSet[0].val;
        }

        private async Task ReloadPgCache()
        {
            string[] vmPgs = await LoadVmPortGroups(null);
            ObjectContent[] oc = await LoadNetObject();
            HostPortGroup[] pgs = (HostPortGroup[])oc[0].propSet[0].val;
            HostVirtualSwitch[] sws = (HostVirtualSwitch[])oc[0].propSet[1].val;

            lock (_pgAllocation)
            {
                _swAllocation.Clear();
                foreach (HostVirtualSwitch sw in sws)
                    if (sw.name.StartsWith("sw#") && !_swAllocation.ContainsKey(sw.name))
                        _swAllocation.Add(sw.name, 0);

                foreach(HostPortGroup pg in pgs)
                {
                    if (pg.spec.name.Contains("#") && !_pgAllocation.ContainsKey(pg.spec.name))
                        _pgAllocation.Add(pg.spec.name, new PortGroupAllocation { Net = pg.spec.name, VlanId = pg.spec.vlanId });

                    if (_swAllocation.ContainsKey(pg.spec.vswitchName))
                        _swAllocation[pg.spec.vswitchName] += 1;
                }

                foreach (string net in vmPgs)
                {
                    if (_pgAllocation.ContainsKey(net))
                        _pgAllocation[net].Counter += 1;
                }

                // TODO: Revisit this cleanup upon start if things start getting messy.
                // TODO: Need to change it to preserve pg/vs if any vm exists in the isolation group, regardless
                // of where its nics are attached.  This should prevent garbage collection of a network when the
                // vm nic is temporarily on bridge-net.

                // //cleanup any empties
                // string[] empties = _pgAllocation.Values.Where(p => p.Counter ==0).Select(p => p.Net).ToArray();
                // RemoveVmPortGroups(empties).Wait();

                // //remove empty switches
                // List<string> emptySwitches = new List<string>();
                // foreach (string key in _swAllocation.Keys)
                //     if (_swAllocation[key]==0)
                //         emptySwitches.Add(key);
                // RemoveVirtualSwitch(emptySwitches.ToArray()).Wait();

            }
        }

        private async Task<string[]> LoadVmPortGroups(ManagedObjectReference mor)
        {
            if (mor == null) mor = _vms;
            List<string> result = new List<string>();
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.VmFilter(mor, "config"));
            ObjectContent[] oc = response.returnval;
            foreach (ObjectContent obj in oc)
            {
                VirtualMachineConfigInfo config = (VirtualMachineConfigInfo)obj.propSet[0].val;
                foreach (VirtualEthernetCard card in config.hardware.device.OfType<VirtualEthernetCard>())
                {
                    result.Add(((VirtualEthernetCardNetworkBackingInfo)card.backing).deviceName);
                }
            }
            return result.ToArray();
        }

        private async Task RemoveVmPortGroups(string[] nets)
        {
            await Task.Delay(0);
            lock(_pgAllocation)
            {
                foreach (string net in nets.Distinct().ToArray())
                {
                    if (_pgAllocation.ContainsKey(net))
                    {
                        _pgAllocation[net].Counter -= 1;
                        if (_pgAllocation[net].Counter < 1)
                        {
                            //_vlanMap[_pgAllocation[net].VlanId] = false;
                            _pgAllocation.Remove(net);
                            _vim.RemovePortGroupAsync(_net, net);

                            //decrement _swAllocation[key]
                            string key = net.Tag().ToSwitchName();
                            if (_swAllocation.ContainsKey(key))
                                _swAllocation[key] -= 1;
                        }
                    }
                }
            }

        }

        private async Task RemoveVirtualSwitch(string[] keys)
        {
            await Task.Delay(0);
            lock (_swAllocation)
            {
                foreach (string key in keys)
                {
                    if (_swAllocation.ContainsKey(key) && _swAllocation[key]==0)
                    {
                        _vim.RemoveVirtualSwitchAsync(_net, key).Wait();
                        _swAllocation.Remove(key);
                    }
                }
            }
        }

        private async Task<TaskInfo> WaitForVimTask(ManagedObjectReference task)
        {
            int i = 0;
            TaskInfo info = new TaskInfo();

            //iterate the search until complete or timeout occurs
            do
            {
                //check every so often
                await Task.Delay(_pollInterval);

                info = await GetVimTaskInfo(task);

                //increment timeout counter
                i++;
                //_idle = 0;

                //check for status updates until the task is complete
            } while ((info.state == TaskInfoState.running || info.state == TaskInfoState.queued));

            //return the task info
            return info;
        }

        private async Task<TaskInfo> GetVimTaskInfo(ManagedObjectReference task)
        {
            TaskInfo info = new TaskInfo();
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.TaskFilter(task));
            ObjectContent[] oc = response.returnval;
            // if (oc != null)
            //     if (oc.Length > 0 && oc[0].propSet.Length > 0)
                    info = (TaskInfo)oc[0].propSet[0].val;
            return info;
        }

        private async Task Connect()
        {
            await Task.Delay(0);
            if (_vim != null && _vim.State == CommunicationState.Opened)
                return;

            //only want one client object created, so first one through wins
            //everyone else wait here
            lock (_config)
            {
                if (_vim != null && _vim.State == CommunicationState.Faulted)
                {
                    _logger.LogDebug($"{_config.Url} CommunicationState is Faulted.");
                    Disconnect().Wait();
                }

                if (_vim == null)
                {
                    try
                    {
                        DateTime sp = DateTime.Now;
                        _logger.LogDebug($"Instantiating client {_config.Url}...");
                        VimPortTypeClient client = new VimPortTypeClient(VimPortTypeClient.EndpointConfiguration.VimPort, _config.Url);
                        _logger.LogDebug($"client: [{client}]");
                        _logger.LogDebug($"Instantiated {_config.Host} in {DateTime.Now.Subtract(sp).TotalSeconds} seconds");

                        sp = DateTime.Now;
                        _logger.LogInformation($"Connecting to {_config.Host}...");
                        _sic = client.RetrieveServiceContentAsync(new ManagedObjectReference { type = "ServiceInstance", Value = "ServiceInstance"}).Result;
                        _props = _sic.propertyCollector;
                        _vdm = _sic.virtualDiskManager;
                        _file = _sic.fileManager;
                        _logger.LogDebug($"Connected {_config.Host} in {DateTime.Now.Subtract(sp).TotalSeconds} seconds");

                        sp = DateTime.Now;
                        _logger.LogInformation($"logging into {_config.Host}...[{_config.User}]");
                        _session = client.LoginAsync(_sic.sessionManager, _config.User, _config.Password, null).Result;
                        _logger.LogDebug($"Authenticated {_config.Host} in {DateTime.Now.Subtract(sp).TotalSeconds} seconds");

                        sp = DateTime.Now;
                        _logger.LogDebug($"Initializing {_config.Host}...");
                        InitManagedObjectReferences(client).Wait();
                        _logger.LogDebug($"Initialized {_config.Host} in {DateTime.Now.Subtract(sp).TotalSeconds} seconds");

                        _vim = client;
                        _disposing = false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(0, ex, $"Failed to connect with " + _config.Host);
                    }
                }
            }
        }

        public async Task Disconnect()
        {
            _logger.LogDebug($"Disconnecting from {this.Name}");
            _disposing = true;
            await Task.Delay(500);
            _vim.Dispose();
            _vim = null;
            _sic = null;
            _session = null;
        }

        private async Task InitManagedObjectReferences(VimPortTypeClient client)
        {
            //Verbose method
            PropertySpec prop;
            List<PropertySpec> props = new List<PropertySpec>();

            TraversalSpec trav;
            List<SelectionSpec> list = new List<SelectionSpec>();

            SelectionSpec sel;
            List<SelectionSpec> selectset = new List<SelectionSpec>();

            trav = new TraversalSpec();
            trav.name = "DatacenterTraversalSpec";
            trav.type = "Datacenter";
            trav.path = "hostFolder";

            selectset.Clear();
            sel = new SelectionSpec();
            sel.name = "FolderTraversalSpec";
            selectset.Add(sel);
            trav.selectSet = selectset.ToArray();
            list.Add(trav);

            trav = new TraversalSpec();
            trav.name = "FolderTraversalSpec";
            trav.type = "Folder";
            trav.path = "childEntity";
            selectset.Clear();
            sel = new SelectionSpec();
            sel.name = "DatacenterTraversalSpec";
            selectset.Add(sel);
            sel = new SelectionSpec();
            sel.name = "ComputeResourceTraversalSpec";
            selectset.Add(sel);
            trav.selectSet = selectset.ToArray();
            list.Add(trav);

            trav = new TraversalSpec();
            trav.name = "ComputeResourceTraversalSpec";
            trav.type = "ComputeResource";
            trav.path = "host";
            list.Add(trav);

            prop = new PropertySpec();
            prop.type = "Datacenter";
            prop.pathSet = new string[] { "vmFolder" };
            props.Add(prop);

            prop = new PropertySpec();
            prop.type = "ComputeResource";
            prop.pathSet = new string[] { "resourcePool" };
            props.Add(prop);

            prop = new PropertySpec();
            prop.type = "HostSystem";
            prop.pathSet = new string[] { "configManager", "datastoreBrowser"};
            props.Add(prop);

            ObjectSpec objectspec = new ObjectSpec();
            objectspec.obj = _sic.rootFolder;
            objectspec.selectSet = list.ToArray();

            PropertyFilterSpec filter = new PropertyFilterSpec();
            filter.propSet = props.ToArray();
            filter.objectSet = new ObjectSpec[] { objectspec };

            PropertyFilterSpec[] filters = new PropertyFilterSpec[] { filter };
            RetrievePropertiesResponse response = await client.RetrievePropertiesAsync(
                _props, filters);
            ObjectContent[] oc = response.returnval;

            _datacenter = (ManagedObjectReference)oc[0].obj;
            _vms = (ManagedObjectReference)oc[0].propSet[0].val;
            _res = (ManagedObjectReference)oc[1].obj;
            _pool = (ManagedObjectReference)oc[1].propSet[0].val;
            _dsb = (ManagedObjectReference)oc[2].propSet[1].val;

            _hcm = (HostConfigManager)oc[2].propSet[0].val;
            _net = _hcm.networkSystem;
            _opt = _hcm.advancedOption;
            _svc = _hcm.serviceSystem;
            _dt = _hcm.dateTimeSystem;
            _ds = _hcm.datastoreSystem;
            _hauth = _hcm.authenticationManager;
        }

        private async Task<Vm[]> ReloadVmCache()
        {
            List<string> existing = _vmCache.Values.Where(o=>o.Host == _config.Host).Select(o=>o.Id).ToList();
            List<Vm> list = new List<Vm>();

            //retrieve the properties specificied
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.VmFilter(_vms));

            ObjectContent[] oc = response.returnval;

            //iterate through the collection of Vm's
            foreach (ObjectContent obj in oc)
            {
                Vm vm = LoadVm(obj);

                if (vm != null)
                {
                    list.Add(vm);
                }
            }

            Vm stale = null;
            List<string> active = list.Select(o=>o.Id).ToList();
            foreach (string key in existing.Except(active))
            {
                if (_vmCache.TryRemove(key, out stale))
                {
                    _logger.LogDebug($"refreshing cache on {_config.Host} deleted vm {stale.Name}");
                }
            }

            //return an array of vm's
            return list.ToArray();
        }

        private Vm LoadVm(ObjectContent obj)
        {
            //create a new vm object
            Vm vm = new Vm();

            //iterate through the retrieved properties and set values for the appropriate types
            foreach (DynamicProperty dp in obj.propSet)
            {
                if (dp.val.GetType() == typeof(VirtualMachineRuntimeInfo))
                {
                    VirtualMachineRuntimeInfo runtime = (VirtualMachineRuntimeInfo)dp.val;
                    //vm.Question = GetQuestion(runtime);
                }

                if (dp.val.GetType() == typeof(VirtualMachineSnapshotInfo))
                {
                }

                if (dp.val.GetType() == typeof(VirtualMachineFileLayout))
                {
                    VirtualMachineFileLayout layout = (VirtualMachineFileLayout)dp.val;
                    if (layout != null && layout.disk != null && layout.disk.Length > 0 && layout.disk[0].diskFile.Length > 0)
                    {
                        //_logger.LogDebug(layout.disk[0].diskFile[0]);
                        vm.DiskPath = layout.disk[0].diskFile[0];
                    }
                }

                if (dp.val.GetType() == typeof(VirtualMachineSummary))
                {
                    try
                    {
                        VirtualMachineSummary summary = (VirtualMachineSummary)dp.val;
                        vm.Host = _config.Host;
                        //vm.HostId = _config.Id;
                        vm.Name = summary.config.name;
                        vm.Path = summary.config.vmPathName;
                        vm.Id = summary.config.uuid;
                        //vm.IpAddress = summary.guest.ipAddress;
                        //vm.Os = summary.guest.guestId;
                        vm.State = (summary.runtime.powerState == VirtualMachinePowerState.poweredOn)
                            ? VmPowerState.running
                            : VmPowerState.off;

                        //vm.IsPoweredOn = (summary.runtime.powerState == VirtualMachinePowerState.poweredOn);
                        vm.Reference = summary.vm.AsString(); //summary.vm.type + "|" + summary.vm.Value;
                        vm.Stats = String.Format("{0} | mem-{1}% cpu-{2}%", summary.overallStatus,
                            Math.Round(((float)summary.quickStats.guestMemoryUsage / (float)summary.runtime.maxMemoryUsage) * 100, 0),
                            Math.Round(((float)summary.quickStats.overallCpuUsage / (float)summary.runtime.maxCpuUsage) * 100, 0));
                        //vm.Annotations = summary.config.annotation.Lines();
                        //vm.ContextNumbers = vm.Annotations.FindOne("context").Value();
                        //vm.ContextNames = vm.Annotations.FindOne("display").Value();
                        //vm.HasGuestAgent = (vm.Annotations.FindOne("guestagent").Value() == "true");
                        vm.Question = GetQuestion(summary.runtime.question);
                        vm.Status = "deployed";
                        if (_tasks.ContainsKey(vm.Id))
                        {
                            var t = _tasks[vm.Id];
                            vm.Task = new VmTask { Name= t.Action, WhenCreated = t.WhenCreated, Progress = t.Progress };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex.Message);
                        if (!String.IsNullOrEmpty(vm.Name))
                        {
                            _logger.LogDebug(String.Format("Error refreshing VirtualMachine {0} on host {1}", vm.Name, _config.Host));
                        }
                        else
                        {
                            _logger.LogDebug(String.Format("Error refreshing host {0}", _config.Host));
                        }

                        return null;
                    }
                }
            }

            if (!vm.Id.HasValue())
            {
                _logger.LogDebug($"{this.Name} found a vm without an Id");
                return null;
            }

            _vmCache.AddOrUpdate(vm.Id, vm, (k,v) => (v = vm));

            return vm;
        }

        private VmQuestion GetQuestion(VirtualMachineQuestionInfo question)
        {
            if (question == null)
                return null;

            return new VmQuestion {
                Id = question.id,
                Prompt = question.text,
                DefaultChoice = question.choice.choiceInfo[question.choice.defaultIndex].key,
                Choices = question.choice.choiceInfo.Select(x =>
                    new VmQuestionChoice {
                        Key = x.key,
                        Label = x.label
                    })
                    .ToArray(),
            };
        }

        private async Task<Vm> GetVirtualMachine(ManagedObjectReference mor)
        {
            RetrievePropertiesResponse response = await _vim.RetrievePropertiesAsync(
                _props,
                FilterFactory.VmFilter(mor));
            ObjectContent[] oc = response.returnval;
            return (oc.Length > 0) ? LoadVm(oc[0]) : null;
        }

        private void ResolveConfigMacros()
        {
            string pattern = "{host}";
            string val = _config.Host.Split('.').First();
            _config.VmStore = _config.VmStore.Replace(pattern, val);
            _config.DiskStore = _config.DiskStore.Replace(pattern, val);
            _config.StockStore = _config.StockStore.Replace(pattern, val);
            _config.IsoStore = _config.IsoStore.Replace(pattern, val);
            _config.DisplayUrl = _config.DisplayUrl.Replace(pattern, val);
        }

        private bool _portgroupsInitialized = false;
        private async Task MonitorSession()
        {
            _logger.LogDebug($"{_config.Host}: starting cache loop");
            await Task.Delay(0);
            while (!_disposing)
            {
                try
                {
                    await Connect();
                    await ReloadVmCache();

                    //only run this after the first connection
                    if (!_portgroupsInitialized)
                    {
                        _portgroupsInitialized = true;
                        await ReloadPgCache();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, $"Failed to refresh cache for {_config.Host}");
                }
                finally
                {
                    await Task.Delay(_syncInterval);
                }
            }
            _logger.LogDebug("sessionMonitor ended.");
        }

        private async Task MonitorTasks()
        {
            _logger.LogDebug($"{_config.Host}: starting task monitor");
            while (!_disposing)
            {
                try
                {
                    foreach (string key in _tasks.Keys.ToArray())
                    {
                        var t = _tasks[key];
                        var info = await GetVimTaskInfo(t.Task);
                        Console.WriteLine("task progress: {0}", info.progress);
                        switch (info.state)
                        {
                            case TaskInfoState.error:
                                t.Progress = -1;
                                t.Action = info.description.message + " - " +
                                    info.error.localizedMessage;
                                _tasks.Remove(key);
                                break;

                            case TaskInfoState.success:
                                t.Progress = 100;
                                _tasks.Remove(key);
                                break;

                            default:
                                t.Progress = info.progress;
                                break;
                        }
                        Vm vm = _vmCache[key];
                        if (vm.Task == null)
                            vm.Task = new VmTask();
                        vm.Task.Progress = t.Progress;
                        vm.Task.Name = t.Action;
                        _vmCache.TryUpdate(key, vm, vm);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, $"Error in task monitor of {_config.Host}");
                }
                finally
                {
                    await Task.Delay(_taskMonitorInterval);
                }
            }
            _logger.LogDebug("taskMonitor ended.");
        }
    }

    internal class PortGroupAllocation
    {
        public string Net { get; set; }
        public int Counter { get; set; }
        public int VlanId { get; set; }
    }

    internal class VimHostTask
    {
        public ManagedObjectReference Task { get; set; }
        public string Action { get; set; }
        public int Progress { get; set; }
        public DateTime WhenCreated { get; set; }
    }
}