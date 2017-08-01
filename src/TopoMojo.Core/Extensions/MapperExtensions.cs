using System;
using System.Collections.Generic;
using System.Linq;
using TopoMojo.Core;
using TopoMojo.Core.Entities;
using TopoMojo.Models;

namespace TopoMojo.Extensions
{
    public static class ModelExtensions
    {
        public static void AddVms(this GameState state, IEnumerable<Linker> templates)
        {
            state.Vms = templates
                .Select(t => new VmState
                {
                    Name = t.Name,
                    TemplateId = t.Id
                })
                .ToList();
        }

        public static void AddVms(this GameState state, Vm[] vms)
        {
            foreach (Vm vm in vms)
                {
                    string name = vm.Name.Untagged();
                    VmState vs = state.Vms
                        .Where(t => t.Name == name && !t.Id.HasValue())
                        .FirstOrDefault();

                    if (vs != null)
                    {
                        vs.Id = vm.Id;
                        vs.IsRunning = vm.State == VmPowerState.running;
                    }
                }
        }
    }
}