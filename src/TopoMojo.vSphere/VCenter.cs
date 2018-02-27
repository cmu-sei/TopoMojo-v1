using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.vSphere
{
    public class VCenter : IPodManager
    {
        public VCenter(
            PodConfiguration options,
            ILogger<VCenter> logger
            //ILoggerFactory mill
        )
        {
            _options = options;
            //_mill = mill;
            _logger = logger; //_mill.CreateLogger<VCenter>();
            //_hostCache = new Dictionary<string, VimHost>();
            //_affinityMap = new Dictionary<string, VimHost>();
            _vmCache = new ConcurrentDictionary<string, Vm>();

            // InitVlans();
            // _vlans = new Dictionary<string,int>();
            // foreach (Vlan vlan in _options.Vlan.Reservations)
            //     _vlans.Add(vlan.Name, vlan.Id);
            _netmgr = new NetworkManager(_options.Vlan);
            InitHost();
        }

        private readonly NetworkManager _netmgr;
        private readonly PodConfiguration _options;
        //private readonly TemplateOptions _optTemplate;
        private readonly ILogger<VCenter> _logger;
        //private readonly ILoggerFactory _mill;

        //private int _syncInterval = 20000;
        //private Dictionary<string, VimHost> _hostCache;
        //private Dictionary<string, int> _vlans;
        //private BitArray _vlanMap;
        private TemplateOptions _cachedTemplateOptions;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private int _cacheExpirationThreshold = 3;
        //private Dictionary<string, VimHost> _affinityMap;
        private ConcurrentDictionary<string, Vm> _vmCache;
        private Client _host;

        public PodConfiguration Options { get {return _options;}}

        public async Task ReloadHost(string hostname)
        {
            await InitHost();
        }

        //TODO: refactor this as InitializationProgress
        public async Task<Vm> Refresh(Template template)
        {
            string target = template.Name + "#" + template.IsolationTag;
            Vm vm = (await Find(target)).FirstOrDefault();
            if (vm == null)
            {
                vm = new Vm() { Name = target, Status = "created" };
                int progress = await VerifyDisks(template);
                if (progress == 100)
                    vm.Status = "initialized";
                else
                    if (progress >= 0)
                    {
                        vm.Task = new VmTask { Name = "initializing", Progress = progress };
                    }
            }

            //include task
            return vm;
        }
        public async Task<Vm> Deploy(Template template)
        {

            Vm[] vms = await Find(template.Name + "#" + template.IsolationTag);
            if (vms.Any())
                return vms.First();

            _logger.LogDebug("deploy: find host ");
            NormalizeTemplate(template, _options);
            _logger.LogDebug("deploy: normalized "+ template.Name);

            if (!template.Disks.IsEmpty())
            {
                bool found = await _host.FileExists(template.Disks[0].Path);
                if (!found)
                    throw new Exception("Template disks have not been prepared.");
            }

            _logger.LogDebug("deploy: reserve vlans ");
            _netmgr.ReserveVlans(template);

            _logger.LogDebug("deploy: " + template.Name + " " + _host.Name);
            return await _host.Deploy(template);
        }

        public async Task<Vm> Load(string id)
        {
            await Task.Delay(0);
            Vm vm = _vmCache.Values.Where(o=>o.Id == id).SingleOrDefault();
            CheckProgress(vm.Id);
            return vm;
        }

        private void CheckProgress(string id)
        {
            Vm vm = _vmCache.Values.Where(o=>o.Id == id).SingleOrDefault();
            if (vm.Task != null && (vm.Task.Progress < 0 || vm.Task.Progress > 99))
            {
                vm.Task = null;
                _vmCache.TryUpdate(vm.Id, vm, vm);
            }
        }

        private Vm[] CheckProgress(Vm[] vms)
        {
            foreach (Vm vm in vms)
                CheckProgress(vm.Id);
            return vms;
        }

        public async Task<Vm> Start(string id)
        {
            _logger.LogDebug("starting " + id);
            return await _host.Start(id);
        }

        public async Task<Vm> Stop(string id)
        {
            _logger.LogDebug("stopping " + id);
            return await _host.Stop(id);
        }

        public async Task<Vm> Save(string id)
        {
            _logger.LogDebug("saving " + id);
            return await _host.Save(id);
        }

        public async Task<Vm> Revert(string id)
        {
            _logger.LogDebug("reverting " + id);
            return await _host.Revert(id);
        }

        public async Task<Vm> Delete(string id)
        {
            _logger.LogDebug("deleting " + id);
            Vm vm =  await _host.Delete(id);
            return vm;
        }

        public async Task<Vm> Change(string id, KeyValuePair change)
        {
            _logger.LogDebug("changing " + id + " " + change.Key + "=" + change.Value);
            Vm vm = (await Find(id)).FirstOrDefault();
            if (vm == null)
                throw new InvalidOperationException();

            VmOptions vmo = null;
            //sanitze inputs
            if (change.Key == "iso")
            {
                vmo = await GetVmIsoOptions(vm.Name.Tag());
                if (!vmo.Iso.Contains(change.Value))
                    throw new InvalidOperationException();

                //translate display path back to actual path
                if (change.Value.StartsWith("public"))
                {
                    change.Value = _options.IsoStore + System.IO.Path.GetFileName(change.Value);
                }
                else
                {
                    change.Value = _options.DiskStore + vm.Name.Tag() + "/" + System.IO.Path.GetFileName(change.Value);
                }

            }

            if (change.Key == "net")
            {
                vmo = await GetVmNetOptions(vm.Name.Tag());
                if (!vmo.Net.Contains(change.Value))
                    throw new InvalidOperationException();
            }

            return await _host.Change(id, change);
        }

        public async Task<Vm[]> Find(string term)
        {
            await Task.Delay(0);
            IEnumerable<Vm> q = _vmCache.Values;
            if (term.HasValue())
                q =  q.Where(o=>o.Id.Contains(term) || o.Name.Contains(term));
            return CheckProgress(q.ToArray());
        }

        public async Task<int> VerifyDisks(Template template)
        {
            int progress = await _host.TaskProgress(template.Id);
            if (progress >= 0)
                return progress;

            string pattern = @"blank-(\d+)([^\.]+)";
            Match match = Regex.Match(template.Disks[0].Path, pattern);
            if (match.Success)
            {
                return 100; //show blank disk as created
            }

            NormalizeTemplate(template, _options);
            if (template.Disks.Length > 0)
            {
                _logger.LogDebug(template.Source + " " + template.Disks[0].Path);
                if (await _host.FileExists(template.Disks[0].Path))
                {
                    return 100;
                }
            }
            return -1;
        }

        public async Task<int> CreateDisks(Template template)
        {
            int progress = await VerifyDisks(template);
            if (progress < 0)
            {
                Task cloneTask = _host.CloneDisk(template.Id, template.Disks[0].Source, template.Disks[0].Path);
                progress = 0;
            }
            return progress;
        }

        public async Task<int> DeleteDisks(Template template)
        {
            int progress = await VerifyDisks(template);
            if (progress == 100)
            {
                foreach (Disk disk in template.Disks)
                {
                    //protect stock disks; only delete a disk if it is local to the topology
                    //i.e. the disk folder matches the topologyId
                    if (template.IsolationTag.HasValue() && disk.Path.Contains(template.IsolationTag))
                    {
                        Task deleteTask = _host.DeleteDisk(disk.Path);
                    }
                }
                return -1;
            }
            throw new Exception("Cannot delete disk that isn't fully created.");
        }

        public async Task<DisplayInfo> Display(string id)
        {
            DisplayInfo di = new DisplayInfo();
            Vm vm = Find(id).Result.FirstOrDefault();
            if (vm !=  null)
            {
                string ticket = "",
                    conditions = "";

                try
                {
                    ticket = await _host.GetTicket(id);
                }
                catch //(System.Exception ex)
                {
                    conditions = "needs-vm-connected";
                }

                //string[] h = host.Name.Split('.');
                di = new DisplayInfo
                {
                    Id = id,
                    Name = vm.Name.Untagged(),
                    TopoId = vm.Name.Tag(),
                    Method = _options.DisplayMethod,
                    // Url = _options.DisplayUrl.Replace("{host}", targetHost) + ticket,
                    Url = ticket,
                    Conditions = conditions
                };
            }
            return di;
        }

        public async Task<Vm> Answer(string id, VmAnswer answer)
        {
            return await _host.AnswerVmQuestion(id, answer.QuestionId, answer.ChoiceKey);
        }

        public async Task<TemplateOptions> GetTemplateOptionsFromCache(string key)
        {
            bool cacheExpired = DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes > _cacheExpirationThreshold
                || _cachedTemplateOptions == null;
            if (cacheExpired)
            {
                _cachedTemplateOptions = await GetTemplateOptions(key);
                _lastCacheUpdate = DateTime.Now;
            }
            return _cachedTemplateOptions;
        }

        public async Task<TemplateOptions> GetTemplateOptions(string key)
        {
            TemplateOptions to = new TemplateOptions();
            return await Task.FromResult(to);
        }

        public async Task<VmOptions> GetVmIsoOptions(string id)
        {
            List<string> isos = new List<string>();

            //translate actual path to display path
            isos.AddRange(
                (await _host.GetFiles(_options.IsoStore + "/*.iso", false))
                .Select(x => "public/" + System.IO.Path.GetFileName(x)).ToArray());
            isos.AddRange(
                (await _host.GetFiles(_options.DiskStore + id + "/*.iso", false))
                .Select(x => x.Replace(_options.DiskStore, "")));
                //.Select(x => System.IO.Path.GetFileName(x)));
            isos.Sort();
            return new VmOptions {
                Iso = isos.ToArray()
            };
        }

        public async Task<VmOptions> GetVmNetOptions(string id)
        {
            await Task.Delay(0);
            return new VmOptions {
                Net = _netmgr.FindNetworks(id)
            };
        }
        public string Version
        {
            get
            {
                return "TopoMojo Pod Manager for vSphere, v1.0.0";
            }
        }

        private void NormalizeTemplate(Template template, PodConfiguration option)
        {
            template.UseUplinkSwitch = true;

            if (!template.Iso.HasValue())
            {
                template.Iso = option.IsoStore + "null.iso";
            }
            else
            {
                template.Iso = (template.Iso.StartsWith("public"))
                    ? template.Iso = option.IsoStore + System.IO.Path.GetFileName(template.Iso)
                    : template.Iso = option.DiskStore + template.Iso;
                    //: template.Iso = option.DiskStore + template.IsolationTag + "/" + System.IO.Path.GetFileName(template.Iso);
            }

            // if (template.Iso.HasValue() && !template.Iso.StartsWith(option.IsoStore))
            // {
            //     template.Iso = option.IsoStore + template.Iso + ".iso";
            // }

            if (template.Source.HasValue() && !template.Source.StartsWith(option.StockStore))
            {
                //template.Source = option.StockStore + template.Source + ".vmdk";
            }

            // string prefix = option.DiskStore.Trim();
            // if (!prefix.EndsWith("]") && !prefix.EndsWith("/"))
            //     prefix += "/";

            foreach (Disk disk in template.Disks)
            {
                if (!disk.Path.StartsWith(option.DiskStore)
                    && !disk.Path.StartsWith(option.StockStore))
                {
                    DatastorePath dspath = new DatastorePath(disk.Path);
                    dspath.Merge(option.DiskStore);
                    // string folder = dspath.FolderPath;
                    // if (folder.Trim().HasValue())
                    //     folder += "/";
                    // disk.Path = String.Format("{0}{1}/{2}", prefix, folder, dspath.File);
                    disk.Path = dspath.ToString();
                }
            }

            if (template.IsolationTag.HasValue())
            {
                string tag = "#" + template.IsolationTag;
                Regex rgx = new Regex("#.*");
                if (!template.Name.EndsWith(template.IsolationTag))
                    template.Name = rgx.Replace(template.Name, "") + tag;
                foreach (Eth eth in template.Eth)
                {
                    //don't add tag if referencing a global vlan
                    if (!_netmgr.Contains(eth.Net))
                    {
                        eth.Net = rgx.Replace(eth.Net, "") + tag;
                    }
                }
            }
        }

        private async Task InitHost()
        {
            if (_host != null)
                await _host.Disconnect();

            _options.Host = new Uri(_options.Url).Host;

            _host = new Client(_options, _vmCache, _netmgr, _logger);
        }

        // private void InitVlans()
        // {
        //     //initialize vlan map
        //     _vlanMap = new BitArray(4096, true);
        //     foreach (int i in _options.Vlan.Range.ExpandRange())
        //     {
        //         _vlanMap[i] = false;
        //     }

        //     //set admin reservations
        //     _vlans = new Dictionary<string,int>();
        //     foreach (Vlan vlan in _options.Vlan.Reservations)
        //     {
        //         _vlans.Add(vlan.Name, vlan.Id);
        //         _vlanMap[vlan.Id] = true;
        //     }

        //     // UpdateVlanReservationsLoop();
        // }

        // private void ReserveVlans(Template template)
        // {
        //     lock (_vlanMap)
        //     {
        //         foreach (Eth eth in template.Eth)
        //         {
        //             //if net already reserved, use reserved vlan
        //             if (_vlans.ContainsKey(eth.Net))
        //             {
        //                 eth.Vlan = _vlans[eth.Net];
        //             }
        //             else
        //             {
        //                 int id = 0;
        //                 // if (template.UseUplinkSwitch)
        //                 // {
        //                     //get available uplink vlan
        //                     while (id < _vlanMap.Length && _vlanMap[id])
        //                     {
        //                         id += 1;
        //                     }

        //                     if (id > 0 && id < _vlanMap.Length)
        //                     {
        //                         eth.Vlan = id;
        //                         _vlanMap[id] = true;
        //                         _vlans.Add(eth.Net, id);
        //                     }
        //                     else
        //                     {
        //                         throw new Exception("Unable to reserve a vlan for " + eth.Net);
        //                     }
        //                 // }
        //                 // else {
        //                 //     //get highest vlan in this isolation group
        //                 //     id = 100;
        //                 //     foreach (string key in _vlans.Keys.Where(k => k.EndsWith(template.IsolationTag)))
        //                 //         id = Math.Max(id, _vlans[key]);
        //                 //     id += 1;
        //                 //     eth.Vlan = id;
        //                 //     _vlans.Add(eth.Net, id);
        //                 // }

        //             }
        //         }
        //     }
        // }

    }

}
