// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TopoMojo.Core;
using TopoMojo.Extensions;
using TopoMojo.Web;
using TopoMojo.Web.Models;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class DocumentController : _Controller
    {
        public DocumentController(
            WorkspaceService workspaceService,
            IWebHostEnvironment env,
            IServiceProvider sp
        ) : base(sp)
        {
            _workspaceService = workspaceService;
            _env = env;
        }

        private readonly WorkspaceService _workspaceService;
        private readonly IWebHostEnvironment _env;

        [HttpPut("api/document/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult> Save(string id, [FromBody]string text)
        {
            if (await _workspaceService.CanEdit(id))
            {
                string path = GetPath("docs");
                path = System.IO.Path.Combine(path, id+".md");
                System.IO.File.WriteAllText(path, text);
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet("api/images/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<ImageFile[]>> Images(string id)
        {
            if (await _workspaceService.CanEdit(id))
            {
                string path = Path.Combine(_env.WebRootPath, "docs", id);
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

        [HttpDelete("api/image/{id}")]
        [JsonExceptionFilter]
        public async Task<ActionResult<ImageFile>> Delete(string id, string filename)
        {
            if (filename.HasValue() && await _workspaceService.CanEdit(id))
            {
                string path = Path.Combine(_env.WebRootPath, "docs", id, filename);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    return Ok(new ImageFile { Filename = filename });
                }
            }
            throw new InvalidOperationException();
        }

        [HttpPost("api/image/{id}")]
        [JsonExceptionFilter]
        [ApiExplorerSettings(IgnoreApi=true)]
        public async Task<ActionResult<ImageFile>> Upload(string id, IFormFile file)
        {
            if (file.Length > 0)
            {
                if (await _workspaceService.CanEdit(id))
                {
                    string path = GetPath("docs", id);
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
