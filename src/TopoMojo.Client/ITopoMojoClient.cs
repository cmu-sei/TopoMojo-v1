using System.Threading.Tasks;
using TopoMojo.Models;

namespace TopoMojo.Client
{
    public interface ITopoMojoClient
    {
        Task<string> Start(string problemId, WorkspaceSpec workspace);
        Task Stop(string problemId);
        Task<ConsoleSummary> Ticket(string vmId);
        Task ChangeVm(VmAction vmAction);
        Task BuildIso(IsoSpec spec);
        Task<string> Templates(int id);
    }

}
