// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo.Models
{
    public class Workspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Audience { get; set; }
        public DateTime WhenCreated { get; set; }
        public int GamespaceCount { get; set; }
        public string Challenge { get; set; }
        public Worker[] Workers { get; set; }
        public Template[] Templates { get; set; }
    }

    public class WorkspaceSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Audience { get; set; }
        public string Author { get; set; }
        public DateTime WhenCreated { get; set; }
    }

    public class NewWorkspace
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Audience { get; set; }
        public string Author { get; set; }
        public string Challenge { get; set; }
        public string Document { get; set; }
    }

    public class ChangedWorkspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Audience { get; set; }
        public int TemplateLimit { get; set; }
    }

    public class WorkspaceInvitation
    {
        public string Id { get; set; }
        public string ShareCode { get; set; }
    }

    public class Worker
    {
        public string WorkspaceId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
        public Permission Permission { get; set; }
        public bool CanManage => Permission.HasFlag(Permission.Manager);
        public bool CanEdit => Permission.HasFlag(Permission.Editor);
    }

    public class WorkspaceSearch : Search
    {
        public string aud { get; set; }
        public bool WantsAudience => string.IsNullOrEmpty(aud).Equals(false);
    }

    public class ClientAudience
    {
        public string Scope { get; set; }
        public string Audience { get; set; }
    }
}
