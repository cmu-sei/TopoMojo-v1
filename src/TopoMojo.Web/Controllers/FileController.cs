using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using TopoMojo.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Web;
using TopoMojo.Core;

namespace TopoMojo.Controllers
{
    [Authorize]
   public class FileController : _Controller
    {
        public FileController(
            IFileUploadManager uploader,
            IOptions<ApplicationOptions> config,
            IHostingEnvironment host,
            IServiceProvider sp) : base(sp)
        {
            _host = host;
            _uploader = uploader;
            _config = config.Value.FileUpload;
        }
        private readonly IHostingEnvironment _host;
        private readonly IFileUploadManager _uploader;
        private readonly FileUploadConfiguration _config;

        [HttpGet("api/[controller]/[action]/{id}")]
        public async Task<IActionResult> Progress([FromRoute]string id)
        {
            return Json(_uploader.CheckProgress(id));
        }

        [HttpPost]
        [JsonExceptionFilter]
        [FileUploadMaxSize((long)10E6)]
        public async Task<IActionResult> UploadImage(string id, IFormFile file)
        {
            if (!AuthorizedForRoom(id))
                return BadRequest();

            if (!file.ContentType.StartsWith("image"))
                throw new Exception($"Invalid file type [{file.ContentType}].");

            string root = _host.WebRootPath;
            string path = Path.Combine(root, _config.MiscRoot, id);
            if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            path = Path.Combine(path, file.FileName);

            using (var dest = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(dest);
            }
            return Json(new { filename = path.Replace(root, "") });
        }

        [HttpPost]
        [JsonExceptionFilter]
        [FileUploadMaxSize((long)10E9)]
        public async Task<IActionResult> UploadIso(string id, IFormFile file)
        {
            string scope = "", fn = "", key = "";
            string path = "";
            string root = _config.IsoRoot;

            if (!AuthorizedForRoom(id))
                return BadRequest();

            if (file != null)
            {
                if (_config.MaxFileBytes > 0 && file.Length > _config.MaxFileBytes)
                    throw new Exception($"File size exceeds the {_config.MaxFileBytes} maximum.");

                key = file.FileName.Tag();
                fn = file.FileName.Untagged().ToLower();

                int x = fn.IndexOf('-');
                if (x > 0)
                {
                    scope = fn.Substring(0, x);
                    fn = fn.Substring(x+1);
                }

                switch (scope)
                {
                    case "public":
                    path = "public";
                    root = _config.IsoRoot;
                    break;

                    case "private":
                    path = id;
                    root = _config.TopoRoot;
                    break;

                    case "temp":
                    path = Path.Combine(id, "temp");
                    root = _config.TopoRoot;
                    break;

                    default:
                    throw new Exception("Invalid file scope.");
                    break;

                }

                if (!fn.EndsWith(".iso") || file.ContentType != "application/octet-stream")
                {
                    throw new Exception("Invalid file format.");
                }

                path = Path.Combine(root, path);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, fn);
                using (var dest = new FileStream(path, FileMode.Create))
                using (var source = file.OpenReadStream())
                {
                    _logger.LogInformation($"UploadIso {key} {file.FileName} {file.Length}");
                    await _uploader.Save(source, dest, file.Length, key);
                }
            }
            return Json(new { filename = path.Replace(root,"") });
        }

        public IActionResult Error()
        {
            return View();
        }

    }


}
