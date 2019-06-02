// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class ChatController : _Controller
    {
        public ChatController(
            ChatService chatService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            IServiceProvider sp
        ) : base(sp)
        {
            _chatService = chatService;
            _hub = hub;
        }

        private readonly ChatService _chatService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        [HttpGet("api/chats/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Message[]>> List(string id, int marker, int take = 25)
        {
            var result = await _chatService.List(id, take, marker);
            return result;
        }

        [HttpGet("api/chat/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var result = await _chatService.Find(id);
            return result;
        }


        [HttpPost("api/chat")]
        [ProducesResponseType(201)]
        [JsonExceptionFilter]
        public async Task<ActionResult> Add([FromBody]NewMessage model)
        {
            var msg = await _chatService.Add(model);
            SendBroadcast(msg.RoomId, "added", msg);
            return CreatedAtAction(nameof(GetMessage), new { id = msg.Id }, msg);
        }

        [HttpPut("api/chat")]
        [ProducesResponseType(200)]
        [JsonExceptionFilter]
        public async Task<ActionResult> Update([FromBody]ChangedMessage model)
        {
            var msg = await _chatService.Update(model);
            SendBroadcast(msg.RoomId, "updated", msg);
            return Ok();
        }

        [HttpDelete("api/chat/{id}")]
        [ProducesResponseType(200)]
        [JsonExceptionFilter]
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
