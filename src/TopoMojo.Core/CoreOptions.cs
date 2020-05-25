// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo
{
    public class CoreOptions
    {
        public int GamespaceLimit { get; set; } = 2;
        public int DefaultWorkspaceLimit { get; set; } = 0;
        public int DefaultTemplateLimit { get; set; } = 3;
        public int NetworkHostTemplateId { get; set; } = 421;
        public int ReplicaLimit { get; set; } = 5;
        public string GameEngineIsoFolder { get; set; } = "static";
        public string ConsoleHost { get; set; }
        public string DemoCode { get; set; }
        public JanitorOptions Expirations { get; set; } = new JanitorOptions();

    }

    public class JanitorOptions
    {
        public bool DryRun { get; set; } = true;

        // gamespace vm's deleted 12 hours after last activity
        public string IdleGamespaceExpiration { get; set; } = "12h";

        // workspace vm's deleted 12 hours after last activity
        public string IdleWorkspaceVmsExpiration { get; set; } = "1d";

        // published workspaces deleted 1y after no launches
        public string InactiveWorkspaceExpiration { get; set; } = "1y";

        // unpublished workspaces deleted 7d after last activity
        public string UnpublishedWorkspaceTimeout { get; set; } = "1w";
    }

}
