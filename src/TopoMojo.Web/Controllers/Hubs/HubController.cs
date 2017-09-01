using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Threading.Tasks;

namespace TopoMojo.Controllers
{
    public abstract class HubController<T> : _Controller
        where T : Hub
    {
        private readonly IHubContext _hub;
        public IHubConnectionContext<dynamic> Clients { get; private set; }
        public IGroupManager Groups { get; private set; }
        protected HubController(
            IConnectionManager connectionManager,
            IServiceProvider sp
        ) : base(sp) {
            _hub = connectionManager.GetHubContext<T>();
            Clients = _hub.Clients;
            Groups = _hub.Groups;
        }

        public Task Broadcast(string groupId, object model)
        {
            return Clients.Group(groupId).TopoEvent(model);
        }
    }

    public class BroadcastEvent<T> where T : class
    {
        public BroadcastEvent(
            System.Security.Principal.IPrincipal user,
            string action,
            T model
        ) {
            Actor = user.AsActor();
            Action = action;
            Model = model;
        }

        public Actor Actor { get; private set; }
        public string Action { get; set; }
        public T Model { get; set; }
    }
}