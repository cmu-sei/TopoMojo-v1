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
using TopoMojo.Hubs;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize(Policy = "Players")]
    [ApiController]
    public class VmController : _Controller
    {
        public VmController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            TemplateService templateService,
            IHubContext<AppHub, IHubEvent> hub,
            UserService userService,
            IHypervisorService podService,
            CoreOptions options
        ) : base(logger, identityResolver)
        {
            _templateService = templateService;
            _userService = userService;
            _pod = podService;
            _hub = hub;
            _options = options;
        }

        private readonly IHypervisorService _pod;
        private readonly TemplateService _templateService;
        private readonly UserService _userService;
        private readonly IHubContext<AppHub, IHubEvent> _hub;
        private readonly CoreOptions _options;

        /// <summary>
        /// List vms.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("api/vms")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Vm[]>> Find([FromQuery]string filter)
        {
            AuthorizeAll();

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
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

            return Ok(
                await _pod.Load(id)
            );
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
            await Validate(op);

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(op.Id, Actor.GlobalId).Result
            );

            Vm vm = await _pod.ChangeState(op);

            SendBroadcast(vm, op.Type.ToString().ToLower());

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
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

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
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

            // need elevated privileges to change vm to special nets
            if (
                Actor.IsBuilder.Equals(false) &&
                change.Key == "net" &&
                change.Value.Contains("#").Equals(false) &&
                _options.AllowUnprivilegedVmReconfigure.Equals(false)
            )
            {
                throw new ActionForbidden();
            }

            Vm vm = await _pod.ChangeConfiguration(id, change);

            SendBroadcast(vm, "change");

            return Ok(vm);
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
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

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
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

            return Ok(
                await _pod.GetVmIsoOptions(
                    await GetVmIsolationTag(id)
                )
            );
        }

        /// <summary>
        /// Find virtual networks available to a vm.
        /// </summary>
        /// <param name="id">Vm Id</param>
        /// <returns></returns>
        [HttpGet("api/vm/{id}/nets")]
        public async Task<ActionResult<VmOptions>> NetOptions(string id)
        {
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

            return Ok(
                await _pod.GetVmNetOptions(
                    await GetVmIsolationTag(id)
                )
            );
        }

        /// <summary>
        /// Request a vm console access ticket.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/vm-console/{id}")]
        public async Task<ActionResult<ConsoleSummary>> Ticket(string id)
        {
            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(id, Actor.GlobalId).Result
            );

            var info = await _pod.Display(id);

            if (info.Url.IsEmpty())
                return Ok(info);

            _logger.LogDebug($"mks url: {info.Url}");

            var src = new Uri(info.Url);
            string target = "";
            string qs = "";
            string internalHost = src.Host.Split('.').First();
            string domain = Request.Host.Value.IndexOf(".") >= 0
                        ? Request.Host.Value.Substring(Request.Host.Value.IndexOf(".")+1)
                        : Request.Host.Value;

            switch (_pod.Options.TicketUrlHandler.ToLower())
            {
                case "local-app":
                    target = $"{Request.Host.Value}{Request.PathBase}{internalHost}";
                break;

                case "external-domain":
                    target = $"{internalHost}.{domain}";
                break;

                case "host-map":
                    var map = _pod.Options.TicketUrlHostMap;
                    if (map.ContainsKey(src.Host))
                        target = map[src.Host];
                break;

                case "querystring":
                default:
                    qs = $"?vmhost={src.Host}";
                    target = _options.ConsoleHost;
                break;
            }

            if (target.NotEmpty())
                info.Url = info.Url.Replace(src.Host, target);

            info.Url += qs;

            _logger.LogDebug($"mks url: {info.Url}");

            return Ok(info);
        }

        /// <summary>
        /// Resolve a vm from a template.
        /// </summary>
        /// <param name="id">Template Id</param>
        /// <returns></returns>
        [HttpGet("api/vm-template/{id}")]
        public async Task<ActionResult<Vm>> Resolve(string id)
        {
            var template  = await _templateService.GetDeployableTemplate(id, null);

            string name = $"{template.Name}#{template.IsolationTag}";

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(name, Actor.GlobalId).Result
            );

            return Ok(
                await _pod.Refresh(template)
            );
        }

        /// <summary>
        /// Deploy a vm from a template.
        /// </summary>
        /// <param name="id">Template Id</param>
        /// <returns></returns>
        [HttpPost("api/vm-template/{id}")]
        public async Task<ActionResult<Vm>> Deploy(string id)
        {
            VmTemplate template  = await _templateService.GetDeployableTemplate(id, null);

            string name = $"{template.Name}#{template.IsolationTag}";

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(name, Actor.GlobalId).Result
            );

            Vm vm = await _pod.Deploy(template, Actor.IsCreator);

            // SendBroadcast(vm, "deploy");
            VmState state = new VmState
            {
                Id = template.Id.ToString(),
                Name = vm.Name.Untagged(),
                IsolationId = vm.Name.Tag(),
                IsRunning = vm.State == VmPowerState.Running
            };

            await _hub.Clients
                .Group(state.IsolationId)
                .VmEvent(new BroadcastEvent<VmState>(User, "VM.DEPLOY", state))
            ;

            return Ok(vm);
        }

        /// <summary>
        /// Initialize vm disks.
        /// </summary>
        /// <param name="id">Template Id</param>
        /// <returns></returns>
        [HttpPut("api/vm-template/{id}")]
        public async Task<ActionResult<Vm>> Initialize(string id)
        {
            VmTemplate template  = await _templateService.GetDeployableTemplate(id, null);

            string name = $"{template.Name}#{template.IsolationTag}";

            AuthorizeAny(
                () => Actor.IsAdmin,
                () => CanManageVm(name, Actor.GlobalId).Result
            );

            return Ok(
                await _pod.CreateDisks(template)
            );
        }

        /// <summary>
        /// Initiate hypervisor manager reload
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        [HttpPost("api/pod/{host}")]
        [Authorize(Policy = "AdminOnly")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> ReloadHost(string host)
        {
            await _pod.ReloadHost(host);
            return Ok();
        }

        private async Task<bool> CanManageVm(string id, string userId)
        {
            return await _userService.CanInteract(
                await GetVmIsolationTag(id),
                userId
            );
        }

        private async Task<string> GetVmIsolationTag(string id)
        {
            return id.Contains("#")
                ? id.Tag()
                : (await _pod.Load(id))?.Name.Tag();
        }

        private void SendBroadcast(Vm vm, string action)
        {
            VmState state = new VmState
            {
                Id = vm.Id,
                Name = vm.Name.Untagged(),
                IsolationId = vm.Name.Tag(),
                IsRunning = vm.State == VmPowerState.Running
            };

            _hub.Clients
                .Group(vm.Name.Tag())
                .VmEvent(new BroadcastEvent<VmState>(User, "VM." + action.ToUpper(), state))
            ;
        }
    }
}