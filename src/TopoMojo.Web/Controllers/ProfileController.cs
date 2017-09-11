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
using TopoMojo.Core.Models;
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

        [HttpGet("api/profiles")]
        [ProducesResponseType(typeof(SearchResult<Profile>), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromQuery]Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }
    }

}