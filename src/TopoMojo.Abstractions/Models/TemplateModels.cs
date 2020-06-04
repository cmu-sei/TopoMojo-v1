// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class Template
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public bool CanEdit { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public string Guestinfo { get; set; }
        public bool IsHidden { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceGlobalId { get; set; }
    }

    public class ChangedTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public bool IsHidden { get; set; }
        public string Guestinfo { get; set; }
        public int WorkspaceId { get; set; }
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
        public int Id { get; set; }
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
        public string Name { get; set; }
        public string Description { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TemplateLink
    {
        public int TemplateId { get; set; }
        public int WorkspaceId { get; set; }
    }

    public class ConvergedTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Networks { get; set; }
        public string Iso { get; set; }
        public string Detail { get; set; }
        public string Guestinfo { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceGlobalId { get; set; }
        public bool WorkspaceUseUplinkSwitch { get; set; }
    }
}
