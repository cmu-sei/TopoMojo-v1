// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class Workspace
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
        public string Author { get; set; }
        public string Audience { get; set; }
        public string WhenCreated { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        public int TemplateLimit { get; set; }
        public bool IsPublished { get; set; }
        public int GamespaceCount { get; set; }
        public Worker[] Workers { get; set; }
        public Template[] Templates { get; set; }
    }

    public class WorkspaceSummary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        public bool IsPublished { get; set; }
        public string Audience { get; set; }
        public string Author { get; set; }
        public string WhenCreated { get; set; }
    }

    public class NewWorkspace
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ChangedWorkspace
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public bool IsPublished { get; set; }
        public string Audience { get; set; }
        public string DocumentUrl { get; set; }
        public int TemplateLimit { get; set; }
    }

    public class WorkspaceInvitation
    {
        public int Id { get; set; }
        public string ShareCode { get; set; }
    }

    public class Worker
    {
        public int Id { get; set; }
        public string PersonName { get; set; }
        public string PersonGlobalId { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
    }

}
