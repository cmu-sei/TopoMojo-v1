// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;

namespace TopoMojo.Models
{
    public class Template
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public string Guestinfo { get; set; }
        public bool IsHidden { get; set; }
        public int Replicas { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceGlobalId { get; set; }
    }

    public class ChangedTemplate
    {
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public bool IsHidden { get; set; }
        public string Guestinfo { get; set; }
        public int Replicas { get; set; }
    }

    public class NewTemplateDetail
    {
        public string Name { get; set; }
        public string Networks { get; set; }
        public string Detail { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TemplateDetail
    {
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Guestinfo { get; set; }
        public string Detail { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TemplateSummary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceGlobalId { get; set; }
        public string WorkspaceName { get; set; }
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TemplateLink
    {
        public string TemplateId { get; set; }
        public string WorkspaceId { get; set; }
    }

    public class ConvergedTemplate
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public string Detail { get; set; }
        public string Guestinfo { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceGlobalId { get; set; }
        public bool WorkspaceUseUplinkSwitch { get; set; }
        public int Replicas { get; set; }
    }

    public class TemplateSearch: Search
    {
        public const string PublishFilter = "published";
        public const string ParentFilter = "parents";
        public bool WantsPublished => Filter.Contains(PublishFilter);
        public bool WantsParents => Filter.Contains(ParentFilter);
        public string pid { get; set; }
    }
}
