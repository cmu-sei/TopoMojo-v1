// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using TopoMojo.Services;
using TopoMojo.Web.Services;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    public class FileController : _Controller
    {
        public FileController(
            IFileUploadHandler uploader,
            IFileUploadMonitor monitor,
            FileUploadOptions uploadOptions,
            IWebHostEnvironment host,
            IServiceProvider sp,
            WorkspaceService workspaceService
        ) : base(sp)
        {
            _host = host;
            _monitor = monitor;
            _config = uploadOptions;
            _workspaceService = workspaceService;
            _uploader = uploader;
        }

        private readonly IWebHostEnvironment _host;
        private readonly IFileUploadMonitor _monitor;
        private readonly FileUploadOptions _config;
        private readonly WorkspaceService _workspaceService;
        private readonly IFileUploadHandler _uploader;

        [HttpGet("api/file/progress/{id}")]
        [ProducesResponseType(typeof(int), 200)]
        public IActionResult Progress(string id)
        {
            return Json(_monitor.Check(id).Progress);
        }

        [HttpPost("api/file/upload")]
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<bool>> Upload()
        {
            await _uploader.Process(
                Request,
                metadata => {
                    string publicTarget = Guid.Empty.ToString();
                    string original = metadata["original-name"];
                    string filename = metadata["name"] ?? original;
                    string key = metadata["group-key"];
                    long size = Int64.Parse(metadata["size"] ?? "0");

                    if (_config.MaxFileBytes > 0 && size > _config.MaxFileBytes)
                        throw new Exception($"File {filename} exceeds the {_config.MaxFileBytes} byte maximum size.");

                    if (key != publicTarget && !_workspaceService.CanEdit(key).Result)
                        throw new InvalidOperationException();

                    // Log("uploading", null, filename);
                    string dest = BuildDestinationPath(filename, key);
                    metadata.Add("destination-path", dest);
                    Log("uploading", null, dest);

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

        private string BuildDestinationPath(string filename, string key)
        {
            string path = Path.Combine(
                _config.IsoRoot,
                Sanitize(key, Path.GetInvalidPathChars())
            );

            string fn = Sanitize(filename, Path.GetInvalidFileNameChars());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return Path.Combine(path, fn);
        }

        private string Sanitize(string target, char[] bad)
        {
            string p = "";

            foreach (char c in target.ToCharArray())
                if (!bad.Contains(c))
                    p += c;

            return p;
        }

    }

}
