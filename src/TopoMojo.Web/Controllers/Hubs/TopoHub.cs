using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.SignalR;
using TopoMojo.Core.Entities;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class TopologyHub : Hub<ITopoEvent>
    {
        public override Task OnConnected()
        {
            Console.WriteLine($"connected {Context.User?.Identity.Name} {Context.ConnectionId}");
            return null;
        }
        public override Task OnReconnected()
        {
            Console.WriteLine($"reconnected  {Context.User?.Identity.Name} {Context.ConnectionId}");
            return null;
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine($"disconnected {Context.ConnectionId} {stopCalled}");
            return null;
        }

        public Task Listen(string channelId)
        {
            //Console.WriteLine($"listen {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.Add(Context.ConnectionId, channelId);
            return Clients.OthersInGroup(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.ARRIVED"));
        }

        public Task Leave(string channelId)
        {
            //Console.WriteLine($"leave {Context.ConnectionId} {channelId}");
            Clients.OthersInGroup(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.DEPARTED"));
            return Groups.Remove(Context.ConnectionId, channelId);
        }

        public Task Greet(string channelId)
        {
            //Console.WriteLine($"welcome {Context.ConnectionId} {channelId} {Context.User.AsActor().Name}");
            return Clients.OthersInGroup(channelId).PresenceEvent(new BroadcastEvent(Context.User, "PRESENCE.GREETED"));
        }

        public Task Post(string channelId, string text)
        {
            return Clients.OthersInGroup(channelId).ChatEvent(new BroadcastEvent<string>(Context.User, "CHAT.MESSAGE", text));
        }

        public Task Typing(string channelId)
        {
            return Clients.OthersInGroup(channelId).ChatEvent(new BroadcastEvent<string>(Context.User, "CHAT.TYPING", ""));
        }

        public Task Destroying(string channelId)
        {
            return Clients.OthersInGroup(channelId).TopoEvent(new BroadcastEvent<Topology>(Context.User, "TOPO.DELETED", null));
        }

        public Task TemplateMessage(string action, Linker model){
            return Clients.OthersInGroup(model.Topology.GlobalId).TemplateEvent(new BroadcastEvent<Linker>(Context.User, action, model));
        }

        // private Actor Actor
        // {
        //     get {
        //         return Context.User.AsActor();
        //     }
        // }

    }

    public interface ITopoEvent
    {
        // Task Typing(BroadcastEvent be);
        // Task Posted(BroadcastEvent<string> be);
        //Task Destroying(Actor actor);
        Task TopoEvent(BroadcastEvent<Topology> be);
        Task TemplateEvent(BroadcastEvent<Linker> be);
        Task ChatEvent(BroadcastEvent<string> be);
        Task VmEvent(BroadcastEvent<Models.Vm> be);
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

    // public class Message
    // {
    //     public Actor Actor { get; set; }
    //     public string Text { get; set; }
    // }

    // public class ActionModel
    // {
    //     public string Action { get; set; }
    //     public Actor Actor { get; set; }
    // }

    // public class ChatActionModel : ActionModel
    // {
    //     public string Text { get; set; }
    // }

    // public class TopoActionModel : ActionModel
    // {
    //     public Topology Topology { get; set; }
    // }

    // public enum TopoAction
    // {
    //     updated,
    //     shared,
    //     published,
    //     destroyed
    // }
    public static class HubExtensions
    {
        // public static ActionModel WithAction(this Actor actor, string action)
        // {
        //     return new ActionModel
        //     {
        //         Action = action,
        //         Actor = actor
        //     };
        // }

        // public static Message WithMessage(this Actor actor, string text)
        // {
        //     return new Message
        //     {
        //         Actor = actor,
        //         Text = text
        //     };
        // }

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