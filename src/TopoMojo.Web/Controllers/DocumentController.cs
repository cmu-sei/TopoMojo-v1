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
using TopoMojo.Web.Models;

namespace TopoMojo.Controllers
{
    [Authorize]
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

        [HttpPut("api/document/{guid}")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        public async Task<bool> Save([FromRoute] string guid, [FromBody] string text)
        {
            if (await _mgr.CanEdit(guid))
            {
                string path = GetPath("docs");
                path = System.IO.Path.Combine(path, guid+".md");
                System.IO.File.WriteAllText(path, text);
                return true;
            }
            return false;
        }

        [HttpGet("api/images/{guid}")]
        [ProducesResponseType(typeof(ImageFile[]), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Images([FromRoute] string guid)
        {
            if (await _mgr.CanEdit(guid))
            {
                string path = Path.Combine(_env.WebRootPath, "docs", guid);
                if (Directory.Exists(path))
                {
                    return Ok(
                        Directory.GetFiles(path)
                        .Select(x => new ImageFile { Filename = Path.GetFileName(x)})
                        .ToArray()
                    );
                }
            }
            return Ok(new ImageFile[]{});
        }

        [HttpDelete("api/image/{guid}")]
        [ProducesResponseType(typeof(ImageFile), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute] string guid, [FromQuery] string filename)
        {
            if (await _mgr.CanEdit(guid))
            {
                string path = Path.Combine(_env.WebRootPath, "docs", guid, filename);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    return Json(new ImageFile { Filename = filename });
                }
            }
            throw new InvalidOperationException();
        }

        [HttpPost("api/image/{guid}")]
        [ProducesResponseType(typeof(ImageFile), 200)]
        [JsonExceptionFilter]
        [ApiExplorerSettings(IgnoreApi=true)]
        public async Task<IActionResult> Upload([FromRoute] string guid, IFormFile file)
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
                    return Ok(new ImageFile { Filename = filename});
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
            return fn.ToLower();
        }
    }
}
