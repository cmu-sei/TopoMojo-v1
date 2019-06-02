// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace TopoMojo.Models
{
    public class PodConfiguration {
        public bool IsVCenter { get; set; }
        public string Type { get; set; }
        public string Url { get; set;}  //accepts range expansion
        public string Host { get; set;}
        public string User { get; set; }
        public string Password { get; set; }
        public string PoolPath { get; set; }
        public string Uplink { get; set; }
        public string VmStore { get; set; }
        public string DiskStore { get; set; }
        public string IsoStore { get; set; }
        // public string StockStore { get; set; }
        public string DisplayMethod { get; set; }
        public string DisplayUrl { get; set; }
        public string TicketUrlHandler { get; set; }  = "none"; //"local-app", "external-domain", "host-map", "none"
        public Dictionary<string,string> TicketUrlHostMap { get; set; } = new Dictionary<string, string>();
        public VlanOptions Vlan { get; set; }
        public int ConnectionKeepAliveTimeoutMinutes { get; set; } = 10;
    }

    public class TemplateOptions {
        public string[] Cpu { get; set; }
        public string[] Ram { get; set; }
        public string[] Adapters { get; set; }
        public string[] Guest { get; set; }
        public string[] Iso { get; set; }
        public string[] Source { get; set; }
        public string[] Palette { get; set; }
    }

    public class VmOptions {
        public string[] Iso { get; set; }
        public string[] Net { get; set; }
    }

    public class VlanOptions
    {
        public string Range { get; set; }
        public Vlan[] Reservations { get; set; } = new Vlan[] {};
    }

    public class Vlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool OnUplink { get; set; }
    }

    public class TaskStatus
    {
        public string Id { get; set; }
        public int Progress { get; set; }
    }

    public class KeyValuePair{
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
