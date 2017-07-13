using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Entities;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class InstanceController : _Controller
    {
        public InstanceController(
            GamespaceManager instanceManager,
            IPodManager podManager,
            IServiceProvider sp) : base(sp)
        {
            _pod = podManager;
            _mgr = instanceManager;
        }

        private readonly IPodManager _pod;
        private readonly GamespaceManager _mgr;

        [HttpGetAttribute("{id}")]
        [JsonExceptionFilter]
        public async Task<GamespaceSummary> Launch([FromRoute]int id)
        {
            return await _mgr.Launch(id);
        }

        [HttpGetAttribute("{id}")]
        [JsonExceptionFilter]
        public async Task<GamespaceSummary> Check([FromRoute]int id)
        {
            return await _mgr.Check(id);
        }

        [HttpDeleteAttribute("{id}")]
        [JsonExceptionFilter]
        public async Task<bool> Destroy([FromRoute] int id)
        {
            return await _mgr.Destroy(id);
        }

        [HttpPostAttribute]
        [JsonExceptionFilterAttribute]
        public async Task<SearchResult<Gamespace>> List(Search search)
        {
            return await _mgr.ListAsync(search);
        }

        [HttpGetAttribute]
        [JsonExceptionFilterAttribute]
        public async Task<Player[]> Active()
        {
            return await _mgr.Gamespaces();
        }

    }
}