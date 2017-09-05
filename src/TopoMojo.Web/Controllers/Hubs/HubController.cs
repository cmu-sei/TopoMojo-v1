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

        public Task Broadcast(string groupId, BroadcastEvent model)
        {
            return Clients.Group(groupId).TopoEvent(model);
        }
    }

}