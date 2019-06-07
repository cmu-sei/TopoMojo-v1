// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TopoMojo.Models.Virtual;

namespace TopoMojo.Core
{
    public class TemplateUtility
    {
        public TemplateUtility(string detail, string diskname = "placeholder")
        {
            if (detail.HasValue())
            {
                _template = (Template)JObject.Parse(detail).ToObject(typeof(Template));
            }
            else
            {
                diskname = Regex.Replace(diskname, @"[^\w\d]", "-").Replace("--", "-").Trim('-').ToLower();
                _template = new Template
                {
                    Ram = 4,
                    VideoRam = 0,
                    Cpu = "1x2",
                    Adapters = 1,
                    Eth = new Eth[] { new Eth { Net = "lan", Type="e1000" }},
                    Disks = new Disk[] { new Disk
                    {
                        Path = $"[ds] {Guid.Empty.ToString()}/{diskname}.vmdk",
                        Source = "",
                        Controller = "lsilogic",
                        Size = 10
                    }}
                };
            }
        }

        private Template _template = null;

        public string Id
        {
            get { return _template.Id; }
            set { _template.Id = value; }
        }

        public string Name
        {
            get { return _template.Name; }
            set { _template.Name = value; }
        }

        public string IsolationTag
        {
            get { return _template.IsolationTag; }
            set { _template.IsolationTag = value; }
        }

        public string Networks
        {
            get { return String.Join(", ", _template.Eth.Select(e => e.Net)); }
            set
            {
                List<Eth> nics = _template.Eth.ToList();
                if (nics.Count == 0 || !value.HasValue())
                    return;
                Eth proto = nics.First();

                string[] nets = value.Split(new char[] { ' ', ',', '\t', '|', ':'}, StringSplitOptions.RemoveEmptyEntries);
                if (nets.Length == 0) nets = new string[]{ "lan" };

                for (int i = nics.Count; i > nets.Length; i--)
                    nics.RemoveAt(i);

                for (int i = 0; i < nets.Length; i++)
                {
                    if (nics.Count < i+1)
                    {
                        nics.Add(new Eth{
                            Type = proto.Type,
                        });
                    }
                    nics[i].Net = nets[i];
                }
                _template.Eth = nics.ToArray();
            }
        }

        public string Iso
        {
            get { return _template.Iso; }
            set { _template.Iso = value; }
        }

        public void LocalizeDiskPaths(string topologyKey, string templateKey)
        {
            if (!topologyKey.HasValue() || !templateKey.HasValue())
                throw new InvalidOperationException();

            string path = $"[ds] {topologyKey}/{templateKey}";
            int i = 0;
            foreach (Disk disk in _template.Disks)
            {
                if (!disk.Path.StartsWith(path))
                {
                    disk.Source = disk.Path;
                    disk.Path = $"{path}_{i}.vmdk";
                }
                i += 1;
            }
        }

        public override string ToString()
        {
            return JObject.FromObject(_template).ToString();
        }

        public Template AsTemplate()
        {
            return _template;
        }
    }
}
