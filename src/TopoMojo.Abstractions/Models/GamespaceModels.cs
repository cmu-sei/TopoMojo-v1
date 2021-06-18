// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TopoMojo.Models
{
    public class Gamespace
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string ClientId { get; set; }
        public string Audience { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string WhenCreated { get; set; }
        public string WorkspaceDocument { get; set; }
        public int WorkspaceId { get; set; }
        public Player[] Players { get; set; }
    }

    public class GameState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GlobalId { get; set; }
        public string WhenCreated { get; set; }
        public string WorkspaceDocument { get; set; }
        public string Markdown { get; set; }
        public string ShareCode { get; set; }
        public string Audience { get; set; }
        public string LaunchpointUrl { get; set; }
        public Player[] Players { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<VmState> Vms { get; set; } = new List<VmState>();
        public Models.v2.ChallengeView Challenge { get; set; }

    }

    public class VmState
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IsolationId { get; set; }
        public bool IsRunning { get; set; }
        public bool IsVisible { get; set; }
    }

    public class Player
    {
        public int Id { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Permission Permission { get; set; }
        public bool IsManager => Permission == Permission.Manager;
    }

    public class GamespaceSearch: Search
    {
        public const string FilterAll = "all";
        public bool WantsAll => Filter.Contains(FilterAll);
    }
}
