using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class VmController : HubController<TopologyHub>
    {
        public VmController(
            TemplateManager templateManager,
            // TopologyManager topoManager,
            ProfileManager profileManager,
            IPodManager podManager,
            IServiceProvider sp,
            IConnectionManager sigr
            )
        :base(sigr, sp)
        {
            _mgr = templateManager;
            // _topoManager = topoManager;
            _profileManager = profileManager;
            _pod = podManager;
        }

        private readonly IPodManager _pod;
        private readonly TemplateManager _mgr;
        // private readonly TopologyManager _topoManager;
        private readonly ProfileManager _profileManager;

        // [AllowAnonymous]
        // [HttpGet("/[controller]/[action]/{id}")]
        // public async Task<IActionResult> Console([FromRoute] string id)
        // {
        //     //await HttpContext.Authentication.SignInAsync("Cookies", HttpContext.User);
        //     return View("wmks", new DisplayInfo { Id = id });
        // }

        [HttpGet("api/vm/{id}/ticket")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Ticket([FromRoute] string id)
        {
            await AuthorizeAction(id, "ticket");
            DisplayInfo info = await _pod.Display(id);
            return Ok(info);
        }

        // [HttpGet("api/vm/{id}")]
        // [JsonExceptionFilter]
        // public async Task<IActionResult> Refresh(int id)
        // {
        //     Template template  = await _mgr.GetDeployableTemplate(id, null);
        //     Vm vm = await _pod.Refresh(template);
        //     return Ok(vm);
        // }

        [HttpGet("api/vm/{id}")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load(string id)
        {
            await AuthorizeAction(id, "load");
            Vm vm = await _pod.Load(id);
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/start")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Start(string id)
        {
            await AuthorizeAction(id, "start");
            Vm vm = await _pod.Start(id);
            SendBroadcast(vm);
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/stop")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Stop(string id)
        {
            await AuthorizeAction(id, "stop");
            Vm vm = await _pod.Stop(id);
            SendBroadcast(vm);
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/save")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Save(string id)
        {
            await AuthorizeAction(id, "save");
            Vm vm = await _pod.Save(id);
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/revert")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Revert(string id)
        {
            await AuthorizeAction(id, "revert");
            Vm vm = await _pod.Revert(id);
            SendBroadcast(vm);
            return Ok(vm);
        }

        [HttpDelete("api/vm/{id}")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]string id)
        {
            await AuthorizeAction(id, "delete");
            Vm vm = await _pod.Delete(id);
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/change")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Change([FromRoute]string id, [FromBody] KeyValuePair change)
        {
            await AuthorizeAction(id, "change");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            return Ok(await _pod.Change(id, change));
        }

        [HttpPost("api/vm/{id}/deploy")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Deploy([FromRoute]int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            return Ok(await _pod.Deploy(template));
        }

        [HttpPost("api/vm/{id}/init")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Initialize([FromRoute]int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            return Ok(await _pod.CreateDisks(template));
        }

        [HttpPost("api/vm/{id}/answer")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Answer([FromRoute]string id, [FromBody] VmAnswer answer)
        {
            await AuthorizeAction(id, "answer");
            return Ok(await _pod.Answer(id, answer));
        }

        [HttpGet("api/vm/{id}/isos")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> IsoOptions([FromRoute]string id)
        {
            await AuthorizeAction(id, "isooptions");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            if (vm == null)
                return BadRequest();

            string tag = vm.Name.Tag();
            return Ok(await _pod.GetVmIsoOptions(tag));
        }

        [HttpGet("api/vm/{id}/nets")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> NetOptions([FromRoute]string id)
        {
            await AuthorizeAction(id, "netoptions");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            if (vm == null)
                return BadRequest();

            string tag = vm.Name.Tag();
            return Ok(await _pod.GetVmNetOptions(tag));
        }

        private async Task<bool> AuthorizeAction(string id, string method)
        {
            if (_profile.IsAdmin)
                return true;

            Vm vm = _pod.Find(id).Result.FirstOrDefault();
            if (vm == null)
                throw new InvalidOperationException();

            string instanceId = vm.Name.Tag();
            if (String.IsNullOrEmpty(instanceId))
                throw new InvalidOperationException();

            bool result = await _profileManager.CanEditSpace(instanceId);

            // if (!result && "ticket load".Contains(method))
            //     result = await _topoManager.AllowedInstanceAccess(instanceId);

            if (!result)
                throw new InvalidOperationException();

            if (method != "load")
            {
                Log(method, vm);
                //_logger.LogInformation($"vm-action {method} {id}");
            }
            return result;
        }

        private void SendBroadcast(Vm vm)
        {
            Clients.Group(vm.Name.Tag()).VmUpdated(new {
                id = vm.Id,
                name = vm.Name.Untagged(),
                isRunning = vm.State == VmPowerState.running
            });

        }
    }


}
