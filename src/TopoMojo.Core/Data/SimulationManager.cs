// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using TopoMojo.Abstractions;

// namespace TopoMojo.Core
// {
//     public class SimulationManager : EntityManager<Simulation>
//     {
//         public SimulationManager(
//             TopoMojoDbContext db,
//             IUserResolver userResolver,
//             IOptions<CoreOptions> options,
//             ILoggerFactory mill
//         ) : base (db, userResolver, options, mill)
//         {
//         }

//         public async Task<Simulation> Create(Simulation sim)
//         {
//             //TODO: enforce limit on number of sims a person can Create
//             DateTime now = DateTime.UtcNow;
//             Simulation simulation = new Simulation
//             {
//                 Name = sim.Name,
//                 Description = sim.Description,
//                 GlobalId = Guid.NewGuid().ToString(),
//                 WhenCreated = now,
//                 Topology = new Topology { Name = "Topo: "+sim.Name, GlobalId = Guid.NewGuid().ToString(), WhenCreated = now },
//                 Document = new Document { Name = "Doc: "+sim.Name, GlobalId = Guid.NewGuid().ToString(), WhenCreated = now }
//             };

//             await base.SaveAsync(simulation);
//             _db.Permissions.Add(new Permission {
//                 EntityType = EntityType.Simulation,
//                 EntityId = simulation.Id,
//                 PersonId = _user.Id,
//                 Value = PermissionFlag.Manager
//             });
//             _db.Permissions.Add(new Permission {
//                 EntityType = EntityType.Topology,
//                 EntityId = simulation.Topology.Id,
//                 PersonId = _user.Id,
//                 Value = PermissionFlag.Manager
//             });
//             _db.SaveChanges();
//             return simulation;
//         }

//         public async Task<Simulation> Update(Simulation simulation)
//         {
//             if (!(await Permission(simulation.Id)).CanEdit())
//                 throw new InvalidOperationException();

//             return await base.SaveAsync(simulation);
//         }

//         private async Task<PermissionFlag> Permission(int simId)
//         {
//             return await _db.Permissions.PermissionFor(_user.Id, simId, EntityType.Simulation);
//         }

//         public async Task<Permission[]> Members(int id)
//         {
//             if (! (await Permission(id)).CanManage())
//                 throw new InvalidOperationException();

//             return await _db.Permissions
//                 .Where(m => m.EntityId == id && m.EntityType == EntityType.Simulation)
//                 .Include(m => m.Person)
//                 .ToArrayAsync();
//         }

//         public async Task<Permission> Grant(int id, PermissionFlag flag)
//         {
//             Permission member = await _db.Permissions
//                 .Include(p => p.Person)
//                 .Where(p => p.Id == id)
//                 .FirstOrDefaultAsync();

//             if (member == null)
//                 throw new InvalidOperationException();

//             if (!(await Permission(id)).CanManage())
//                 throw new InvalidOperationException();

//             member.Value = flag;
//             _db.Attach(member);
//             _db.SaveChanges();
//             return member;
//         }
//     }
// }