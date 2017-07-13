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

        public async Task<GamespaceSummary> Launch(int topoId)
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

            if (game == null)
            {
                Player player = new Player {
                    PersonId = _user.Id,
                    Permission = Permission.Manager,
                    Gamespace = new Gamespace {
                        TopologyId = topoId,
                        GlobalId = Guid.NewGuid().ToString(),
                        WhenCreated = DateTime.UtcNow
                    }
                };
                _db.Players.Add(player);
                await _db.SaveChangesAsync();
                game = player.Gamespace;
            }

            await _db.Entry(game).Reference(i => i.Topology).LoadAsync();
            await _db.Entry(game.Topology).Collection(t => t.Linkers).LoadAsync();

            GamespaceSummary summary = new GamespaceSummary
            {
                Id = game.Id,
                WhenCreated = game.WhenCreated.ToString(),
                Document = game.Topology.Document,
                VmCount = game.Topology.Linkers.Count(),
                Vms = await Deploy(topoId, game.GlobalId)
            };

            return summary;
        }

        public async Task<GamespaceSummary> Check(int topoId)
        {
            //check for active instance, return it
            Gamespace game = await _db.Players
                .Include(m => m.Gamespace)
                .Where(m => m.PersonId == _user.Id && m.Gamespace.TopologyId == topoId)
                .Select(m => m.Gamespace)
                .SingleOrDefaultAsync();

            //if none, and at threshold, throw error
            if (game == null)
            {
                Topology topo = await _db.Topologies
                    .Include(t => t.Linkers)
                    .Where(t => t.Id == topoId)
                    .SingleOrDefaultAsync();

                if (topo == null)
                    throw new InvalidOperationException();

                return new GamespaceSummary
                {
                    VmCount = topo.Linkers.Count(),
                    Document = topo.Document
                };
            }

            await _db.Entry(game).Reference(i => i.Topology).LoadAsync();
            await _db.Entry(game.Topology).Collection(t => t.Linkers).LoadAsync();

            GamespaceSummary summary = new GamespaceSummary
            {
                Id = game.Id,
                WhenCreated = game.WhenCreated.ToString(),
                Document = game.Topology.Document,
                Vms = await _pod.Find(game.GlobalId),
                VmCount = game.Topology.Linkers.Count()
            };

            return summary;
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
                .Where(m => m.GamespaceId == id)
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
