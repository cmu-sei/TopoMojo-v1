// // Copyright 2020 Carnegie Mellon University.
// // Released under a MIT (SEI) license. See LICENSE.md in the project root.

// using System;
// using System.Threading.Tasks;
// using TopoMojo.Abstractions;
// using TopoMojo.Models;

// namespace TopoMojo.Client
// {
//     public class TopoMojoStub : ITopoMojoClient
//     {
//         public async Task<WorkspaceSummary[]> List(Search search)
//         {
//             await Task.Delay(0);
//             return new WorkspaceSummary[] {};
//         }

//         public async Task BuildIso(IsoBuildSpec spec)
//         {
//             await Task.Delay(0);
//         }

//         public async Task ChangeVm(VmAction vmAction)
//         {
//             await Task.Delay(0);
//         }

//         public async Task<GameState> Start(GamespaceSpec spec)
//         {
//             await Task.Delay(0);

//             return new GameState
//             {
//                 Id = 1,
//                 Name = "Mock Gamespace",
//                 Id = spec.IsolationId,
//                 Vms = new VmState[]
//                 {
//                     new VmState
//                     {
//                         Id = Guid.NewGuid().ToString(),
//                         Name = "mock-vm",
//                         IsRunning = true
//                     }
//                 }
//             };
//         }

//         public async Task<string> Start(string problemId, GamespaceSpec workspace)
//         {
//             await Task.Delay(0);
//             return "mock gamespace markdown document";
//         }

//         public async Task Stop(string problemId)
//         {
//             await Task.Delay(0);
//         }

//         public async Task<string> Templates(int id)
//         {
//             await Task.Delay(0);
//             return "mock workspace templates";
//         }

//         public async Task<ConsoleSummary> Ticket(string vmId)
//         {
//             await Task.Delay(0);
//             return new ConsoleSummary {
//                 Id = "12345",
//                 Name = "mockvm",
//                 Url = "/ticket/9876543210"
//             };
//         }

//         public async Task<Registration> Register(GamespaceRegistration request)
//         {
//             throw new NotImplementedException();
//         }

//         public async Task<Challenge> Grade(Challenge challenge)
//         {
//             throw new NotImplementedException();
//         }

//         public async Task<Challenge> Hints(Challenge challenge)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }
