using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Data;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;
using TopoMojo.Data.Entities.Extensions;
using TopoMojo.Core.Models;
using TopoMojo.Core.Models.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models.Virtual;

namespace TopoMojo.Core
{
    public class GamespaceManager : EntityManager<Gamespace>
    {
        public GamespaceManager(
            IProfileRepository pr,
            IGamespaceRepository repo,
            ITopologyRepository topos,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (pr, mill, options, profileResolver)
        {
            _pod = podManager;
            _repo = repo;
            _topos = topos;
        }

        private readonly IPodManager _pod;
        private readonly IGamespaceRepository _repo;
        private readonly ITopologyRepository _topos;

        public async Task<GameState> Create(int topoId)
        {
            Gamespace[] gamespaces = await _repo.ListByProfile(Profile.Id).ToArrayAsync();

            Gamespace game = gamespaces
                .Where(m => m.TopologyId == topoId)
                .SingleOrDefault();

            if (game == null)
            {
                if (gamespaces.Length >= _options.ConcurrentInstanceMaximum)
                    throw new MaximumInstancesDeployedException();

                game = new Gamespace
                {
                    TopologyId = topoId,
                    ShareCode = Guid.NewGuid().ToString("N")
                };
                game.Players.Add(
                    new Player
                    {
                        PersonId = Profile.Id,
                        Permission = Permission.Manager,
                    }
                );
                await _repo.Add(game);
            }
            return await Deploy(game.Id);
        }

        public async Task<Models.GameState> Deploy(int id)
        {
            Gamespace gamespace = await _repo.Load(id);
            if (gamespace == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            List<Task<Vm>> tasks = new List<Task<Vm>>();
            foreach (Data.Entities.Template template in gamespace.Topology.Templates)
            {
                TemplateUtility tu = new TemplateUtility(template.Detail ?? template.Parent.Detail);
                tu.Name = template.Name;
                tu.Networks = template.Networks;
                tu.Iso = template.Iso;
                tu.IsolationTag = gamespace.GlobalId;
                tu.Id = template.Id.ToString();
                tasks.Add(_pod.Deploy(tu.AsTemplate()));
            }
            Task.WaitAll(tasks.ToArray());

            return await LoadState(gamespace, gamespace.TopologyId);
        }

        // public async Task<GameState> Launch(int topoId)
        // {
        //     //check for active instance, return it
        //     Gamespace[] gamespaces = await _repo.ListByProfile(Profile.Id).ToArrayAsync();

        //     Gamespace game = gamespaces
        //         .Where(m => m.TopologyId == topoId)
        //         .SingleOrDefault();

        //     //if none, and at threshold, throw error
        //     if (game == null && gamespaces.Length >= _options.ConcurrentInstanceMaximum)
        //         throw new MaximumInstancesDeployedException();

        //     string gameId = Guid.NewGuid().ToString();
        //     Task<Vm[]> deploy = Deploy(topoId, gameId);

        //     if (game == null)
        //     {
        //         Player player = new Player {
        //             PersonId = Profile.Id,
        //             Permission = Permission.Manager,
        //             Gamespace = new Gamespace {
        //                 TopologyId = topoId,
        //                 GlobalId = gameId,
        //                 WhenCreated = DateTime.UtcNow,
        //                 ShareCode = Guid.NewGuid().ToString()
        //             }
        //         };
        //         _db.Players.Add(player);
        //         await _db.SaveChangesAsync();
        //         game = player.Gamespace;
        //     }

        //     GameState state = new GameState
        //     {
        //         Id = game.Id,
        //         GlobalId = game.GlobalId,
        //         Document = game.Topology.Document,
        //         WhenCreated = game.WhenCreated.ToString(),
        //         ShareCode = game.ShareCode
        //     };

        //     state.AddVms(game.Topology.Linkers);
        //     Task.WaitAll(deploy);
        //     state.AddVms(deploy.Result);

        //     return state;
        // }

        public async Task<Models.GameState> Load(int id)
        {
            Gamespace gamespace = await _repo.Load(id);
            return await LoadState(gamespace, gamespace.TopologyId);
        }

        public async Task<Models.GameState> LoadFromTopo(int topoId)
        {
            Gamespace gamespace = await _repo.FindByContext(topoId, Profile.Id);
            return await LoadState(gamespace, topoId);
        }

        private async Task<GameState> LoadState(Gamespace gamespace, int topoId)
        {
            GameState state = null;

            if (gamespace == null)
            {
                Data.Entities.Topology topo = await _topos.Load(topoId);
                if (topo == null)
                    throw new InvalidOperationException();

                state = new GameState();
                state.Document = topo.Document;
                state.Vms = topo.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
            }
            else
            {
                state = Mapper.Map<Models.GameState>(gamespace);
                state.Vms = gamespace.Topology.Templates
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
                state.MergeVms(await _pod.Find(gamespace.GlobalId));
            }

            return state;
        }

        // private async Task<Vm[]> DeployMachines(Gamespace gamespace)
        // {
        //     List<Task<Vm>> tasks = new List<Task<Vm>>();
        //     foreach (Entities.Template template in gamespace.Topology.Templates)
        //     {
        //         TemplateUtility tu = new TemplateUtility(template.Detail ?? template.BaseTemplate.Detail);
        //         tu.Name = template.Name;
        //         tu.Networks = template.Networks;
        //         tu.Iso = template.Iso;
        //         tu.IsolationTag = gamespace.GlobalId;
        //         tu.Id = template.Id.ToString();
        //         tasks.Add(_pod.Deploy(tu.AsTemplate()));
        //     }
        //     Task.WaitAll(tasks.ToArray());
        //     return tasks.Select(t => t.Result).ToArray();
        // }

        // private async Task<Vm[]> Deploy(int topoId, string tag)
        // {
        //     TopoMojo.Models.Virtual.Template[] templates = await GetDeployableTemplates(topoId, tag);
        //     List<Task<Vm>> tasks = new List<Task<Vm>>();
        //     foreach (TopoMojo.Models.Virtual.Template template in templates)
        //         tasks.Add(_pod.Deploy(template));
        //     Task.WaitAll(tasks.ToArray());

        //     return tasks.Select(t => t.Result).ToArray();
        // }

        // private async Task<TopoMojo.Models.Virtual.Template[]> GetDeployableTemplates(int topoId, string tag)
        // {
        //     List<Models.Template> result = new List<Models.Template>();
        //     Topology topology = await _db.Topologies
        //         .Include(t => t.Linkers)
        //             .ThenInclude(tt => tt.Template)
        //         .Where(t => t.Id == topoId)
        //         .SingleOrDefaultAsync();

        //     if (topology == null)
        //         throw new InvalidOperationException();

        //     foreach (Linker tref in topology.Linkers)
        //     {
        //         TemplateUtility tu = new TemplateUtility(tref.Template.Detail);
        //         if (tref.Name.HasValue())
        //             tu.Name = tref.Name;

        //         if (tref.Networks.HasValue())
        //             tu.Networks = tref.Networks;

        //         if (tref.Iso.HasValue())
        //             tu.Iso = tref.Iso;

        //         tu.IsolationTag = tag.HasValue() ? tag : topology.GlobalId;
        //         tu.Id = tref.Id.ToString();
        //         result.Add(tu.AsTemplate());
        //     }
        //     return result.ToArray();
        // }

        public async Task<Models.GameState> Destroy(int id)
        {
            Gamespace gamespace = await _repo.Load(id);
            if (gamespace == null)
                throw new InvalidOperationException();

            if (! await _repo.CanEdit(id, Profile))
                throw new InvalidOperationException();

            List<Task<Vm>> tasks = new List<Task<Vm>>();
            foreach (Vm vm in await _pod.Find(gamespace.GlobalId))
                tasks.Add(_pod.Delete(vm.Id));
            Task.WaitAll(tasks.ToArray());
            //_pod.DeleteGroup(player.Gamespace.GlobalId);

            await _repo.Remove(gamespace);
            return Mapper.Map<Models.GameState>(gamespace);
        }

        public async Task<Models.GamespaceSummary[]> List()
        {
            return await _repo.ListByProfile(Profile.Id)
                .Select(g => Mapper.Map<Models.GamespaceSummary>(g))
                .ToArrayAsync();
        }

        // public async Task<Player[]> Players(int id)
        // {
        //     Player[] players = await _db.Players
        //         .Where(p => p.GamespaceId == id)
        //         .ToArrayAsync();

        //     Player player = players
        //         .Where(p => p.PersonId == Profile.Id)
        //         .SingleOrDefault();

        //     if (player == null)
        //         throw new InvalidOperationException();

        //     return players;
        // }

        // public async Task<string> Share(int id, bool revoke)
        // {
        //     Player member = await _db.Players
        //         .Include(m => m.Gamespace)
        //         .Where(m => m.GamespaceId == id)
        //         .SingleOrDefaultAsync();

        //     if (member == null || member.PersonId != Profile.Id || !member.Permission.CanManage())
        //         throw new InvalidOperationException();

        //     string code = (revoke) ? "" : Guid.NewGuid().ToString("N");
        //     member.Gamespace.ShareCode = code;
        //     await _db.SaveChangesAsync();
        //     return code;
        // }

        public async Task<bool> Enlist(string code)
        {
            Gamespace gamespace = await _repo.FindByShareCode(code);
            if (gamespace == null)
                throw new InvalidOperationException();

            if (!gamespace.Players.Where(m => m.PersonId == Profile.Id).Any())
            {
                gamespace.Players.Add(new Player
                {
                    PersonId = Profile.Id,
                });
                await _repo.Update(gamespace);
            }
            return true;
        }

        public async Task<bool> Delist(int topoId, int memberId)
        {
            Gamespace gamespace = await _repo.Load(topoId);

            if (gamespace == null)
                throw new InvalidOperationException();

            if (! await _repo.CanManage(topoId, Profile))
                throw new InvalidOperationException();

            Player member = gamespace.Players
                .Where(p => p.PersonId == memberId)
                .SingleOrDefault();

            if (member != null)
            {
                gamespace.Players.Remove(member);
                await _repo.Update(gamespace);
            }
            return true;
        }
    }
}
