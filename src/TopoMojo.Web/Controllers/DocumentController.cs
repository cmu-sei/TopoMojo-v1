using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class DocumentController : _Controller
    {
        public DocumentController(
            TopologyManager topologyManager,
            IHostingEnvironment env,
            IServiceProvider sp
        ) : base(sp)
        {
            _mgr = topologyManager;
            _env = env;
        }

        private readonly TopologyManager _mgr;
        private readonly IHostingEnvironment _env;

        [HttpPostAttribute("{guid}")]
        [JsonExceptionFilterAttribute]
        public async Task<bool> Save([FromRoute] string guid, [FromBody] string text)
        {
            if (await _mgr.CanEdit(guid))
            {
                string path = GetPath("docs", guid);
                path = System.IO.Path.Combine(path, guid+".md");
                System.IO.File.WriteAllText(path, text);
                return true;
            }
            return false;
        }


        [HttpGetAttribute("{guid}")]
        [JsonExceptionFilterAttribute]
        public async Task<object[]> Images([FromRoute] string guid)
        {
            if (await _mgr.CanEdit(guid))
            {
                string path = Path.Combine(_env.WebRootPath, "docs", guid);
                if (Directory.Exists(path))
                {
                    return Directory.GetFiles(path)
                        .Select(x => new { filename = Path.GetFileName(x)})
                        .ToArray();
                }
            }
            return null;
        }

        [HttpDelete("{guid}/{filename}")]
        [JsonExceptionFilterAttribute]
        public async Task<object> Delete([FromRoute] string guid, [FromRoute] string filename)
        {
            if (await _mgr.CanEdit(guid))
            {
                string path = Path.Combine(_env.WebRootPath, "docs", guid, filename);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    return new { filename = filename };
                }
            }
            throw new InvalidOperationException();
        }

        [HttpPostAttribute("{guid}")]
        [JsonExceptionFilterAttribute]
        public async Task<object> Upload([FromRoute] string guid, IFormFile file)
        {
            if (file.Length > 0)
            {
                if (await _mgr.CanEdit(guid))
                {
                    string path = GetPath("docs", guid);
                    string filename = SanitizeFilename(file.FileName);
                    path = Path.Combine(path, filename);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return new { filename = filename};
                }
            }
            throw new InvalidOperationException();
        }


        private string GetPath(params string[] segments)
        {
            string path = _env.WebRootPath;
            foreach (string s in segments)
                path = System.IO.Path.Combine(path, s);

            if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

            return path;
        }

        private string SanitizeFilename(string filename)
        {
            string fn = "";
            char[] badFilenameChars = Path.GetInvalidFileNameChars();
            filename = filename.Replace(" ", "");
            foreach (char c in filename.ToCharArray())
                if (!badFilenameChars.Contains(c))
                    fn += c;
            return fn;
        }
    }
}
