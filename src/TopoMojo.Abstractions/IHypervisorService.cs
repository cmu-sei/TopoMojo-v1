// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TopoMojo.Models;

namespace TopoMojo.Abstractions
{
    public interface IHypervisorService : IHostedService
    {
        Task<Vm> Load(string id);
        Task<Vm> Start(string id);
        Task<Vm> Stop(string id);
        Task<Vm> Save(string id);
        Task<Vm> Revert(string id);
        Task<Vm> Delete(string id);
        Task StartAll(string target);
        Task StopAll(string target);
        Task DeleteAll(string target);
        Task<Vm> ChangeState(VmOperation op);
        Task<Vm> ChangeConfiguration(string id, KeyValuePair<string,string> change);
        Task<Vm> Deploy(VmTemplate template);
        Task SetAffinity(string isolationTag, Vm[] vms, bool start);
        Task<Vm> Refresh(VmTemplate template);
        Task<Vm[]> Find(string searchText);
        Task<int> CreateDisks(VmTemplate template);
        Task<int> VerifyDisks(VmTemplate template);
        Task<int> DeleteDisks(VmTemplate template);
        Task<ConsoleSummary> Display(string id);
        Task<Vm> Answer(string id, VmAnswer answer);
        // Task<TemplateOptions> GetTemplateOptions(string key);
        Task<VmOptions> GetVmIsoOptions(string key);
        Task<VmOptions> GetVmNetOptions(string key);
        string Version { get; }
        Task ReloadHost(string host);

        HypervisorServiceConfiguration Options { get; }
    }

}
