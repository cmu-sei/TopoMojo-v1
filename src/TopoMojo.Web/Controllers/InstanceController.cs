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
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class InstanceController : _Controller
    {
        public InstanceController(
            InstanceManager instanceManager,
            IPodManager podManager,
            IServiceProvider sp) : base(sp)
        {
            _pod = podManager;
            _mgr = instanceManager;
        }

        private readonly IPodManager _pod;
        private readonly InstanceManager _mgr;

        [HttpGetAttribute("{id}")]
        [JsonExceptionFilter]
        public async Task<InstanceSummary> Launch([FromRoute]int id)
        {
            return await _mgr.Launch(id);
        }

        [HttpDeleteAttribute("{id}")]
        [JsonExceptionFilter]
        public async Task<bool> Destroy([FromRoute] int id)
        {
            return await _mgr.Destroy(id);
        }

        [HttpPostAttribute]
        [JsonExceptionFilterAttribute]
        public async Task<SearchResult<Instance>> List(Search search)
        {
            return await _mgr.ListAsync(search);
        }

        [HttpGetAttribute]
        [JsonExceptionFilterAttribute]
        public async Task<InstanceMember[]> Active()
        {
            return await _mgr.ProfileInstances();
        }

    }
}