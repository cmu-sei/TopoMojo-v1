// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class ProfileController : _Controller
    {
        public ProfileController(
            UserService userService,
            IServiceProvider sp
        ) : base(sp)
        {
            _userService = userService;
        }

        private readonly UserService _userService;

        [Authorize(Roles = "Administrator")]
        [HttpGet("api/profiles")]
        [JsonExceptionFilter]
        public async Task<ActionResult<SearchResult<User>>> List(Search search)
        {
            var result = await _userService.List(search);
            return Ok(result);
        }

        [HttpGet("api/profile")]
        [JsonExceptionFilter]
        public async Task<ActionResult<User>> GetProfile()
        {
            var result = await _userService.FindByGlobalId("");
            return Ok(result);
        }

        [HttpPut("api/profile")]
        [JsonExceptionFilter]
        public async Task<IActionResult> UpdateProfile([FromBody]ChangedUser profile)
        {
            await _userService.UpdateProfile(profile);
            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("api/profile/priv")]
        [JsonExceptionFilter]
        public async Task<IActionResult> PrivilegedUpdate([FromBody]User profile)
        {
            await _userService.PrivilegedUpdate(profile);
            return Ok();

        }

        [HttpDelete("api/profile/{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            await _userService.DeleteProfile(id);
            return Ok();
        }

    }

}
