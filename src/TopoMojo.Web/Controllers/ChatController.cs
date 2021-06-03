// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ChatController : _Controller
    {
        public ChatController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            ChatService chatService,
            IHubContext<AppHub, IHubEvent> hub
        ) : base(logger, identityResolver)
        {
            _chatService = chatService;
            _hub = hub;
        }

        private readonly ChatService _chatService;
        private readonly IHubContext<AppHub, IHubEvent> _hub;

        [HttpGet("api/chats/{id}")]
        public async Task<ActionResult<Message[]>> List(string id, int marker, int take = 25)
        {
            var result = await _chatService.List(id, take, marker);
            return result;
        }

        [HttpGet("api/chat/{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var result = await _chatService.Find(id);
            return result;
        }

        [HttpPost("api/chat")]
        [ProducesResponseType(201)]
        public async Task<ActionResult> Add([FromBody]NewMessage model)
        {
            var msg = await _chatService.Add(model);
            SendBroadcast(msg.RoomId, "added", msg);
            return CreatedAtAction(nameof(GetMessage), new { id = msg.Id }, msg);
        }

        [HttpPut("api/chat")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Update([FromBody]ChangedMessage model)
        {
            var msg = await _chatService.Update(model);
            SendBroadcast(msg.RoomId, "updated", msg);
            return Ok();
        }

        [HttpDelete("api/chat/{id}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Delete([FromRoute]int id)
        {
            var msg = await _chatService.Delete(id);
            SendBroadcast(msg.RoomId, "deleted", msg);
            return Ok();
        }

        private void SendBroadcast(string roomId, string action, Message msg)
        {
            _hub.Clients
                .Group(roomId)
                .ChatEvent(
                    new BroadcastEvent<Message>(
                        User,
                        "CHAT." + action.ToUpper(),
                        msg
                    )
                );
        }
    }
}
