using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class VmController : _Controller
    {
        public VmController(
            Core.TemplateManager templateManager,
            Core.TopologyManager topoManager,
            IHubContext<TopologyHub, ITopoEvent> hub,
            Core.ProfileManager profileManager,
            IPodManager podManager,
            IServiceProvider sp
            )
        :base(sp)
        {
            _mgr = templateManager;
            _topoMgr = topoManager;
            _profileManager = profileManager;
            _pod = podManager;
            _hub = hub;
        }

        private readonly IPodManager _pod;
        private readonly Core.TemplateManager _mgr;
        private readonly Core.TopologyManager _topoMgr;
        private readonly Core.ProfileManager _profileManager;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        // [AllowAnonymous]
        // [HttpGet("/[controller]/[action]/{id}")]
        // public async Task<ActionResult<>> Console(string id)
        // {
        //     //await HttpContext.Authentication.SignInAsync("Cookies", HttpContext.User);
        //     return View("wmks", new DisplayInfo { Id = id });
        // }

        [HttpGet("api/vm/{id}/ticket")]
        [JsonExceptionFilter]
        public async Task<ActionResult<DisplayInfo>> Ticket(string id)
        {
            await AuthorizeAction(id, "ticket");
            DisplayInfo info = await _pod.Display(id);

            if (info.Url.HasValue())
            {
                _logger.LogDebug("ticket url: {0}", info.Url);
                var src = new Uri(info.Url);
                string target = "";
                string qs = "";
                string internalHost = src.Host.Split('.').First();
                string domain = Request.Host.Value.IndexOf(".") >= 0
                            ? Request.Host.Value.Substring(Request.Host.Value.IndexOf(".")+1)
                            : Request.Host.Value;

                switch (_pod.Options.TicketUrlHandler.ToLower())
                {
                    case "querystring":
                        qs = $"?vmhost={src.Host}";
                        target = _pod.Options.DisplayUrl;
                    break;

                    case "local-app":
                        target = $"{Request.Host.Value}/{internalHost}";
                    break;

                    case "external-domain":
                        target = $"{internalHost}.{domain}";
                    break;

                    case "host-map":
                        var map = _pod.Options.TicketUrlHostMap;
                        if (map.ContainsKey(src.Host))
                            target = map[src.Host];
                    break;
                }

                if (target.HasValue())
                    info.Url = info.Url.Replace(src.Host, target);

                info.Url += qs;
            }
            return Ok(info);
        }

        [HttpGet("api/vms")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm[]>> Find(string tag)
        {
            Vm[] vms = new Vm[]{};
            if (_profile.IsAdmin) //)
            {
                vms = await _pod.Find(tag);
                var keys = vms.Select(v => v.Name.Tag()).Distinct().ToArray();
                var map = await _mgr.ResolveKeys(keys);
                foreach (Vm vm in vms)
                    vm.GroupName = map[vm.Name.Tag()];
            }
            return Ok(vms.OrderBy(v => v.GroupName).ThenBy(v => v.Name).ToArray());
        }

        [HttpGet("api/vm/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Load(string id)
        {
            await AuthorizeAction(id, "load");
            Vm vm = await _pod.Load(id);
            return Ok(vm);
        }

        [HttpPost("api/vm/action")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> ChangeVm([FromBody]VmOperation op)
        {
            string opType = op.Type.ToString().ToLower();
            await AuthorizeAction(op.Id, opType);

            if (op.Type == VmOperationType.Save && await _topoMgr.HasGames(op.WorkspaceId))
                throw new Core.WorkspaceNotIsolatedException();

            Vm vm = await _pod.ChangeState(op);
            SendBroadcast(vm, opType);
            return Ok(vm);
        }

        // [HttpPut("api/vm/{id}/start")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<Vm>> Start(string id)
        // {
        //     await AuthorizeAction(id, "start");
        //     Vm vm = await _pod.Start(id);
        //     SendBroadcast(vm, "start");
        //     return Ok(vm);
        // }

        // [HttpPut("api/vm/{id}/stop")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<Vm>> Stop(string id)
        // {
        //     await AuthorizeAction(id, "stop");
        //     Vm vm = await _pod.Stop(id);
        //     SendBroadcast(vm, "stop");
        //     return Ok(vm);
        // }

        // [HttpPut("api/vm/{id}/save/{topoId}")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<Vm>> Save(string id, int topoId)
        // {
        //     await AuthorizeAction(id, "save");
        //     Vm vm = await _pod.Save(id);
        //     SendBroadcast(vm, "save");
        //     return Ok(vm);
        // }

        // [HttpPut("api/vm/{id}/revert")]
        // [JsonExceptionFilter]
        // public async Task<ActionResult<Vm>> Revert(string id)
        // {
        //     await AuthorizeAction(id, "revert");
        //     Vm vm = await _pod.Revert(id);
        //     SendBroadcast(vm, "revert");
        //     return Ok(vm);
        // }

        [HttpDelete("api/vm/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Delete(string id)
        {
            await AuthorizeAction(id, "delete");
            Vm vm = await _pod.Delete(id);
            SendBroadcast(vm, "delete");
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/change")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Reconfigure(string id, [FromBody] KeyValuePair change)
        {
            await AuthorizeAction(id, "change");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            SendBroadcast(vm, "change");
            return Ok(await _pod.ChangeConfiguration(id, change));
        }

        [HttpPost("api/vm/{id}/answer")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Answer(string id, [FromBody] VmAnswer answer)
        {
            await AuthorizeAction(id, "answer");
            Vm vm = await _pod.Answer(id, answer);
            SendBroadcast(vm, "answer");
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/isos")]
        [JsonExceptionFilter]
        public async Task<ActionResult<VmOptions>> IsoOptions(string id)
        {
            await AuthorizeAction(id, "isooptions");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            if (vm == null)
                return BadRequest();

            string tag = vm.Name.Tag();
            return Ok(await _pod.GetVmIsoOptions(tag));
        }

        [HttpGet("api/vm/{id}/nets")]
        [JsonExceptionFilter]
        public async Task<ActionResult<VmOptions>> NetOptions(string id)
        {
            await AuthorizeAction(id, "netoptions");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            if (vm == null)
                return BadRequest();

            string tag = vm.Name.Tag();
            return Ok(await _pod.GetVmNetOptions(tag));
        }

        [HttpGet("api/template/{id}/vm")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Resolve(int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            Vm vm = await _pod.Refresh(template);
            if (vm != null)
                await AuthorizeAction(vm, "resolve");
            return Ok(vm);
        }

        [HttpPost("api/template/{id}/deploy")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Deploy(int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            Vm vm = await _pod.Deploy(template);
            SendBroadcast(vm, "deploy");
            return Ok(vm);
        }

        [HttpPost("api/template/{id}/disks")]
        [JsonExceptionFilter]
        public async Task<ActionResult<Vm>> Initialize(int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            return Ok(await _pod.CreateDisks(template));
        }

        [HttpPost("api/host/{host}/reload")]
        public async Task<ActionResult> ReloadHost(string host)
        {
            if (_profile.IsAdmin)
            {
                await _pod.ReloadHost(host);
                return Ok();
            }
            return BadRequest();
        }

        private async Task<bool> AuthorizeAction(string id, string method)
        {
            if (_profile.IsAdmin)
                return true;

            Vm vm = _pod.Find(id).Result.FirstOrDefault();
            return await AuthorizeAction(vm, method);
        }

        private async Task<bool> AuthorizeAction(Vm vm, string method)
        {
            if (vm == null)
                throw new InvalidOperationException();

            string instanceId = vm.Name.Tag();
            if (String.IsNullOrEmpty(instanceId))
                throw new InvalidOperationException();

            bool canManage = await _profileManager.CanEditSpace(instanceId);

            if (!canManage) {
                throw new InvalidOperationException();
                //_logger.LogDebug("checking if {0} can edit {1} -- {2}", _profile.Name, instanceId, result);
            }

            if (!"load resolve".Contains(method))
            {
                Log(method, vm);
                //_logger.LogInformation($"vm-action {method} {id}");
            }
            return canManage;
        }
        private void SendBroadcast(Vm vm, string action)
        {
            Core.Models.VmState state = new Core.Models.VmState
            {
                Id = vm.Id,
                Name = vm.Name.Untagged(),
                IsRunning = vm.State == VmPowerState.running
            };
            _hub.Clients.Group(vm.Name.Tag()).VmEvent(new BroadcastEvent<Core.Models.VmState>(User, "VM." + action.ToUpper(), state));
        }
    }


}
