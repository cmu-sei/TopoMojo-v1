// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;

namespace TopoMojo.Services
{
    public class TransferService : _Service
    {
        public TransferService (
            IUserStore userStore,
            IWorkspaceStore workspaceStore,
            ITemplateStore templateStore,
            ILogger<TransferService> logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base(logger, mapper, options, identityResolver)
        {
            _workspaceStore = workspaceStore;
            _templateStore = templateStore;
            _userStore = userStore;
            jsonSerializerSettings = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        private readonly IUserStore _userStore;
        private readonly IWorkspaceStore _workspaceStore;
        private readonly ITemplateStore _templateStore;
        private Data.Profile _user;
        private JsonSerializerOptions jsonSerializerSettings;

        public async Task Export(int[] ids, string src, string dest)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            var list = new List<Data.Topology>();

            foreach (int id in ids)
            {
                var topo = await _workspaceStore.LoadWithParents(id);

                if (topo != null)
                    list.Add(topo);

            }

            // if (ids.Contains(0))
            // {
            //     list.Add(await _topoRepo.LoadAdminTopo());
            // }

            if (list.Count < 1)
                return;

            string docSrc = Path.Combine(src, "_docs");

            string docDest = Path.Combine(dest, "_docs");

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            if (!Directory.Exists(docDest))
                Directory.CreateDirectory(docDest);

            foreach (var topo in list)
            {
                string folder = Path.Combine(dest, topo.GlobalId);

                Directory.CreateDirectory(folder);

                File.WriteAllText(
                    Path.Combine(folder, "import.this"),
                    "please import this topology"
                );

                //export data
                topo.Workers.Clear();

                topo.Gamespaces.Clear();

                topo.Id = 0;

                topo.ShareCode = "";

                foreach (var template in topo.Templates)
                {
                    template.Id = 0;
                    template.TopologyId = 0;
                    template.Topology = null;
                }

                File.WriteAllText(
                    Path.Combine(folder, "topo.json"),
                    JsonSerializer.Serialize(topo, jsonSerializerSettings)
                );

                //export doc
                try
                {
                    CopyFile(
                        Path.Combine(docSrc, topo.GlobalId) + ".md",
                        Path.Combine(docDest, topo.GlobalId) + ".md"
                    );

                    CopyFolder(
                        Path.Combine(docSrc, topo.GlobalId),
                        Path.Combine(docDest, topo.GlobalId)
                    );

                } catch {}

                //export disk-list
                var disks = new List<string>();

                foreach (var template in topo.Templates)
                {
                    var tu = new TemplateUtility(template.Detail ?? template.Parent.Detail);
                    var t = tu.AsTemplate();

                    foreach (var disk in t.Disks)
                        disks.Add(disk.Path);

                    if (t.Iso.HasValue())
                        disks.Add(t.Iso);
                }

                if (disks.Count > 0)
                {
                    File.WriteAllLines(
                        Path.Combine(folder, "topo.disks"),
                        disks.Distinct()
                    );
                }
            }

        }

        public async Task<IEnumerable<string>> Import(string repoPath, string docPath)
        {
            if (!User.IsAdmin)
                throw new InvalidOperationException();

            var results = new List<string>();

            var files = Directory.GetFiles(repoPath, "import.this", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                //skip any staged exports
                if (file.Contains("_export"))
                    continue;

                try
                {
                    _logger.LogInformation("Importing topo from {0}", file);

                    string folder = Path.GetDirectoryName(file);

                    //import data
                    string dataFile = Path.Combine(folder, "topo.json");

                    var topo = JsonSerializer.Deserialize<Data.Topology>(
                        File.ReadAllText(dataFile),
                        jsonSerializerSettings
                    );

                    //enforce uniqueness :(
                    var found = await _workspaceStore.Load(topo.GlobalId);

                    if (found != null)
                        continue;
                        // throw new Exception("Duplicate GlobalId");

                    // map parentid to new parentId
                    foreach (var template in topo.Templates)
                    {
                        if (template.Parent != null)
                        {
                            var pt = await _templateStore.Load(template.Parent.GlobalId);

                            if (pt == null)
                            {
                                template.ParentId = 0;
                                template.TopologyId = 0;
                                pt = await _templateStore.Add(template.Parent);
                            }

                            template.ParentId = pt.Id;
                            template.Parent = null;
                        }
                    }

                    await _workspaceStore.Add(topo);

                    results.Add($"Success: {topo.Name}");

                }
                catch (Exception ex)
                {
                    results.Add($"Failure: {file} -- {ex.Message}");

                    _logger.LogError(ex, "Import topo failed for {0}", file);
                }
                finally
                {
                    //clean up
                    File.Delete(file);
                }
            }

            return results;
        }

        private void CopyFile(string src, string dest, bool deleteSource = false)
        {
            if (File.Exists(src))
            {
                if (!Directory.Exists(Path.GetDirectoryName(dest)))
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));

                File.Copy(src, dest);

                if (deleteSource)
                    File.Delete(src);
            }
        }

        private void CopyFolder(string src, string dest, bool deleteSource = false)
        {
            if (Directory.Exists(src))
            {
                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);

                foreach (string file in Directory.GetFiles(src))
                    File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));

                if (deleteSource)
                    Directory.Delete(src, true);
            }
        }

    }
}
