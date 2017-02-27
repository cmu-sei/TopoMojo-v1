using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Models;
using TopoMojo.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Collections.Specialized;

namespace TopoMojo.Controllers
{
    [Authorize]
   public class FileController : _Controller
    {
        public FileController(
            IFileUploadMonitor monitor,
            IOptions<ApplicationOptions> config,
            IHostingEnvironment host,
            IServiceProvider sp) : base(sp)
        {
            _host = host;
            _monitor = monitor;
            _config = config.Value.FileUpload;
        }
        private readonly IHostingEnvironment _host;
        private readonly IFileUploadMonitor _monitor;
        private readonly FileUploadOptions _config;

        [HttpGet("api/[controller]/[action]/{id}")]
        public IActionResult Progress([FromRoute]string id)
        {
            return Json(_monitor.Progress(id).Progress);
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            FormOptions _formOptions = new FormOptions {
                MultipartBodyLengthLimit = (long)1E9
            };

            string boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _formOptions.MultipartBoundaryLengthLimit);
            MultipartReader reader = new MultipartReader(boundary, HttpContext.Request.Body);

            MultipartSection section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        NameValueCollection fileMetadata = MultipartRequestHelper.FileProperties(contentDisposition.FileName);

                        string filename = fileMetadata["fn"];
                        string pkey = fileMetadata["pk"];
                        string key = fileMetadata["fk"];
                        string scope = fileMetadata["fd"];
                        long size = Int64.Parse(fileMetadata["fs"] ?? "0");
                        if (_config.MaxFileBytes > 0 && size > _config.MaxFileBytes)
                            throw new Exception($"File ${filename} exceeds the {_config.MaxFileBytes} byte maximum size.");

                        _logger.LogInformation(_user.Email + " uploading file " + filename);
                        string dest = DestinationPath(filename, key, scope);
                        using (var targetStream = System.IO.File.Create(dest))
                        {
                            await Save(section.Body, targetStream, size, pkey);
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Json(true);
        }

        private string DestinationPath(string filename, string key, string scope)
        {
            string fn = "", keypath="", root = "", path = "";

            //sanitize fn
            char[] bad = Path.GetInvalidFileNameChars();
            foreach (char c in filename.ToCharArray())
                if (!bad.Contains(c))
                    fn += c;

            bad = Path.GetInvalidPathChars();
            foreach (char c in key.ToCharArray())
                if (!bad.Contains(c))
                    keypath += c;

            switch (scope)
            {
                case "public":
                path = _config.IsoRoot; //Path.Combine(_config.IsoRoot, "public");
                break;

                case "private":
                path = Path.Combine(_config.TopoRoot,keypath);
                break;

                case "temp":
                path = Path.Combine(_config.TopoRoot, keypath, "temp");
                root = _config.TopoRoot;
                break;

                case "img":
                path = Path.Combine(_config.MiscRoot, keypath);
                break;

                default:
                throw new Exception("Invalid file scope.");

            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, fn);
            return path;
        }

        private async Task Save(Stream source, Stream dest, long size, string key)
        {
            _monitor.Start(key);

            if (size == 0) size = (long)5E9;
            byte[] buffer = new byte[4096];
            int bytes = 0, progress = 0;
            long totalBytes = 0, totalBlocks = 0;

            do
            {
                bytes = await source.ReadAsync(buffer, 0, buffer.Length);
                await dest.WriteAsync(buffer, 0, bytes);
                totalBlocks += 1;
                totalBytes += bytes;
                if (totalBlocks % 1024 == 0)
                {
                    progress = (int)(((float)totalBytes / (float)size) * 100);
                    _monitor.Update(key, progress);
                }
            } while (bytes > 0);
            _monitor.Update(key, 100);
            FileProgress fp = _monitor.Progress(key);
            int duration = (int)fp.Stop.Subtract(fp.Start).TotalSeconds;
            _logger.LogInformation($"FileUpload complete for {key} in {duration}s");
        }

        public IActionResult Error()
        {
            return View();
        }

    }


}
