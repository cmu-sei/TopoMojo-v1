using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;
using TopoMojo.Core.Entities.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class GamespaceManager : EntityManager<Gamespace>
    {
        public GamespaceManager(
            TopoMojoDbContext db,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (db, mill, options, profileResolver)
        {
            _pod = podManager;
        }

        private readonly IPodManager _pod;

        public async Task<GameState> Launch(int topoId)
        {
            //check for active instance, return it
            Player[] gamespaces = await _db.Players
                .Include(m => m.Gamespace)
                .Where(m => m.PersonId == _user.Id)
                .ToArrayAsync();

            Gamespace game = gamespaces
                .Where(m => m.Gamespace.TopologyId == topoId)
                .Select(m => m.Gamespace)
                .SingleOrDefault();

            //if none, and at threshold, throw error
            if (game == null && gamespaces.Length >= _options.ConcurrentInstanceMaximum)
                throw new MaximumInstancesDeployedException();

            string gameId = Guid.NewGuid().ToString();
            Task<Vm[]> deploy = Deploy(topoId, gameId);

            if (game == null)
            {
                Player player = new Player {
                    PersonId = _user.Id,
                    Permission = Permission.Manager,
                    Gamespace = new Gamespace {
                        TopologyId = topoId,
                        GlobalId = gameId,
                        WhenCreated = DateTime.UtcNow,
                        ShareCode = Guid.NewGuid().ToString()
                    }
                };
                _db.Players.Add(player);
                await _db.SaveChangesAsync();
                game = player.Gamespace;
            }

            GameState state = new GameState
            {
                Id = game.Id,
                GlobalId = game.GlobalId,
                Document = game.Topology.Document,
                WhenCreated = game.WhenCreated.ToString(),
                ShareCode = game.ShareCode
            };

            state.AddVms(game.Topology.Linkers);
            Task.WaitAll(deploy);
            state.AddVms(deploy.Result);

            return state;
        }

        public async Task<GameState> Check(int topoId)
        {
            Topology topo = await _db.Topologies
                .Include(t => t.Linkers)
                .Where(t => t.Id == topoId)
                .SingleOrDefaultAsync();

            if (topo == null)
                throw new InvalidOperationException();

            GameState state = new GameState
            {
                Document = topo.Document,
            };
            state.AddVms(topo.Linkers);

            //check for active instance, return it
            Gamespace game = await _db.Players
                .Include(m => m.Gamespace)
                .Where(m => m.PersonId == _user.Id && m.Gamespace.TopologyId == topoId)
                .Select(m => m.Gamespace)
                .SingleOrDefaultAsync();

            if (game != null)
            {
                state.Id = game.Id;
                state.GlobalId = game.GlobalId;
                state.WhenCreated = game.WhenCreated.ToString();
                state.ShareCode = game.ShareCode;
                Vm[] vms = await _pod.Find(game.GlobalId);
                state.AddVms(vms);
            }

            return state;
        }


        private async Task<Vm[]> Deploy(int topoId, string tag)
        {
            Models.Template[] templates = await GetDeployableTemplates(topoId, tag);
            List<Task<Vm>> tasks = new List<Task<Vm>>();
            foreach (Models.Template template in templates)
                tasks.Add(_pod.Deploy(template));
            Task.WaitAll(tasks.ToArray());

            return tasks.Select(t => t.Result).ToArray();
        }

        private async Task<Models.Template[]> GetDeployableTemplates(int topoId, string tag)
        {
            List<Models.Template> result = new List<Models.Template>();
            Topology topology = await _db.Topologies
                .Include(t => t.Linkers)
                    .ThenInclude(tt => tt.Template)
                .Where(t => t.Id == topoId)
                .SingleOrDefaultAsync();

            if (topology == null)
                throw new InvalidOperationException();

            foreach (Linker tref in topology.Linkers)
            {
                TemplateUtility tu = new TemplateUtility(tref.Template.Detail);
                if (tref.Name.HasValue())
                    tu.Name = tref.Name;

                if (tref.Networks.HasValue())
                    tu.Networks = tref.Networks;

                if (tref.Iso.HasValue())
                    tu.Iso = tref.Iso;

                tu.IsolationTag = tag.HasValue() ? tag : topology.GlobalId;
                tu.Id = tref.Id.ToString();
                result.Add(tu.AsTemplate());
            }
            return result.ToArray();
        }

        public async Task<bool> Destroy(int id)
        {
            Player player = await _db.Players
                .Include(m => m.Gamespace)
                .Where(m => m.GamespaceId == id && m.PersonId == _user.Id)
                .SingleOrDefaultAsync();

            if (player == null || player.PersonId != _user.Id || !player.Permission.CanManage())
                throw new InvalidOperationException();

            List<Task<Vm>> tasks = new List<Task<Vm>>();
            foreach (Vm vm in await _pod.Find(player.Gamespace.GlobalId))
                tasks.Add(_pod.Delete(vm.Id));
            Task.WaitAll(tasks.ToArray());
            //_pod.DeleteGroup(player.Gamespace.GlobalId);

            _db.Gamespaces.Remove(player.Gamespace);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Player[]> Gamespaces()
        {
            return await _db.Players
                .Include(m => m.Gamespace).ThenInclude(i => i.Topology)
                .Where(m => m.PersonId == _user.Id)
                .ToArrayAsync();
        }

        public async Task<Player[]> Players(int id)
        {
            Player[] players = await _db.Players
                .Where(p => p.GamespaceId == id)
                .ToArrayAsync();

            Player player = players
                .Where(p => p.PersonId == _user.Id)
                .SingleOrDefault();

            if (player == null)
                throw new InvalidOperationException();

            return players;
        }

        public async Task<string> Share(int id, bool revoke)
        {
            Player member = await _db.Players
                .Include(m => m.Gamespace)
                .Where(m => m.GamespaceId == id)
                .SingleOrDefaultAsync();

            if (member == null || member.PersonId != _user.Id || !member.Permission.CanManage())
                throw new InvalidOperationException();

            string code = (revoke) ? "" : Guid.NewGuid().ToString("N");
            member.Gamespace.ShareCode = code;
            await _db.SaveChangesAsync();
            return code;
        }

        public async Task<bool> Enlist(string code)
        {
            Gamespace gamespace = await _db.Gamespaces
                .Include(i => i.Players)
                .Where(i => i.ShareCode == code)
                .SingleOrDefaultAsync();

            if (gamespace == null)
                throw new InvalidOperationException();

            if (!gamespace.Players.Where(m => m.PersonId == _user.Id).Any())
            {
                gamespace.Players.Add(new Player
                {
                    PersonId = _user.Id,
                });
                await _db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> Delist(int topoId, int memberId)
        {
            Gamespace gamespace = await _db.Gamespaces
                .Include(t => t.Players)
                .Where(t => t.Id == topoId)
                .SingleOrDefaultAsync();

            Player actor = gamespace.Players
                .Where(p => p.PersonId == _user.Id)
                .SingleOrDefault();

            Player target = gamespace.Players
                .Where(p => p.PersonId == memberId)
                .SingleOrDefault();

            if (actor == null || !actor.Permission.CanManage() || target == null)
                throw new InvalidOperationException();

            gamespace.Players.Remove(target);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
