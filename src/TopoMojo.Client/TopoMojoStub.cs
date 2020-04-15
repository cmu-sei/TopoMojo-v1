using System.Threading.Tasks;
using TopoMojo.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Client
{
    public class TopoMojoStub : ITopoMojoClient
    {
        public async Task BuildIso(IsoBuildSpec spec)
        {
            await Task.Delay(0);
        }

        public async Task ChangeVm(VmAction vmAction)
        {
            await Task.Delay(0);
        }

        public async Task<string> Start(string problemId, GamespaceSpec workspace)
        {
            await Task.Delay(0);
            return "mock gamespace markdown document";
        }

        public async Task Stop(string problemId)
        {
            await Task.Delay(0);
        }

        public async Task<string> Templates(int id)
        {
            await Task.Delay(0);
            return "mock workspace templates";
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
        {
            await Task.Delay(0);
            return new ConsoleSummary {
                Id = "12345",
                Name = "mockvm",
                Url = "/ticket/9876543210"
            };
        }
    }
}
