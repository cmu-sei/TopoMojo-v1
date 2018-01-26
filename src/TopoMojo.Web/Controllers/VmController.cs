using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
//using TopoMojo.Core;
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
            // TopologyManager topoManager,
            Core.ProfileManager profileManager,
            IPodManager podManager,
            IServiceProvider sp
            )
        :base(sp)
        {
            _mgr = templateManager;
            _profileManager = profileManager;
            _pod = podManager;
        }

        private readonly IPodManager _pod;
        private readonly Core.TemplateManager _mgr;
        private readonly Core.ProfileManager _profileManager;

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

        [HttpGet("api/vms/find")]
        [ProducesResponseType(typeof(Vm[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Find([FromQuery] string tag)
        {
            Vm[] vms = new Vm[]{};
            if (_profile.IsAdmin && tag.HasValue())
                vms = await _pod.Find(tag);
            return Ok(vms);
        }

        [HttpGet("api/vm/{id}/load")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load(string id)
        {
            await AuthorizeAction(id, "load");
            Vm vm = await _pod.Load(id);
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/resolve")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Resolve(int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            Vm vm = await _pod.Refresh(template);
            if (vm != null)
                await AuthorizeAction(vm, "resolve");
            //Vm vm = await _pod.Load(id);
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/start")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Start(string id)
        {
            await AuthorizeAction(id, "start");
            Vm vm = await _pod.Start(id);
            SendBroadcast(vm, "start");
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/stop")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Stop(string id)
        {
            await AuthorizeAction(id, "stop");
            Vm vm = await _pod.Stop(id);
            SendBroadcast(vm, "stop");
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/save")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Save(string id)
        {
            await AuthorizeAction(id, "save");
            Vm vm = await _pod.Save(id);
            SendBroadcast(vm, "save");
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/revert")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Revert(string id)
        {
            await AuthorizeAction(id, "revert");
            Vm vm = await _pod.Revert(id);
            SendBroadcast(vm, "revert");
            return Ok(vm);
        }

        [HttpDelete("api/vm/{id}/delete")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]string id)
        {
            await AuthorizeAction(id, "delete");
            Vm vm = await _pod.Delete(id);
            SendBroadcast(vm, "delete");
            return Ok(vm);
        }

        [HttpPost("api/vm/{id}/change")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Change([FromRoute]string id, [FromBody] KeyValuePair change)
        {
            await AuthorizeAction(id, "change");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            SendBroadcast(vm, "change");
            return Ok(await _pod.Change(id, change));
        }

        [HttpGet("api/vm/{id}/deploy")]
        [ProducesResponseType(typeof(Vm), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Deploy([FromRoute]int id)
        {
            Template template  = await _mgr.GetDeployableTemplate(id, null);
            Vm vm = await _pod.Deploy(template);
            SendBroadcast(vm, "deploy");
            return Ok(vm);
        }

        [HttpGet("api/vm/{id}/init")]
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
            Vm vm = await _pod.Answer(id, answer);
            SendBroadcast(vm, "answer");
            return Ok(vm);
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

        [HttpGet("api/host/{host}/reload")]
        public async Task<IActionResult> ReloadHost([FromRoute]string host)
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

            bool result = await _profileManager.CanEditSpace(instanceId);

            if (!result) {
                throw new InvalidOperationException();
                //_logger.LogDebug("checking if {0} can edit {1} -- {2}", _profile.Name, instanceId, result);
            }

            if (!"load resolve".Contains(method))
            {
                Log(method, vm);
                //_logger.LogInformation($"vm-action {method} {id}");
            }
            return result;
        }
        private void SendBroadcast(Vm vm, string action)
        {
            // Core.Models.VmState state = new Core.Models.VmState
            // {
            //     Id = vm.Id,
            //     Name = vm.Name.Untagged(),
            //     IsRunning = vm.State == VmPowerState.running
            // };
            // Broadcast(vm.Name.Tag(), new BroadcastEvent<Core.Models.VmState>(User, "VM." + action.ToUpper(), state));

        }
    }


}
