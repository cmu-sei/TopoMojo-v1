// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TopoMojo.Models
{
    public class Gamespace
    {
        public string Id { get; set; }
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
        public string Id { get; set; }
        public string Name { get; set; }
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
        public ChallengeView Challenge { get; set; }

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
        public string GamespaceId { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Permission Permission { get; set; }
        public bool IsManager => Permission == Permission.Manager;
    }

    public class GamespaceSearch: Search
    {
        public const string FilterAll = "all";
        public const string FilterActive = "active";
        public bool WantsAll => Filter.Contains(FilterAll);
        public bool WantsActive => Filter.Contains(FilterActive);

    }

    public class GamespaceRegistration
    {
        public string ResourceId { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int Variant { get; set; }
        public int MaxAttempts { get; set; }
        public int MaxMinutes { get; set; }
        public int Points { get; set; } = 100;
        public bool AllowReset { get; set; }
        public bool AllowPreview { get; set; }
        public bool StartGamespace { get; set; }
        public DateTime ExpirationTime { get; set; }
        public RegistrationPlayer[] Players { get; set; } = new RegistrationPlayer[] {};
    }

    public class RegistrationPlayer
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
    }
}
