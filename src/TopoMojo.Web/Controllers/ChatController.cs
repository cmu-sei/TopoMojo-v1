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
    [Authorize(AuthenticationSchemes = "IdSrv,Bearer")]
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

        [HttpGet("api/chat/{id}")]
        [ProducesResponseType(typeof(Message[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromRoute]string id, [FromQuery]int marker, [FromQuery]int take = 25)
        {
            var result = await _chatService.List(id, take, marker);
            return Ok(result);
        }


        [HttpPost("api/chat")]
        [ProducesResponseType(typeof(Message), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Add([FromBody]NewMessage model)
        {
            var msg = await _chatService.Add(model);
            SendBroadcast(msg.RoomId, "added", msg.Text);
            return Ok(msg);
        }

        [HttpPut("api/chat")]
        [ProducesResponseType(typeof(Message), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Update([FromBody]ChangedMessage model)
        {
            var msg = await _chatService.Update(model);
            SendBroadcast(msg.RoomId, "updated", msg.Text);
            return Ok(msg);
        }

        [HttpDelete("api/chat/{id}")]
        [ProducesResponseType(typeof(Message), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var msg = await _chatService.Delete(id);
            SendBroadcast(msg.RoomId, "deleted", id.ToString());
            return Ok();
        }

        private void SendBroadcast(string roomId, string action, string text = "")
        {
            _hub.Clients
                .Group(roomId)
                .ChatEvent(
                    new BroadcastEvent<string>(
                        User,
                        "CHAT." + action.ToUpper(),
                        text
                    )
                );
        }
    }
}