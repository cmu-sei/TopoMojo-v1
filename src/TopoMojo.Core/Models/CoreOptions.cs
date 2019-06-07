// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Core
{
    public class CoreOptions
    {
        public int ConcurrentInstanceMaximum { get; set; } = 2;
        public int DefaultWorkspaceLimit { get; set; } = 0;
        public int WorkspaceTemplateLimit { get; set; } = 3;
    }
}
