using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Core.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class TopologyHub : Hub<ITopoEvent>
    {
        public TopologyHub (
            ILogger<TopologyHub> logger
        ) {
            _logger = logger;
        }
        private readonly ILogger<TopologyHub> _logger;


        public Task Listen(string channelId)
        {
            _logger.LogDebug($"listen {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.AddAsync(Context.ConnectionId, channelId);
            return Clients.Group(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.ARRIVED"));
        }

        public Task Leave(string channelId)
        {
            _logger.LogDebug($"leave {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.RemoveAsync(Context.ConnectionId, channelId);
            return Clients.Group(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.DEPARTED"));
        }

        public Task Greet(string channelId)
        {
            return Clients.Group(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.GREETED"));
        }

        public Task Post(string channelId, string text)
        {
            return Clients.Group(channelId).ChatEvent(new BroadcastEvent<string>(Context.User, "CHAT.MESSAGE", text));
        }

        public Task Typing(string channelId)
        {
            return Clients.Group(channelId).ChatEvent(new BroadcastEvent<string>(Context.User, "CHAT.TYPING", ""));
        }

        public Task Destroying(string channelId)
        {
            return Clients.Group(channelId).TopoEvent(new BroadcastEvent<Topology>(Context.User, "TOPO.DELETED", null));
        }

        public Task TemplateMessage(string action, Core.Models.Template model){
            return Clients.Group(model.TopologyGlobalId).TemplateEvent(new BroadcastEvent<Core.Models.Template>(Context.User, action, model));
        }

    }

    public interface ITopoEvent
    {
        Task TopoEvent(BroadcastEvent<Topology> be);
        Task TemplateEvent(BroadcastEvent<Core.Models.Template> be);
        Task ChatEvent(BroadcastEvent<string> be);
        Task VmEvent(BroadcastEvent<Vm> be);
        Task PresenceEvent(BroadcastEvent be);
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
                Id = ((ClaimsPrincipal)user).FindFirstValue(JwtClaimTypes.Subject),
                Name = user.Identity.Name
            };
        }
    }
}