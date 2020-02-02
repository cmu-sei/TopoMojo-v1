using System.Threading.Tasks;
using TopoMojo.Models;

namespace TopoMojo.Abstractions
{
    public interface ITopoMojoClient
    {
        Task<string> Start(string problemId, GamespaceSpec workspace);
        Task Stop(string problemId);
        Task<ConsoleSummary> Ticket(string vmId);
        Task ChangeVm(VmAction vmAction);
        Task BuildIso(IsoBuildSpec spec);
        Task<string> Templates(int id);
    }

}
