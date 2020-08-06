// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;

namespace TopoMojo.Models
{
    public class NewGamespace
    {
        public string Id { get; set; }
        public GamespaceSpec Workspace { get; set; }
    }

    public class GamespaceSpec
    {
        public string IsolationId { get; set; }
        [Obsolete]
        public int WorkspaceId { get; set; }
        public string WorkspaceGuid { get; set; }
        public VmSpec[] Vms { get; set; } = new VmSpec[] {};
        public bool CustomizeTemplates { get; set; }
        public string Templates { get; set; }
        public string Iso { get; set; }
        public string IsoTarget { get; set; }
        public bool HostAffinity { get; set; }
        public bool AppendMarkdown { get; set; }
    }

    public class VmSpec
    {
        public string Name { get; set; }
        public int Replicas { get; set; }
        public bool SkipIso { get; set; }
    }

    public class NetworkServerSpec
    {
        public string IpAddress { get; set; }
        public string[] Dnsmasq { get; set; } = new string[] {};
        public string[] HostFileEntries { get; set; } = new string[] {};
    }

    public class VmAction
    {
        public string Id { get; set; }
        public string IsolationId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }

     public class IsoBuildSpec
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public string[] Files { get; set; }
    }

}
