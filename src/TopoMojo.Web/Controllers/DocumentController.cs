// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;
using TopoMojo.Web.Models;

namespace TopoMojo.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class DocumentController : _Controller
    {
        public DocumentController(
            ILogger<AdminController> logger,
            IIdentityResolver identityResolver,
            WorkspaceService workspaceService,
            FileUploadOptions uploadOptions,
            IHubContext<TopologyHub, ITopoEvent> hub
        ) : base(logger, identityResolver)
        {
            _uploadOptions = uploadOptions;
            _workspaceService = workspaceService;
            _hub = hub;
        }

        private readonly WorkspaceService _workspaceService;
        private readonly FileUploadOptions _uploadOptions;
        private readonly IHubContext<TopologyHub, ITopoEvent> _hub;

        /// <summary>
        /// Save markdown as document.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <param name="text">Markdown text</param>
        /// <param name="fromTyping">Cause of save was automatic from user typing</param>
        /// <returns></returns>
        [HttpPut("api/document/{id}")]
        public async Task<ActionResult> Save(string id, [FromBody]string text, bool fromTyping = false)
        {
            if (!await _workspaceService.CanEdit(id))
                return Forbid();

            string path = BuildPath();

            path = System.IO.Path.Combine(path, id + ".md");
            System.IO.File.WriteAllText(path, text);
            
            if (fromTyping)
                SendBroadcast($"{id}-doc", "saved", text);

            return Ok();
        }

        /// <summary>
        /// List document image files.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <returns></returns>
        [HttpGet("api/images/{id}")]
        public async Task<ActionResult<ImageFile[]>> Images(string id)
        {
            if (!await _workspaceService.CanEdit(id))
                return Forbid();

            string path = Path.Combine(_uploadOptions.DocRoot, id);

            // if (!Directory.Exists(path))
            //     return Ok(new ImageFile[]{});

            return Ok(
                Directory.GetFiles(path)
                .Select(x => new ImageFile { Filename = Path.GetFileName(x)})
                .ToArray()
            );
        }

        /// <summary>
        /// Delete document image file.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpDelete("api/image/{id}")]
        public async Task<IActionResult> Delete(string id, string filename)
        {
            if (!await _workspaceService.CanEdit(id))
                return Forbid();

            string path = BuildPath(id, filename);
            if (filename.HasValue() && System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// Upload document image file.
        /// </summary>
        /// <param name="id">Workspace Id</param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("api/image/{id}")]
        public async Task<ActionResult<ImageFile>> Upload(string id, IFormFile file)
        {
            if (!await _workspaceService.CanEdit(id))
                return Forbid();

            string path = BuildPath(id);

            string filename = file.FileName.SanitizeFilename();

            path = Path.Combine(path, filename);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new ImageFile { Filename = filename});
        }

        private string BuildPath(params string[] segments)
        {
            string path = _uploadOptions.DocRoot;

            foreach (string s in segments)
                path = System.IO.Path.Combine(path, s);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        private void SendBroadcast(string roomId, string action, string text)
        {
            _hub.Clients
                .Group(roomId)
                .DocumentEvent(
                    new BroadcastEvent<Document>(
                        User,
                        "DOCUMENT." + action.ToUpper(),
                        new Document {
                            Text = text,
                            WhenSaved = DateTime.UtcNow.ToString("u")
                        }
                    )
                );
        }

    }

}
