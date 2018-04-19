using System;
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

namespace TopoMojo.vMock
{
    public class PodManager : IPodManager
    {
        public PodManager(
            PodConfiguration podConfiguration,
            ILoggerFactory mill
        )
        {
            _optPod = podConfiguration;
            _mill = mill;
            _logger = _mill.CreateLogger<PodManager>();
            _vms = new Dictionary<string, Vm>();
            _tasks = new Dictionary<string, VmTask>();
            _rand = new Random();
        }

        private readonly PodConfiguration _optPod;
        private readonly ILogger<PodManager> _logger;
        private readonly ILoggerFactory _mill;
        private Random _rand;
        private Dictionary<string, Vm> _vms;
        private Dictionary<string, VmTask> _tasks;

        public PodConfiguration Options { get { return _optPod; } }

        public async Task<Vm> Refresh(Template template)
        {
            Vm vm = null;
            NormalizeTemplate(template, _optPod);
            string key = template.Name; //template.IsolationTag + "-" + template.Id;
            vm = (await Find(key)).FirstOrDefault();
            if (vm != null)
            {

                vm.Status = "deployed";
                //IncludeTask(key, vm);

            }
            else
            {
                vm = new Vm() { Name = template.Name, Status = "created" };
                if (VerifyDisks(template).Result == 100)
                    vm.Status = "initialized";
            }

            IncludeTask(key, vm);
            return vm;
        }

        private void IncludeTask(string key, Vm vm)
        {
            if (_tasks.ContainsKey(key))
            {
                VmTask task = _tasks[key];
                float elapsed = (int)DateTime.UtcNow.Subtract(task.WhenCreated).TotalSeconds;
                task.Progress = (int) Math.Min(100, (elapsed / 10) * 100);
                if (task.Progress == 100)
                {
                    _tasks.Remove(key);
                    task = null;
                }
                vm.Task = task;
            }
        }

        public async Task<Vm> Deploy(Template template)
        {
            NormalizeTemplate(template, _optPod);
            string key = template.Name;
            //string key = template.IsolationTag + "-" + template.Id;
            Vm vm = null;
            if (!_vms.ContainsKey(key))
            {
                if (template.Disks.Length > 0)
                {
                    if (template.Disks[0].Path.Contains("blank"))
                        throw new Exception("Disks have not been prepared");
                    if (VerifyDisks(template).Result != 100)
                        throw new Exception("Disks have not been prepared.");
                }
                await Delay();
                vm = new Vm
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = template.Name,
                    Path = "[mock] pod/vm",
                    Status = "deployed"
                };
                _logger.LogDebug($"deployed vm {vm.Name}");
                _vms.Add(vm.Id, vm);
            }
            else
            {
                vm = _vms[key];
                vm.Status = "deployed";
                _logger.LogDebug($"vm {vm.Name} already deployed");
            }
            return vm;
        }

        public async Task<Vm> Load(string id)
        {
            Vm vm = _vms[id];
            int test = _rand.Next(9);
            if (test == 0)
            {
                vm.Question = new VmQuestion {
                    Id = Guid.NewGuid().ToString(),
                    Prompt = "This vm has a question you must answer. Would you like to answer it?",
                    DefaultChoice = "yes",

                    Choices = new VmQuestionChoice[] {
                        new VmQuestionChoice { Key="yes", Label="Yes" },
                        new VmQuestionChoice { Key="no", Label="No" }
                    }
                };
            }
            await Delay();
            return _vms[id];
        }

        public async Task<Vm> Start(string id)
        {
            Vm vm = _vms[id];
            await Delay();
            vm.State = VmPowerState.running;
            return vm;
        }

        public async Task<Vm> Stop(string id)
        {
            Vm vm = _vms[id];
            await Delay();
            vm.State = VmPowerState.off;
            return vm;
        }

        public async Task<Vm> Save(string id)
        {
            if (_tasks.ContainsKey(id))
                throw new InvalidOperationException();

            Vm vm = _vms[id];
            vm.Task = new VmTask { Id = id, Name = "saving", WhenCreated = DateTime.UtcNow};
            _tasks.Add(vm.Name, vm.Task);
            await Delay();
            return vm;
        }

        public async Task<Vm> Revert(string id)
        {
            Vm vm = _vms[id];
            await Delay();
            vm.State = VmPowerState.off;
            return vm;
        }

        public async Task<Vm> Delete(string id)
        {
            Vm vm = _vms[id];
            await Delay();
            _vms.Remove(id);
            vm.State = VmPowerState.off;
            vm.Status = "initialized";
            return vm;
        }

        public async Task<Vm> Change(string id, KeyValuePair change)
        {
            Vm vm = _vms[id];
            await Delay();
            return vm;
        }

        public async Task<Vm[]> Find(string term)
        {
            await Task.Delay(0);
            return (term.HasValue())
            ?  _vms.Values.Where(o=> o.Id.Contains(term) || o.Name.Contains(term)).ToArray()
            : _vms.Values.ToArray();
        }

        List<MockDisk> _disks = new List<MockDisk>();
        public async Task<int> CreateDisks(Template template)
        {
            NormalizeTemplate(template, _optPod);
            string key = template.Name; //template.IsolationTag + "-" + template.Id;
            Vm vm = (await Find(key)).FirstOrDefault();
            if (vm != null)
                return 100;


            int progress = await VerifyDisks(template);
            if (progress < 0)
            {
                Disk disk = template.Disks.First();
                // if (!_tasks.ContainsKey(key))
                //     _tasks.Add(key, new VmTask {
                //         Name = "initializing",
                //         WhenCreated = DateTime.UtcNow,
                //         Id = key
                //     });
                _logger.LogDebug("disk: creating " + disk.Path);
                _disks.Add(new MockDisk
                {
                    CreatedAt = DateTime.Now,
                    Path = disk.Path,
                    Disk = disk
                });
            }
            return progress;
        }

        public async Task<int> CreateDisksOld(Template template)
        {
            int progress = await VerifyDisks(template);
            if (progress < 0)
            {
                Disk disk = template.Disks.First();
                _logger.LogDebug("disk: creating " + disk.Path);
                _disks.Add(new MockDisk
                {
                    CreatedAt = DateTime.Now,
                    Path = disk.Path,
                    Disk = disk
                });
            }
            return 0;
        }

        public async Task<int> VerifyDisks(Template template)
        {
            await Delay();

            NormalizeTemplate(template, _optPod);
            int progress = -1;
            Disk disk = template.Disks.FirstOrDefault();
            if (disk != null)
            {
                    if (disk.Path.Contains("blank-"))
                        return 100;

                MockDisk mock = _disks.FirstOrDefault(o=>o.Path == disk.Path);
                if (mock == null)
                {    _disks.Add(new MockDisk
                    {
                        CreatedAt = DateTime.Now,
                        Path = disk.Path,
                        Disk = disk
                    });
                }
                progress = 100;
                // if (mock != null)
                // {
                //     float elapsed = (int)DateTime.Now.Subtract(mock.CreatedAt).TotalSeconds;
                //     progress = (int) Math.Min(100, (elapsed / 10) * 100);
                // }
            }
            return progress;
        }


        public async Task<int> DeleteDisks(Template template)
        {
            int progress = await VerifyDisks(template);
            if (progress < 0)
                return -1;

            if (progress == 100)
            {
                Disk disk = template.Disks.First();
                MockDisk mock = _disks.FirstOrDefault(o=>o.Path == disk.Path);
                if (mock != null)
                {
                    _logger.LogDebug("disk: deleting " + disk.Path);
                    _disks.Remove(mock);
                    return -1;
                }
            }
            throw new Exception("Cannot delete disk that isn't fully created.");
        }

        public async Task<DisplayInfo> Display(string id)
        {
            await Task.Delay(0);
            return new DisplayInfo
            {
                Id = id,
                Name = _vms[id].Name.Untagged(),
                TopoId = _vms[id].Name.Tag(),
                Url = "wss://test1.internal.net/ticket/12345678"
            };
        }

        public async Task<TemplateOptions> GetTemplateOptions(string key)
        {
            // _optTemplate.Guest = new string[] {"win10", "rhel7_64", "otherLinux3"};
            // _optTemplate.Iso = new string[] {"Centos7-1503-01"};
            // _optTemplate.Source = new string[] {"centos7-10g", "blank-10g", "blank-20g"};
            // return _optTemplate;
            await Task.Delay(0);
            return new TemplateOptions();
        }
        public string Version
        {
            get
            {
                _logger.LogDebug("returning PodManager.Version");
                return "Generic Pod Manager, v17.02.13";
            }
        }

        private void NormalizeTemplate(Template template, PodConfiguration option)
        {
            if (template.Iso.HasValue() && !template.Iso.StartsWith(option.IsoStore))
            {
                template.Iso = option.IsoStore + template.Iso + ".iso";
            }

            if (template.Source.HasValue() && !template.Source.StartsWith(option.StockStore))
            {
                template.Source = option.StockStore + template.Source + ".vmdk";
            }

            foreach (Disk disk in template.Disks)
            {
                if (!disk.Path.StartsWith(option.DiskStore))
                    disk.Path = option.DiskStore + disk.Path + ".vmdk";
            }

            if (template.IsolationTag.HasValue())
            {
                string tag = "#" + template.IsolationTag;
                Regex rgx = new Regex("#.*");
                if (!template.Name.EndsWith(template.IsolationTag))
                    template.Name = rgx.Replace(template.Name, "") + tag;
                foreach (Eth eth in template.Eth)
                    eth.Net = rgx.Replace(eth.Net, "") + tag;
            }
        }

        private async Task Delay()
        {
            int x = _rand.Next(500,2500);
            Console.WriteLine($"delay: {x}");
            await Task.Delay(x);
        }

        public async Task<VmOptions> GetVmIsoOptions(string id)
        {
            await Task.Delay(0);
            VmOptions opt = new VmOptions()
            {
                Iso = new string[] { "test1.iso", "really-long-iso-name-that-needs-to-wrap-1.0.0.test2.iso" },
            };
            return opt;
        }
        public async Task<VmOptions> GetVmNetOptions(string id)
        {
            await Task.Delay(0);
            VmOptions opt = new VmOptions()
            {
                Net = new string[] { "bridge-net", "isp-att" }
            };
            return opt;
        }

        public async Task<Vm> Answer(string id, VmAnswer answer)
        {
            await Task.Delay(0);
            Vm vm = _vms[id];
            vm.Question = null;
            return vm;
        }

        public async Task ReloadHost(string host)
        {
            await Task.Delay(0);
        }
    }

    public class MockDisk
    {
        public DateTime CreatedAt { get; set; }
        public string Path { get; set; }
        public Disk Disk { get; set; }
    }
}
