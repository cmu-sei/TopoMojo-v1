using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public IActionResult Progress([FromRoute]string id)
        {
            return Json(_monitor.Check(id).Progress);
        }

        [HttpPost("api/file/upload")]
        [ProducesResponseType(typeof(bool), 200)]
        [JsonExceptionFilter]
        [DisableFormValueModelBinding]
        [ApiExplorerSettings(IgnoreApi=true)]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload()
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
                }
            );

            //TODO: handle exceptions -- remove partially saved files

            return Json(true);
        }

        // [HttpPost("api/[controller]/[action]")]
        // [DisableFormValueModelBinding]
        // //[ValidateAntiForgeryToken]
        // public async Task<IActionResult> Upload()
        // {
        //     if (!MultipartRequestExtensions.IsMultipartContentType(Request.ContentType))
        //     {
        //         return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
        //     }

        //     FormOptions _formOptions = new FormOptions
        //     {
        //         MultipartBodyLengthLimit = (long)((_config.MaxFileBytes > 0) ? _config.MaxFileBytes : 1E9)
        //     };

        //     string boundary = MultipartRequestExtensions.GetBoundary(
        //         MediaTypeHeaderValue.Parse(Request.ContentType),
        //         _formOptions.MultipartBoundaryLengthLimit);
        //     MultipartReader reader = new MultipartReader(boundary, HttpContext.Request.Body);

        //     MultipartSection section = await reader.ReadNextSectionAsync();
        //     while (section != null)
        //     {
        //         ContentDispositionHeaderValue contentDisposition;
        //         bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

        //         if (hasContentDispositionHeader)
        //         {
        //             if (MultipartRequestExtensions.HasFileContentDisposition(contentDisposition))
        //             {
        //                 NameValueCollection metadata = MultipartRequestExtensions.ParseFormValues(contentDisposition.FileName);

        //                 string filename = metadata["fn"];
        //                 string pkey = metadata["pk"] ?? Guid.NewGuid().ToString();
        //                 string key = metadata["fk"];
        //                 string scope = metadata["fd"];
        //                 long size = Int64.Parse(metadata["fs"] ?? "0");

        //                 if (_config.MaxFileBytes > 0 && size > _config.MaxFileBytes)
        //                     throw new Exception($"File {filename} exceeds the {_config.MaxFileBytes} byte maximum size.");

        //                 if (scope == "private" && ! await _topoManager.CanEdit(key))
        //                     throw new InvalidOperationException();

        //                 if (scope == "public" && !_profile.IsAdmin)
        //                     throw new InvalidOperationException();

        //                 Log("uploading", null, filename);
        //                 string dest = DestinationPath(filename, key, scope);

        //                 using (var targetStream = System.IO.File.Create(dest))
        //                 {
        //                     await Save(section.Body, targetStream, size, pkey);
        //                 }
        //             }
        //         }

        //         // Drains any remaining section body that has not been consumed and
        //         // reads the headers for the next section.
        //         section = await reader.ReadNextSectionAsync();
        //     }

        //     return Json(true);
        // }

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

        // private async Task Save(Stream source, Stream dest, long size, string key)
        // {
        //     _monitor.Update(key, 0);

        //     if (size == 0) size = (long)5E9;
        //     byte[] buffer = new byte[4096];
        //     int bytes = 0, progress = 0;
        //     long totalBytes = 0, totalBlocks = 0;

        //     do
        //     {
        //         bytes = await source.ReadAsync(buffer, 0, buffer.Length);
        //         await dest.WriteAsync(buffer, 0, bytes);
        //         totalBlocks += 1;
        //         totalBytes += bytes;
        //         if (totalBlocks % 1024 == 0)
        //         {
        //             progress = (int)(((float)totalBytes / (float)size) * 100);
        //             _monitor.Update(key, progress);
        //         }
        //     } while (bytes > 0);
        //     _monitor.Update(key, 100);
        //     FileProgress fp = _monitor.Check(key);
        //     int duration = (int)fp.Stop.Subtract(fp.Start).TotalSeconds;
        //     _logger.LogInformation($"FileUpload complete for {key} in {duration}s");
        // }

        public IActionResult Error()
        {
            return View();
        }

    }


}
