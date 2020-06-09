// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Models;
using TopoMojo.Web.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize(Policy = "OneTimeTicket")]
    public class TopologyHub : Hub<ITopoEvent>
    {
        public TopologyHub (
            ILogger<TopologyHub> logger,
            HubCache cache
        ) {
            _logger = logger;
            _cache = cache;
        }
        private readonly ILogger<TopologyHub> _logger;
        private readonly HubCache _cache;

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _cache.Connections.TryRemove(Context.ConnectionId, out CachedConnection cc);
            await base.OnDisconnectedAsync(ex);
        }

        public Task Listen(string channelId)
        {
            _logger.LogDebug($"listen {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            var cc = new CachedConnection
            {
                Id = Context.ConnectionId,
                ProfileId = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub),
                ProfileName = Context.User?.FindFirstValue("name"),
                Room = channelId
            };
            _cache.Connections.TryAdd(cc.Id, cc);
            return Clients.OthersInGroup(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.ARRIVED"));
        }

        public Task Leave(string channelId)
        {
            _logger.LogDebug($"leave {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            _cache.Connections.TryRemove(Context.ConnectionId, out CachedConnection cc);
            return Clients.OthersInGroup(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.DEPARTED"));
        }

        public Task Greet(string channelId)
        {
            return Clients.OthersInGroup(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.GREETED"));
        }

        // public Task Post(string channelId, string text)
        // {
        //     return Clients.Group(channelId).ChatEvent(new BroadcastEvent<string>(Context.User, "CHAT.ADDED", text));
        // }

        public Task Typing(string channelId, bool val)
        {
            return Clients.OthersInGroup(channelId).ChatEvent(new BroadcastEvent<Message>(Context.User, (val) ? "CHAT.TYPING" : "CHAT.IDLE", null));
        }

        // public Task Typed(string channelId)
        // {
        //     return Clients.Group(channelId).ChatEvent(new BroadcastEvent<string>(Context.User, "CHAT.TYPED", ""));
        // }

        // public Task Destroying(string channelId)
        // {
        //     return Clients.Group(channelId).TopoEvent(new BroadcastEvent<Topology>(Context.User, "TOPO.DELETED", null));
        // }

        public Task TemplateMessage(string action, Template model){
            return Clients.OthersInGroup(model.WorkspaceGlobalId).TemplateEvent(new BroadcastEvent<Template>(Context.User, action, model));
        }

    }

    public interface ITopoEvent
    {
        Task GlobalEvent(BroadcastEvent<string> broadcastEvent);
        Task TopoEvent(BroadcastEvent<Workspace> broadcastEvent);
        Task TemplateEvent(BroadcastEvent<Template> broadcastEvent);
        Task ChatEvent(BroadcastEvent<Message> broadcastEvent);
        Task VmEvent(BroadcastEvent<VmState> broadcastEvent);
        Task PresenceEvent(BroadcastEvent broadcastEvent);
        Task GameEvent(BroadcastEvent<GameState> broadcastEvent);
    }

    public class BroadcastEvent
    {
        public BroadcastEvent(
            System.Security.Principal.IPrincipal user,
            string action
        ) {
            Actor = user.AsActor();
            Action = action;
        }

        public Actor Actor { get; private set; }
        public string Action { get; set; }
    }

    public class BroadcastEvent<T> : BroadcastEvent where T : class
    {
        public BroadcastEvent(
            System.Security.Principal.IPrincipal user,
            string action,
            T model
        ) : base(user, action)
        {
            Model = model;
        }

        public T Model { get; set; }
    }

    public class Actor
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public static class HubExtensions
    {
        public static Actor AsActor(this System.Security.Principal.IPrincipal user)
        {
            return new Actor
            {
                Id = ((ClaimsPrincipal)user).FindFirstValue(JwtRegisteredClaimNames.Sub),
                Name = user.Identity.Name
            };
        }
    }
}
