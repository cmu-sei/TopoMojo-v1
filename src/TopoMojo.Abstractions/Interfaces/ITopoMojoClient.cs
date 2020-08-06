// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Threading.Tasks;
using TopoMojo.Models;

namespace TopoMojo.Abstractions
{
    public interface ITopoMojoClient
    {
        Task<GameState> Start(GamespaceSpec workspace);
        Task Stop(string problemId);
        Task<ConsoleSummary> Ticket(string vmId);
        Task ChangeVm(VmAction vmAction);
        Task BuildIso(IsoBuildSpec spec);
        Task<string> Templates(int id);
        Task<WorkspaceSummary[]> List(Search search);
    }

}
