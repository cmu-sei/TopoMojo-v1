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
    public class ProfileController : _Controller
    {
        public ProfileController(
            ProfileManager profileManager,
            IServiceProvider sp) : base(sp)
        {
            _mgr = profileManager;
        }

        private readonly ProfileManager _mgr;        

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<SearchResult<Profile>> List([FromBody]Search search)
        {
            return await _mgr.ListAsync(search);
        }        
    }

}