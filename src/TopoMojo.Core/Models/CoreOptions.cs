// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Core
{
    public class CoreOptions
    {
        public int ConcurrentInstanceMaximum { get; set; } = 2;
        public int DefaultWorkspaceLimit { get; set; } = 0;
        public int WorkspaceTemplateLimit { get; set; } = 3;
        public int DefaultNetServerTemplateId { get; set; } = 421;
        public int GameEngineMaxReplicas { get; set; } = 5;
        public string GameEngineIsoFolder { get; set; } = "static";
        public string ConsoleHost { get; set; }
        public string EngineKey { get; set; }
    }
}
