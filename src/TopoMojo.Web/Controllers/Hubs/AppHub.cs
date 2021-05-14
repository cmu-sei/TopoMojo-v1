using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Data.Abstractions;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web.Services;

namespace TopoMojo.Web.Controllers
{
    public interface IHubEvent
    {
        Task GlobalEvent(BroadcastEvent<string> broadcastEvent);
        Task TopoEvent(BroadcastEvent<Workspace> broadcastEvent);
        Task TemplateEvent(BroadcastEvent<Template> broadcastEvent);
        Task ChatEvent(BroadcastEvent<Message> broadcastEvent);
        Task DocumentEvent(BroadcastEvent<object> broadcastEvent);
        Task DocumentEvent(BroadcastEvent<Document> broadcastEvent);
        Task VmEvent(BroadcastEvent<VmState> broadcastEvent);
        Task PresenceEvent(BroadcastEvent broadcastEvent);
        Task GameEvent(BroadcastEvent<GameState> broadcastEvent);
    }

    public interface IHubAction
    {
        Task Listen(string id);
        Task Leave(string id);
        Task Greet(string id);
        Task Typing(string id, bool value);
        Task TemplateMessage(string action, Template model);
    }

    [Authorize(Policy = "OneTimeTicket")]
    public class AppHub : Hub<IHubEvent>, IHubAction
    {
        public AppHub (
            ILogger<AppHub> logger,
            IUserStore userStore,
            HubCache cache
        ) {
            _logger = logger;
            _cache = cache;
            _userStore = userStore;
        }

        private readonly ILogger<AppHub> _logger;
        private readonly HubCache _cache;
        private readonly IUserStore _userStore;

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            _logger.LogDebug($"connected {Context.User.FindFirstValue("name")} {Context.UserIdentifier} {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _cache.Connections.TryRemove(Context.ConnectionId, out CachedConnection cc);
            await base.OnDisconnectedAsync(ex);
        }

        public Task Listen(string channelId)
        {

            if (! _userStore.IsMember(channelId, Context.UserIdentifier).Result)
                throw new ActionForbiddenException();

            Groups.AddToGroupAsync(Context.ConnectionId, channelId);

            _cache.Connections.TryAdd(Context.ConnectionId,
                new CachedConnection
                {
                    Id = Context.ConnectionId,
                    ProfileId = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub),
                    ProfileName = Context.User?.FindFirstValue("name"),
                    Room = channelId
                }
            );

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

        public Task Typing(string channelId, bool val)
        {
            return Clients.OthersInGroup(channelId).ChatEvent(new BroadcastEvent<Message>(Context.User, (val) ? "CHAT.TYPING" : "CHAT.IDLE", null));
        }

        public Task TemplateMessage(string action, Template model){
            return Clients.OthersInGroup(model.WorkspaceGlobalId).TemplateEvent(new BroadcastEvent<Template>(Context.User, action, model));
        }

        public Task Edited(string channelId, object textDiff)
        {
            return Clients.OthersInGroup(channelId).DocumentEvent(new BroadcastEvent<object>(Context.User, "DOCUMENT.UPDATED", textDiff));
        }

        public Task CursorChanged(string channelId, object positions)
        {
            return Clients.OthersInGroup(channelId).DocumentEvent(new BroadcastEvent<object>(Context.User, "DOCUMENT.CURSOR", positions));
        }

        public Task Editing(string channelId, bool val)
        {
            return Clients.OthersInGroup(channelId).DocumentEvent(new BroadcastEvent<Document>(Context.User, (val) ? "DOCUMENT.TYPING" : "DOCUMENT.IDLE", null));
        }

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
            var principal = user as ClaimsPrincipal;
            return new Actor
            {
                Id = principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Name = principal.FindFirstValue("name")
            };
        }
    }
}
