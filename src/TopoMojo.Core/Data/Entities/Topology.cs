// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class Topology : IEntity
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
        public string Description { get; set; } // = string.Empty;
        public string DocumentUrl { get; set; } // = string.Empty;
        public string Author { get; set; } // = string.Empty;
        public string Audience { get; set; }
        public string ShareCode { get; set; }
        public bool IsPublished { get; set; }
        public int TemplateLimit { get; set; }
        public bool UseUplinkSwitch { get; set; }
        public int LaunchCount { get; set; }
        public DateTime LastActivity { get; set; }
        public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public virtual ICollection<Gamespace> Gamespaces { get; set; } = new List<Gamespace>();
        public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

        [NotMapped]
        public string Document
        {
            get {
                return (string.IsNullOrEmpty(this.DocumentUrl))
                        ? "/docs/" + this.GlobalId + ".md"
                        : this.DocumentUrl;
            }
        }
    }
}
