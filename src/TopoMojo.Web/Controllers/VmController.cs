// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class VmController : _Controller
    {
        public VmController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            TemplateService templateService,
            WorkspaceService workspaceService,
            IHubContext<TopologyHub, ITopoEvent> hub,
            UserService userService,
            IHypervisorService podService,
            CoreOptions options
        ) : base(logger, identityResolver)
        {
            _templateService = templateService;
            _workspaceService = workspaceService;
            _userService = userService;
            _pod = podService;
            _hub = hub;
            _options = options;
        }

        private readonly IHypervisorService _pod;
        private readonly TemplateService _templateService;
        private readonly WorkspaceService _workspaceService;
        private readonly UserService _userService;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;
        private readonly CoreOptions _options;

        /// <summary>
        /// List vms.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("api/vms")]
        public async Task<ActionResult<Vm[]>> Find([FromQuery]string filter)
        {
            var vms = await _pod.Find(filter);

            var keys = vms.Select(v => v.Name.Tag()).Distinct().ToArray();

            var map = await _templateService.ResolveKeys(keys);

            foreach (Vm vm in vms)
                vm.GroupName = map[vm.Name.Tag()];

            return Ok(
                vms
                .OrderBy(v => v.GroupName)
                .ThenBy(v => v.Name)
                .ToArray()
            );
        }

        /// <summary>
        /// Load a vm.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/vm/{id}")]
        public async Task<ActionResult<Vm>> Load(string id)
        {
            await AuthorizeAction(id, "load");

            Vm vm = await _pod.Load(id);

            return Ok(vm);
        }

        /// <summary>
        /// Change vm state.
        /// </summary>
        /// <remarks>
        /// Operations: Start, Stop, Save, Revert
        /// </remarks>
        /// <param name="op"></param>
        /// <returns></returns>
        [HttpPut("api/vm")]
        public async Task<ActionResult<Vm>> ChangeVm([FromBody]VmOperation op)
        {
            string opType = op.Type.ToString().ToLower();

            await AuthorizeAction(op.Id, opType);

            if (
                op.Type == VmOperationType.Save
                && op.WorkspaceId > 0
                && await _workspaceService.HasGames(op.WorkspaceId)
            )
            {
                throw new WorkspaceNotIsolatedException();
            }

            Vm vm = await _pod.ChangeState(op);

            SendBroadcast(vm, opType);

            return Ok(vm);
        }

        /// <summary>
        /// Delete a vm.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <returns></returns>
        [HttpDelete("api/vm/{id}")]
        public async Task<ActionResult<Vm>> Delete(string id)
        {
            await AuthorizeAction(id, "delete");

            Vm vm = await _pod.Delete(id);

            SendBroadcast(vm, "delete");

            return Ok(vm);
        }

        /// <summary>
        /// Change vm iso or network
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <param name="change">key-value pairs</param>
        /// <returns></returns>
        [HttpPut("api/vm/{id}/change")]
        public async Task<ActionResult<Vm>> Reconfigure(string id, [FromBody] VmKeyValue change)
        {
            // need elevated privileges to change vm to special nets
            if (
                change.Key == "net" && !change.Value.Contains("#")
                && (_user.Role == UserRole.User || _user.Role == UserRole.Builder)
            )
            {
                throw new InvalidOperationException("Forbidden");
            }

            await AuthorizeAction(id, "change");

            Vm vm = (await _pod.Find(id)).FirstOrDefault();

            SendBroadcast(vm, "change");

            return Ok(await _pod.ChangeConfiguration(id, change));
        }

        /// <summary>
        /// Answer a vm question.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <param name="answer"></param>
        /// <returns></returns>
        [HttpPut("api/vm/{id}/answer")]
        public async Task<ActionResult<Vm>> Answer(string id, [FromBody] VmAnswer answer)
        {
            await AuthorizeAction(id, "answer");

            Vm vm = await _pod.Answer(id, answer);

            SendBroadcast(vm, "answer");

            return Ok(vm);
        }

        /// <summary>
        /// Find ISO files available to a vm.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <returns></returns>
        [HttpGet("api/vm/{id}/isos")]
        public async Task<ActionResult<VmOptions>> IsoOptions(string id)
        {
            await AuthorizeAction(id, "isooptions");

            Vm vm = (await _pod.Find(id)).FirstOrDefault();

            if (vm == null)
                return BadRequest();

            string tag = vm.Name.Tag();

            return Ok(await _pod.GetVmIsoOptions(tag));
        }

        /// <summary>
        /// Find virtual networks available to a vm.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <returns></returns>
        [HttpGet("api/vm/{id}/nets")]
        public async Task<ActionResult<VmOptions>> NetOptions(string id)
        {
            await AuthorizeAction(id, "netoptions");

            Vm vm = (await _pod.Find(id)).FirstOrDefault();

            if (vm == null)
                return BadRequest();

            string tag = vm.Name.Tag();

            return Ok(await _pod.GetVmNetOptions(tag));
        }

        /// <summary>
        /// Request a vm console access ticket.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/vm-console/{id}")]
        public async Task<ActionResult<ConsoleSummary>> Ticket(string id)
        {
            await AuthorizeAction(id, "ticket");

            var info = await _pod.Display(id);

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
                        target = _pod.Options.ConsoleUrl;
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

        /// <summary>
        /// Resolve a vm from a template.
        /// </summary>
        /// <param name="id">Template Id</param>
        /// <returns></returns>
        [HttpGet("api/vm-template/{id}")]
        public async Task<ActionResult<Vm>> Resolve(int id)
        {
            VmTemplate template  = await _templateService.GetDeployableTemplate(id, null);

            Vm vm = await _pod.Refresh(template);

            if (vm != null)
                await AuthorizeAction(vm, "resolve");

            return Ok(vm);
        }

        /// <summary>
        /// Deploy a vm from a template.
        /// </summary>
        /// <param name="id">Template Id</param>
        /// <returns></returns>
        [HttpPost("api/vm-template/{id}")]
        public async Task<ActionResult<Vm>> Deploy(int id)
        {
            VmTemplate template  = await _templateService.GetDeployableTemplate(id, null);

            Vm vm = await _pod.Deploy(template);

            // SendBroadcast(vm, "deploy");
            VmState state = new VmState
            {
                Id = id.ToString(),
                Name = vm.Name.Untagged(),
                IsRunning = vm.State == VmPowerState.Running
            };

            await _hub.Clients.Group(vm.Name.Tag()).VmEvent(new BroadcastEvent<VmState>(User, "VM.DEPLOY", state));

            return Ok(vm);
        }

        /// <summary>
        /// Initialize vm disks.
        /// </summary>
        /// <param name="id">Template Id</param>
        /// <returns></returns>
        [HttpPut("api/vm-template/{id}")]
        public async Task<ActionResult<Vm>> Initialize(int id)
        {
            VmTemplate template  = await _templateService.GetDeployableTemplate(id, null);

            return Ok(await _pod.CreateDisks(template));
        }

        /// <summary>
        /// Initiate hypervisor manager reload
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("api/pod/{host}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> ReloadHost(string host)
        {
            await _pod.ReloadHost(host);

            return Ok();
        }

        // This endpoint is a temporary to support an authless demo
        [HttpGet("api/vm-demo/{id}/{name}")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ConsoleSummary>> Demo([FromRoute]string id, [FromRoute]string name)
        {
            if (!_options.DemoCode.HasValue())
                throw new InvalidOperationException("Endpoint disabled.");

            if (name == "restart")
            {
                try {
                    await _pod.Stop(id);
                    await _pod.Start(id);
                }
                catch {}

                return Ok(null);
            }

            var info = await _pod.Display(id);

            if (info.Url.HasValue())
            {
                var src = new Uri(info.Url);
                info.Url = info.Url.Replace(src.Host, _options.ConsoleHost) + $"?vmhost={src.Host}";
            }
            return Ok(info);
        }

        private async Task AuthorizeAction(string id, string method)
        {
            if (_user.IsAdmin)
                return;

            Vm vm = _pod.Find(id).Result.FirstOrDefault();

            await AuthorizeAction(vm, method);
        }

        private async Task AuthorizeAction(Vm vm, string method)
        {
            string instanceId = vm?.Name.Tag();

            bool canManage = await _userService.MemberOf(instanceId);

            if (!canManage)
                throw new InvalidOperationException();

            if (!"load resolve".Contains(method))
            {
                Log(method, vm);
            }
        }

        private void SendBroadcast(Vm vm, string action)
        {
            VmState state = new VmState
            {
                Id = vm.Id,
                Name = vm.Name.Untagged(),
                IsRunning = vm.State == VmPowerState.Running
            };

            _hub.Clients.Group(vm.Name.Tag()).VmEvent(new BroadcastEvent<VmState>(User, "VM." + action.ToUpper(), state));
        }
    }
}
