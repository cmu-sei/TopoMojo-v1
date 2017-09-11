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
using TopoMojo.Data.Entities;
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
        public async Task<Template> Load([FromRoute]int id)
        {
            return await _mgr.LoadAsync(id);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<SearchResult<Template>> List([FromBody]Search search)
        {
            return await _mgr.List(search);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<TemplateModel> Create([FromBody]TemplateModel model)
        {
            return await _mgr.Create(model);
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<Template> Save([FromBody]Template template)
        {
            return await _mgr.Save(template);
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<bool> Delete([FromRoute]int id)
        {
            return await _mgr.Delete(id);
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<bool> Remove([FromRoute]int id)
        {
            Models.Template template = await _mgr.GetDeployableTemplate(id, null);
            await _mgr.RemoveTemplate(id);
            await _pod.DeleteDisks(template); //only remove disks if removetemplate doesn't throw
            return true;
        }
    }

}