using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TopoMojo.Core.Abstractions;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Core
{
    public class TransferService
    {
        public TransferService (
            IProfileResolver profileResolver,
            IProfileRepository profileRepo,
            ITopologyRepository topoRepo,
            ITemplateRepository templateRepository,
            ILogger<TransferService> logger
        ) {
            _topoRepo = topoRepo;
            _templateRepo = templateRepository;
            _profileRepo = profileRepo;
            _profileResolver = profileResolver;
            _logger = logger;

            jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        private readonly IProfileResolver _profileResolver;
        private readonly IProfileRepository _profileRepo;
        private readonly ITopologyRepository _topoRepo;
        private readonly ITemplateRepository _templateRepo;
        private readonly ILogger<TransferService> _logger;
        private Data.Entities.Profile _user;
        private JsonSerializerSettings jsonSerializerSettings;

        public async Task Export(int[] ids, string src, string dest)
        {
            if (!Profile.IsAdmin)
                throw new InvalidOperationException();

            var list = new List<Data.Entities.Topology>();
            foreach (int id in ids)
            {
                var topo = await _topoRepo.Load(id);
                if (topo != null)
                    list.Add(topo);
            }

            if (ids.Contains(0))
            {
                list.Add(await _topoRepo.LoadAdminTopo());
            }

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
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                File.WriteAllText(
                    Path.Combine(folder, "import.this"),
                    "please import this topology"
                );

                //export data
                topo.Workers.Clear();
                topo.Gamespaces.Clear();
                topo.Id = 0;
                topo.ShareCode = Guid.NewGuid().ToString("N");
                foreach (var template in topo.Templates)
                {
                    template.Id = 0;
                    template.TopologyId = 0;
                }
                File.WriteAllText(
                    Path.Combine(folder, "topo.json"),
                    JsonConvert.SerializeObject(topo, jsonSerializerSettings)
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
                    var tu = new TemplateUtility(template.Detail);
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
            if (!Profile.IsAdmin)
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
                    var topo = JsonConvert.DeserializeObject<Data.Entities.Topology>(
                        File.ReadAllText(dataFile),
                        jsonSerializerSettings
                    );

                    //enforce uniqueness :(
                    var found = await _topoRepo.FindByGlobalId(topo.GlobalId);
                    if (found != null)
                        throw new Exception("Duplicate GlobalId");

                    if (topo.GlobalId == Guid.Empty.ToString())
                    {
                        foreach (var t in topo.Templates)
                        {
                            await _templateRepo.Add(t);
                        }
                    }
                    else
                    {
                        await _topoRepo.Add(topo);
                    }

                    // //import doc
                    // CopyFile(
                    //     Path.Combine(folder, "topo.md"),
                    //     Path.Combine(docPath, topo.GlobalId) + ".md",
                    //     false
                    // );

                    // CopyFolder(
                    //     Path.Combine(folder, "topo.md.images"),
                    //     Path.Combine(docPath, topo.GlobalId),
                    //     false
                    // );

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

        protected Data.Entities.Profile Profile
        {
            get
            {
                if (_user == null)
                {
                    _user = Mapper.Map<Data.Entities.Profile>(_profileResolver.Profile);
                    if (_user.Id == 0 && _user.GlobalId.HasValue())
                        _user = _profileRepo.FindByGlobalId(_user.GlobalId).Result;
                }
                return _user;
            }
        }
    }
}
