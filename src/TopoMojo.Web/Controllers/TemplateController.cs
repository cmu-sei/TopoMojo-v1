using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class TemplateController : _Controller
    {
        public TemplateController(
            TemplateManager templateManager,
            IPodManager podManager,
            IServiceProvider sp) : base(sp)
        {
            _mgr = templateManager;
            _pod = podManager;
        }

        private readonly TemplateManager _mgr;
        private readonly IPodManager _pod;

        [HttpGet("{id}")]
        [JsonExceptionFilterAttribute]
        public async Task<IActionResult> Load([FromRoute]int id)
        {
            return Json(await _mgr.LoadAsync(id));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromBody]Search<Template> search)
        {
            return Json(await _mgr.ListAsync(search));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Create([FromBody]TemplateModel model)
        {
            return Json(await _mgr.Create(model));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Save([FromBody]Template template)
        {
            return Json(await _mgr.Save(template));
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            return Json(await _mgr.DeleteTemplate(id));
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Remove([FromRoute]int id)
        {
            Models.Template template = await _mgr.GetDeployableTemplate(id, null);
            await _mgr.RemoveTemplate(id);
            await _pod.DeleteDisks(template); //only remove disks if removetemplate doesn't throw
            return Json(true);
        }
    }

}