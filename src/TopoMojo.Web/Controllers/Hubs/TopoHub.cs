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
            //todo: update presence status
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
            //todo: update presence status
            return null;
        }

        public Task Listen(string channelId)
        {
            Console.WriteLine($"listen {channelId} {Context.User?.Identity.Name} {Context.ConnectionId}");
            Groups.Add(Context.ConnectionId, channelId);
            return Clients.OthersInGroup(channelId).Ping(this.Actor);
        }

        public Task Leave(string channelId)
        {
            Console.WriteLine($"leave {Context.ConnectionId} {channelId}");
            Clients.OthersInGroup(channelId).Pung(this.Actor);
            return Groups.Remove(Context.ConnectionId, channelId);
        }

        public Task Ping(string channelId)
        {
            return Clients.OthersInGroup(channelId).Ping(this.Actor);
        }

        public Task Pong(string channelId)
        {
            return Clients.OthersInGroup(channelId).Pong(this.Actor);
        }

        public Task Post(string channelId, string text)
        {
            return Clients.OthersInGroup(channelId).Posted(craftMessage(text));
        }

        public Task Typing(string channelId)
        {
            return Clients.OthersInGroup(channelId).Typing(this.Actor);
        }

        public Task Destroying(string channelId)
        {
            return Clients.OthersInGroup(channelId).Destroying(this.Actor);
        }

        private Actor Actor
        {
            get {
                return new Actor {
                    id = ((ClaimsPrincipal)Context.User).FindFirstValue(JwtClaimTypes.Subject),
                    name = Context.User.Identity.Name
                };
            }
        }

        private Message craftMessage(string text)
        {
            return new Message{
                actor = this.Actor,
                text = text
            };
        }
    }

    public interface ITopoEvent
    {
        Task Ping(Actor actor);
        Task Pong(Actor actor);
        Task Pung(Actor actor);
        Task Typing(Actor actor);
        Task Destroying(Actor actor);
        Task Posted(Message message);
        Task TopoUpdated(Topology topo);
        Task VmUpdated(Models.Vm vm);

    }

    public class Actor
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Message
    {
        public Actor actor { get; set; }
        public string text { get; set; }
    }
}