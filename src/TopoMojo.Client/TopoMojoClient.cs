using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TopoMojo.Models;

namespace TopoMojo.Client
{
    public class TopoMojoClient : ITopoMojoClient
    {
        ILogger Logger { get; }
        HttpClient Client { get; }

        public TopoMojoClient(
            ILogger<TopoMojoClient> logger,
            HttpClient client
        )
        {
            Logger = logger;
            Client = client;
        }

        public async Task<string> Start(string problemId, WorkspaceSpec workspace)
        {
            string mdText = "";

            var model = new NewGamespace
            {
                Id = problemId,
                Workspace = workspace
            };

            var result = await Client.PostAsync("", Json(model));

            if (result.IsSuccessStatusCode)
            {

                string data = await result.Content.ReadAsStringAsync();

                var game = JsonConvert.DeserializeObject<GameState>(data);

                mdText = "> Gamespace Resources: " + String.Join(" | ", game.Vms.Select(v => $"[{v.Name}](/console/{v.Id}/{v.Name}/{problemId})"));

                try
                {
                    // string repl = $"({Client.BaseAddress.Scheme}://{Client.BaseAddress.Host}/docs/";
                    data = await Client.GetStringAsync(game.TopologyDocument);
                    // data = data.Replace("(/docs/", repl);
                    mdText += "\n\n" + data;
                }
                catch
                {

                }

            }

            return mdText;

        }

        public async Task Stop(string problemId)
        {
            await Client.DeleteAsync(problemId);
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
        {
            string data = await Client.GetStringAsync($"ticket/{vmId}");
            var info = JsonConvert.DeserializeObject<ConsoleSummary>(data);
            return info;
        }

        public async Task ChangeVm(VmAction vmAction)
        {
            await Client.PutAsync($"vmaction", Json(vmAction));
        }

        private HttpContent Json(object obj)
        {
            return new StringContent(
                JsonConvert.SerializeObject(obj),
                Encoding.UTF8,
                "application/json"
            );
        }
        public async Task BuildIso(IsoSpec spec)
        {
            await Task.Delay(0);
        }

        public async Task<string> Templates(int id)
        {
            return await Client.GetStringAsync($"topo/{id}");
        }

    }

}
