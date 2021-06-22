// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using TopoMojo.Api.Extensions;

namespace TopoMojo.Api.Models
{
    public class Workspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Audience { get; set; }
        public DateTime WhenCreated { get; set; }
        public int TemplateLimit { get; set; }
        public string Challenge { get; set; }
        public Worker[] Workers { get; set; }
        public Template[] Templates { get; set; }
    }

    public class WorkspaceSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug => Name.ToSlug();
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

    public class RestrictedChangedWorkspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Audience { get; set; }
    }

    public class ChangedWorkspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Audience { get; set; }
        public string TemplateScope { get; set; }
        public int TemplateLimit { get; set; }
    }

    public class JoinCode
    {
        public string Id { get; set; }
        public string Code { get; set; }
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

    public class WorkspaceStats
    {
        public string Id { get; set; }
        public int ActiveGamespaceCount { get; set; }
        public int LaunchCount { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
