// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class TemplateUtility
    {
        public TemplateUtility(string detail, string diskname = "placeholder")
        {
            if (detail.HasValue())
            {
                _template = JsonSerializer.Deserialize<VmTemplate>(detail);
            }
            else
            {
                diskname = Regex.Replace(diskname, @"[^\w\d]", "-").Replace("--", "-").Trim('-').ToLower();

                _template = new VmTemplate
                {
                    Ram = 4,
                    VideoRam = 0,
                    Cpu = "1x2",
                    Adapters = 1,
                    Eth = new VmNet[] { new VmNet { Net = "lan", Type="e1000" }},
                    Disks = new VmDisk[] { new VmDisk
                    {
                        Path = $"[ds] {Guid.Empty.ToString()}/{diskname}.vmdk",
                        Source = "",
                        Controller = "lsilogic",
                        Size = 10
                    }}
                };
            }
        }

        private VmTemplate _template = null;
        private JsonSerializerOptions jsonOptions => new JsonSerializerOptions
        {
            WriteIndented = true
        };

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
                List<VmNet> nics = _template.Eth.ToList();

                if (nics.Count == 0 || !value.HasValue())
                    return;

                VmNet proto = nics.First();

                string[] nets = value.Split(new char[] { ' ', ',', '\t', '|', ':'}, StringSplitOptions.RemoveEmptyEntries);

                if (nets.Length == 0) nets = new string[]{ "lan" };

                for (int i = nics.Count; i > nets.Length; i--)
                    nics.RemoveAt(i);

                for (int i = 0; i < nets.Length; i++)
                {
                    if (nics.Count < i+1)
                    {
                        nics.Add(new VmNet{
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

        public bool UseUplinkSwitch
        {
            get { return _template.UseUplinkSwitch; }
            set { _template.UseUplinkSwitch = value; }
        }

        public KeyValuePair<string,string>[] GuestSettings
        {
            get { return _template.GuestSettings; }
            set { _template.GuestSettings = value; }
        }

        public void AddGuestSettings(string guestinfo)
        {
            var lines = guestinfo?.Split(
                new char[] {';', '\n', '\r'},
                StringSplitOptions.RemoveEmptyEntries
            ) ?? Array.Empty<string>();

            foreach (var line in lines)
            {
                int x = line.IndexOf('=');

                if (x > 0)
                {
                    string key = line.Substring(0, x).Trim();

                    if (!key.StartsWith("guestinfo."))
                        key = "guestinfo." + key;

                    _template.GuestSettings.Append(
                        new KeyValuePair<string, string>(
                            key,
                            line.Substring(x + 1).Trim()
                        )
                    );
                }
            }
        }

        public void LocalizeDiskPaths(string topologyKey, string templateKey)
        {
            if (!topologyKey.HasValue() || !templateKey.HasValue())
                throw new InvalidOperationException();

            string path = $"[ds] {topologyKey}/{templateKey}";

            int i = 0;

            foreach (VmDisk disk in _template.Disks)
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
            return JsonSerializer.Serialize<VmTemplate>(_template, jsonOptions);
        }

        public VmTemplate AsTemplate()
        {
            return _template;
        }
    }
}
