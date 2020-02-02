// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using TopoMojo.Extensions;

namespace TopoMojo.Models
{
    public static class ModelExtensions
    {
        public static void MergeVms(this GameState state, Vm[] vms)
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
                    vs.IsRunning = vm.State == VmPowerState.Running;
                }
            }
        }
    }
}
