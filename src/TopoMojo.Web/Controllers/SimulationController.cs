// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using TopoMojo.Abstractions;
// using TopoMojo.Core;
// using TopoMojo.Web;

// namespace TopoMojo.Controllers
// {
//     [Authorize]
//     [Route("api/[controller]/[action]")]
//     public class SimulationController : _Controller
//     {
//         public SimulationController(
//             SimulationManager simulationManager,
//             IServiceProvider sp) : base(sp)
//         {
//             _mgr = simulationManager;
//         }

//         private readonly SimulationManager _mgr;

//         [HttpGet("{id}")]
//         [JsonExceptionFilterAttribute]
//         public async Task<IActionResult> Load([FromRoute]int id)
//         {
//             return Json(await _mgr.LoadAsync(id));
//         }

//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> List([FromBody]Search<Simulation> search)
//         {
//             return Json(await _mgr.ListAsync(search));
//         }

//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> Create([FromBody]Simulation sim)
//         {
//             return Json(await _mgr.Create(sim));
//         }
//     }

// }