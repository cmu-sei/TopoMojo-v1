// // Copyright 2020 Carnegie Mellon University.
// // Released under a MIT (SEI) license. See LICENSE.md in the project root.

// using System;
// using System.Linq;
// using System.Net.Http;
// using System.Text;
// using System.Threading.Tasks;
// using System.Web;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using TopoMojo.Abstractions;
// using TopoMojo.Models;

// namespace TopoMojo.Client
// {
//     public class TopoMojoClient : ITopoMojoClient
//     {
//         ILogger Logger { get; }
//         HttpClient Client { get; }

//         public TopoMojoClient(
//             ILogger<TopoMojoClient> logger,
//             HttpClient client
//         )
//         {
//             Logger = logger;
//             Client = client;
//         }

//         public async Task<WorkspaceSummary[]> List(Search search)
//         {
//             string qs = $"?term={search.Term}&skip={search.Skip}&take={search.Take}";

//             string result = await Client.GetStringAsync("workspaces" + qs);

//             var list = JsonConvert.DeserializeObject<WorkspaceSummary[]>(result);

//             return list;
//         }

//         public async Task<GameState> Start(GamespaceSpec spec)
//         {

//             var result = await Client.PostAsync("gamespace", Json(spec));

//             if (!result.IsSuccessStatusCode)
//                 throw new Exception();

//             string data = await result.Content.ReadAsStringAsync();

//             var game = JsonConvert.DeserializeObject<GameState>(data);


//             if (spec.AppendMarkdown)
//             {
//                 try
//                 {
//                     data = await Client.GetStringAsync(game.WorkspaceDocument);

//                     game.Markdown = data;
//                 }
//                 catch
//                 {

//                 }
//             }

//             return game;
//         }

//         public async Task Stop(string problemId)
//         {
//             await Client.DeleteAsync($"gamespace/{problemId}");
//         }

//         public async Task<ConsoleSummary> Ticket(string vmId)
//         {
//             string data = await Client.GetStringAsync(
//                 HttpUtility.UrlEncode($"vm-console/{vmId}")
//             );
//             var info = JsonConvert.DeserializeObject<ConsoleSummary>(data);
//             return info;
//         }

//         public async Task ChangeVm(VmAction vmAction)
//         {
//             await Client.PutAsync($"vm", Json(vmAction));
//         }

//         private HttpContent Json(object obj)
//         {
//             return new StringContent(
//                 JsonConvert.SerializeObject(obj),
//                 Encoding.UTF8,
//                 "application/json"
//             );
//         }

//         public async Task BuildIso(IsoBuildSpec spec)
//         {
//             await Task.Delay(0);
//         }

//         public async Task<string> Templates(int id)
//         {
//             return await Client.GetStringAsync($"templates/{id}");
//         }

//         public async Task<Registration> Register(GamespaceRegistration request)
//         {
//             var result = await Client.PostAsync("register", Json(request));

//             if (!result.IsSuccessStatusCode)
//                 throw new Exception();

//             string data = await result.Content.ReadAsStringAsync();

//             var registration = JsonConvert.DeserializeObject<Registration>(data);

//             return registration;
//         }

//         public async Task<Challenge> Grade(Challenge challenge)
//         {
//             var result = await Client.PostAsync("grade", Json(challenge));

//             if (!result.IsSuccessStatusCode)
//                 throw new Exception();

//             string data = await result.Content.ReadAsStringAsync();

//             var graded = JsonConvert.DeserializeObject<Challenge>(data);

//             return graded;
//         }

//         public async Task<Challenge> Hints(Challenge challenge)
//         {
//             var result = await Client.PostAsync("hints", Json(challenge));

//             if (!result.IsSuccessStatusCode)
//                 throw new Exception();

//             string data = await result.Content.ReadAsStringAsync();

//             var hints = JsonConvert.DeserializeObject<Challenge>(data);

//             return hints;
//         }
//     }

// }
