using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
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

        public async Task<string> Start(string isolationTag, GamespaceSpec spec)
        {
            string mdText = "";

            var model = new NewGamespace
            {
                Id = isolationTag,
                Workspace = spec
            };

            var result = await Client.PostAsync("", Json(model));

            if (result.IsSuccessStatusCode)
            {

                string data = await result.Content.ReadAsStringAsync();

                var game = JsonSerializer.Deserialize<GameState>(data);

                mdText = "> Gamespace Resources: " + String.Join(" | ", game.Vms.Select(v => $"[{v.Name}](/console/{v.Id}/{v.Name}/{isolationTag})"));

                if (spec.AppendMarkdown)
                {
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
            var info = JsonSerializer.Deserialize<ConsoleSummary>(data);
            return info;
        }

        public async Task ChangeVm(VmAction vmAction)
        {
            await Client.PutAsync($"vmaction", Json(vmAction));
        }

        private HttpContent Json(object obj)
        {
            return new StringContent(
                JsonSerializer.Serialize(obj),
                Encoding.UTF8,
                "application/json"
            );
        }

        public async Task BuildIso(IsoBuildSpec spec)
        {
            await Task.Delay(0);
        }

        public async Task<string> Templates(int id)
        {
            return await Client.GetStringAsync($"topo/{id}");
        }

    }

}
