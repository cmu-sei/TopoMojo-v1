using System;
using System.Text;
using System.Collections.Generic;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.vSphere
{
    public static class VimExtensions
    {
        public static string AsString(this ManagedObjectReference mor)
        {
            return $"{mor.type}|{mor.Value}";
        }

        public static ManagedObjectReference AsVim(this Vm vm)
        {
            string[] mor = vm.Reference.Split('|');
            return new ManagedObjectReference { type = mor[0], Value=mor[1]};
        }

        public static void AddRam(this VirtualMachineConfigSpec vmcs, int ram)
        {
            vmcs.memoryMB = (ram > 0) ? ram * 1024 : 1024;
            vmcs.memoryMBSpecified = true;
        }

        public static void AddCpu(this VirtualMachineConfigSpec vmcs, string cpu)
        {
            string[] p = cpu.Split('x');
            int sockets = 1, coresPerSocket = 1;
            if (!Int32.TryParse(p[0], out sockets))
            {
                sockets = 1;
            }

            if (p.Length > 1)
            {
                if (!Int32.TryParse(p[1], out coresPerSocket))
                {
                    coresPerSocket = 1;
                }
            }

            vmcs.numCPUs = sockets * coresPerSocket;
            vmcs.numCPUsSpecified = true;
            vmcs.numCoresPerSocket = coresPerSocket;
            vmcs.numCoresPerSocketSpecified = true;
        }

        public static void AddBootOption(this VirtualMachineConfigSpec vmcs, int delay)
        {
            if (delay != 0)
            {
                vmcs.bootOptions = new VirtualMachineBootOptions();
                if (delay > 0)
                {
                    vmcs.bootOptions.bootDelay = delay * 1000;
                    vmcs.bootOptions.bootDelaySpecified = true;
                }
                if (delay < 0)
                {
                    vmcs.bootOptions.enterBIOSSetup = true;
                    vmcs.bootOptions.enterBIOSSetupSpecified = true;
                }
            }
        }

        public static void AddGuestInfo(this VirtualMachineConfigSpec vmcs, string[] list)
        {
            List<OptionValue> options = new List<OptionValue>();
            foreach (string item in list)
            {
                OptionValue option = new OptionValue();
                int x = item.IndexOf('=');
                if (x > 0)
                {
                    option.key = item.Substring(0, x).Replace(" " , "").Trim();
                    if (!option.key.StartsWith("guestinfo."))
                        option.key = "guestinfo." + option.key;
                    option.value = item.Substring(x + 1).Trim();
                    options.Add(option);
                }
            }
            vmcs.extraConfig = options.ToArray();
        }

        public static void MergeGuestInfo(this VirtualMachineConfigSpec vmcs, string settings)
        {
            //constitue options dictionary
            //constitute settings array
            //foreach setting add/update options
            //persist result in annotation
        }
    }
}