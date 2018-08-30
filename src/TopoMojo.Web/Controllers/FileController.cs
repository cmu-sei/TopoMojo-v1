using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class FileController : _Controller
    {
        public FileController(
            IFileUploadHandler uploader,
            IFileUploadMonitor monitor,
            FileUploadOptions uploadOptions,
            IHostingEnvironment host,
            IServiceProvider sp,
            TopologyManager topoManager
        ) : base(sp)
        {
            _host = host;
            _monitor = monitor;
            _config = uploadOptions;
            _topoManager = topoManager;
            _uploader = uploader;
        }
        private readonly IHostingEnvironment _host;
        private readonly IFileUploadMonitor _monitor;
        private readonly FileUploadOptions _config;
        private readonly TopologyManager _topoManager;

        private readonly IFileUploadHandler _uploader;

        [HttpGet("api/file/progress/{id}")]
        [ProducesResponseType(typeof(int), 200)]
        public IActionResult Progress(string id)
        {
            return Json(_monitor.Check(id).Progress);
        }

        [HttpPost("api/file/upload")]
        [JsonExceptionFilter]
        [DisableFormValueModelBinding]
        // [ApiExplorerSettings(IgnoreApi=true)]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult<bool>> Upload()
        {
            await _uploader.Process(
                Request,
                metadata => {

                    string original = metadata["original-name"];
                    string filename = metadata["name"] ?? original;
                    string key = metadata["group-key"];
                    string scope = metadata["scope"];
                    long size = Int64.Parse(metadata["size"] ?? "0");

                    if (_config.MaxFileBytes > 0 && size > _config.MaxFileBytes)
                        throw new Exception($"File {filename} exceeds the {_config.MaxFileBytes} byte maximum size.");

                    if (scope == "public" && !_profile.IsAdmin)
                        throw new InvalidOperationException();

                    if (scope == "private" && !_topoManager.CanEdit(key).Result)
                        throw new InvalidOperationException();

                    // Log("uploading", null, filename);
                    string dest = BuildDestinationPath(filename, key, scope);
                    metadata.Add("destination-path", dest);
                    Log("uploading", null, dest);

                    string path = Path.GetDirectoryName(dest);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    return System.IO.File.Create(dest);
                },
                status => {
                    if (status.Error != null)
                    {
                        string dp = status.Metadata["destination-path"];
                        if (System.IO.File.Exists(dp))
                            System.IO.File.Delete(dp);
                    }
                    _monitor.Update(status.Key, status.Progress);
                    // TODO: broadcast progress to group
                },
                options => {
                    options.MultipartBodyLengthLimit = (long)((_config.MaxFileBytes > 0) ? _config.MaxFileBytes : 1E9);
                },
                metadata => {
                    string dp = metadata["destination-path"];
                    if (!dp.ToLower().EndsWith(".iso") && System.IO.File.Exists(dp))
                    {
                        CDBuilder builder = new CDBuilder();
                        builder.UseJoliet = true;
                        builder.VolumeIdentifier = "UploadedFile";
                        builder.AddFile(Path.GetFileName(dp), dp);
                        builder.Build(dp + ".iso");
                        System.IO.File.Delete(dp);
                    }
                }
            );

            return Json(true);
        }

        private string SanitizeFileName(string filename)
        {
            string fn = "";
            char[] bad = Path.GetInvalidFileNameChars();
            foreach (char c in filename.ToCharArray())
                if (!bad.Contains(c))
                    fn += c;
            return fn;
        }

        private string SanitizeFilePath(string path)
        {
            string p = "";
            char[] bad = Path.GetInvalidPathChars();
            foreach (char c in path.ToCharArray())
                if (!bad.Contains(c))
                    p += c;
            return p;
        }

        private string BuildDestinationPath(string filename, string key, string scope)
        {
            string fn = SanitizeFileName(filename);
            string path = "";

            switch (scope)
            {
                case "public":
                    path = _config.IsoRoot;
                    break;

                case "private":
                    path = Path.Combine(_config.TopoRoot, key);
                    break;

                default:
                    throw new Exception("Invalid file scope.");

            }

            path = SanitizeFilePath(path);
            path = Path.Combine(path, fn);
            return path;
        }

        public IActionResult Error()
        {
            return View();
        }

    }


}
