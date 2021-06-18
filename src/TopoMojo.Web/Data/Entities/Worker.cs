// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo.Data
{
    public class Worker
    {
        public int Id { get; set; }
        public string WorkspaceGlobalId { get; set; }
        public virtual Workspace Workspace { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Permission Permission { get; set; }
    }
}
