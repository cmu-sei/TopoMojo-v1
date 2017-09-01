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
            Console.WriteLine($"listen {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.Add(Context.ConnectionId, channelId);
            return Clients.OthersInGroup(channelId).PresenceEvent(this.Actor.WithAction("PRESENCE.ARRIVED"));

            //return Clients.OthersInGroup(channelId).Ping(this.Actor);
        }

        public Task Leave(string channelId)
        {
            Console.WriteLine($"leave {Context.ConnectionId} {channelId}");
            //Clients.OthersInGroup(channelId).Pung(this.Actor);
            Clients.OthersInGroup(channelId).PresenceEvent(this.Actor.WithAction("PRESENCE.DEPARTED"));
            return Groups.Remove(Context.ConnectionId, channelId);
        }

        // public Task Ping(string channelId)
        // {
        //     return Clients.OthersInGroup(channelId).Ping(this.Actor);
        // }

        public Task Greet(string channelId)
        {
            Console.WriteLine($"welcome {Context.ConnectionId} {channelId} {Context.User.AsActor().Name}");
            return Clients.OthersInGroup(channelId).PresenceEvent(this.Actor.WithAction("PRESENCE.GREETED"));
        }

        // public Task Pong(string channelId)
        // {
        //     return Clients.OthersInGroup(channelId).Pong(this.Actor);
        // }

        public Task Post(string channelId, string text)
        {
            //return Clients.OthersInGroup(channelId).Posted(craftMessage(text));
            return Clients.OthersInGroup(channelId).Posted(this.Actor.WithMessage(text));
        }

        public Task Typing(string channelId)
        {
            return Clients.OthersInGroup(channelId).Typing(this.Actor);
        }

        public Task Destroying(string channelId)
        {
            return Clients.OthersInGroup(channelId).Destroying(this.Actor);
        }

        public Task SendTopoEvent(Topology topology, string action){
            return Clients.OthersInGroup(topology.GlobalId).TopoEvent(new TopoActionModel
            {
                Action = action,
                Actor = this.Actor,
                Topology = topology
            });
        }

        private Actor Actor
        {
            get {
                return Context.User.AsActor();
            }
        }

        private Message craftMessage(string text)
        {
            return new Message{
                Actor = this.Actor,
                Text = text
            };
        }

    }

    public interface ITopoEvent
    {
        // Task Ping(Actor actor);
        // Task Pong(Actor actor);
        // Task Pung(Actor actor);
        Task Typing(Actor actor);
        Task Posted(Message message);
        Task Destroying(Actor actor);
        Task TopoEvent(TopoActionModel model);
        Task ChatEvent(ActionModel model);
        Task VmEvent(Models.Vm vm);
        Task PresenceEvent(ActionModel model);

    }

    public class Actor
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Message
    {
        public Actor Actor { get; set; }
        public string Text { get; set; }
    }

    public class ActionModel
    {
        public string Action { get; set; }
        public Actor Actor { get; set; }
    }

    public class ChatActionModel : ActionModel
    {
        public string Text { get; set; }
    }

    public class TopoActionModel : ActionModel
    {
        public Topology Topology { get; set; }
    }

    public enum TopoAction
    {
        updated,
        shared,
        published,
        destroyed
    }
    public static class HubExtensions
    {
        public static ActionModel WithAction(this Actor actor, string action)
        {
            return new ActionModel
            {
                Action = action,
                Actor = actor
            };
        }

        public static Message WithMessage(this Actor actor, string text)
        {
            return new Message
            {
                Actor = actor,
                Text = text
            };
        }

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