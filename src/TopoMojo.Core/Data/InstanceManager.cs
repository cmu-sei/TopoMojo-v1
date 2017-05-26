using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class InstanceManager : EntityManager<Instance>
    {
        public InstanceManager(
            IPodManager podManager,
            TopoMojoDbContext db,
            IUserResolver userResolver,
            IOptions<CoreOptions> options,
            ILoggerFactory mill
        ) : base (db, userResolver, options, mill)
        {
            _pod = podManager;
        }

        private readonly IPodManager _pod;

        public async Task<InstanceSummary> Launch(int id)
        {
            //check for active instance, return it
            InstanceMember[] instances = await _db.InstanceMembers
                .Include(m => m.Instance)
                .Where(m => m.PersonId == _user.Id)
                .ToArrayAsync();

            Instance instance = instances
                .Where(m => m.Instance.TopologyId == id)
                .Select(m => m.Instance)
                .SingleOrDefault();

            //if none, and at threshold, throw error
            if (instance == null && instances.Length >= _options.ConcurrentInstanceMaximum)
                throw new MaximumInstancesDeployedException();

            if (instance == null)
            {
                InstanceMember member = new InstanceMember {
                    PersonId = _user.Id,
                    isAdmin = true,
                    Instance = new Instance {
                        TopologyId = id,
                        GlobalId = Guid.NewGuid().ToString(),
                        WhenCreated = DateTime.UtcNow
                    }
                };
                _db.InstanceMembers.Add(member);
                await _db.SaveChangesAsync();
                instance = member.Instance;
            }

            await _db.Entry(instance).Reference(i => i.Topology).LoadAsync();

            InstanceSummary summary = new InstanceSummary
            {
                Id = instance.Id,
                WhenCreated = instance.WhenCreated.ToString(),
                Document = (instance.Topology.DocumentUrl.HasValue())
                    ? instance.Topology.DocumentUrl
                    : "/docs/" + instance.Topology.GlobalId + ".md",
                Vms = await Deploy(id, instance.GlobalId)
            };

            return summary;
        }

        private async Task<Vm[]> Deploy(int id, string tag)
        {
            Models.Template[] templates = await GetDeployableTopology(id, tag);
            List<Task<Vm>> tasks = new List<Task<Vm>>();
            foreach (Models.Template template in templates)
                tasks.Add(_pod.Deploy(template));
            Task.WaitAll(tasks.ToArray());

            return tasks.Select(t => t.Result).ToArray();
        }

        private async Task<Models.Template[]> GetDeployableTopology(int id, string tag)
        {
            List<Models.Template> result = new List<Models.Template>();
            Topology topology = await _db.Topologies
                .Include(t => t.Templates)
                    .ThenInclude(tt => tt.Template)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();

            if (topology == null)
                throw new InvalidOperationException();

            foreach (TemplateReference tref in topology.Templates)
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
            InstanceMember member = await _db.InstanceMembers
                .Include(m => m.Instance)
                .Where(m => m.InstanceId == id)
                .SingleOrDefaultAsync();

            if (member == null || member.PersonId != _user.Id || !member.isAdmin)
                throw new InvalidOperationException();

            List<Task<Vm>> tasks = new List<Task<Vm>>();
            foreach (Vm vm in await _pod.Find(member.Instance.GlobalId))
                tasks.Add(_pod.Delete(vm.Id));
            Task.WaitAll(tasks.ToArray());

            _db.Instances.Remove(member.Instance);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<InstanceMember[]> ProfileInstances()
        {
            return await _db.InstanceMembers
                .Include(m => m.Instance).ThenInclude(i => i.Topology)
                .Where(m => m.PersonId == _user.Id)
                .ToArrayAsync();
        }
    }
}
