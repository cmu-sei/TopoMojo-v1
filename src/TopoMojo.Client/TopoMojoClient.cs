// Copyright 2020 Carnegie Mellon University. 
// Released under a MIT (SEI) license. See LICENSE.md in the project root. 

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public async Task<WorkspaceSummary[]> List(Search search)
        {
            string qs = $"?term={search.Term}&skip={search.Skip}&take={search.Take}";

            string result = await Client.GetStringAsync("workspaces" + qs);

            var list = JsonConvert.DeserializeObject<WorkspaceSummary[]>(result);

            return list;
        }

        public async Task<GameState> Start(GamespaceSpec spec)
        {
            var model = new NewGamespace
            {
                Id = spec.IsolationId,
                Workspace = spec
            };

            var result = await Client.PostAsync("gamespace", Json(model));

            if (!result.IsSuccessStatusCode)
                throw new Exception();

            string data = await result.Content.ReadAsStringAsync();

            var game = JsonConvert.DeserializeObject<GameState>(data);


            if (spec.AppendMarkdown)
            {
                try
                {
                    string mdText = "> Gamespace Resources: " + String.Join(" | ", game.Vms.Select(v => $"[{v.Name}](/console/{v.Id}/{v.Name}/{spec.IsolationId})"));

                    data = await Client.GetStringAsync(game.WorkspaceDocument);

                    mdText += "\n\n" + data;

                    game.Markdown = mdText;
                }
                catch
                {

                }
            }

            return game;
        }

        [Obsolete]
        public async Task<string> Start(string isolationTag, GamespaceSpec spec)
        {
            string mdText = "";

            var model = new NewGamespace
            {
                Id = isolationTag,
                Workspace = spec
            };

            var result = await Client.PostAsync("gamespace", Json(model));

            if (result.IsSuccessStatusCode)
            {

                string data = await result.Content.ReadAsStringAsync();

                var game = JsonConvert.DeserializeObject<GameState>(data);

                mdText = "> Gamespace Resources: " + String.Join(" | ", game.Vms.Select(v => $"[{v.Name}](/console/{v.Id}/{v.Name}/{isolationTag})"));

                if (spec.AppendMarkdown)
                {
                    try
                    {
                        data = await Client.GetStringAsync(game.WorkspaceDocument);

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
            await Client.DeleteAsync($"gamespace/{problemId}");
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
        {
            string data = await Client.GetStringAsync($"vm-console/{vmId}");
            var info = JsonConvert.DeserializeObject<ConsoleSummary>(data);
            return info;
        }

        public async Task ChangeVm(VmAction vmAction)
        {
            await Client.PutAsync($"vm", Json(vmAction));
        }

        private HttpContent Json(object obj)
        {
            return new StringContent(
                JsonConvert.SerializeObject(obj),
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
            return await Client.GetStringAsync($"templates/{id}");
        }

    }

}
