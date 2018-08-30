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
    public class ProfileController : _Controller
    {
        public ProfileController(
            ProfileManager profileManager,
            IServiceProvider sp
        ) : base(sp)
        {
            _mgr = profileManager;
        }

        private readonly ProfileManager _mgr;

        [Authorize(Roles = "admin")]
        [HttpGet("api/profiles")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<Profile>>> List(Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }

        [HttpGet("api/profile")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Profile>> GetProfile()
        {
            var result = await _mgr.FindByGlobalId("");
            return Ok(result);
        }

        [HttpPut("api/profile")]
        [JsonExceptionFilter]
        public async Task<IActionResult> UpdateProfile([FromBody]ChangedProfile profile)
        {
            await _mgr.UpdateProfile(profile);
            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("api/profile/priv")]
        [JsonExceptionFilter]
        public async Task<IActionResult> PrivilegedUpdate([FromBody]Profile profile)
        {
            await _mgr.PrivilegedUpdate(profile);
            return Ok();

        }
    }

}
