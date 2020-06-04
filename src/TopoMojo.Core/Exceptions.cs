// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo
{
    public class GamespaceLimitException : Exception { }
    public class WorkspaceLimitReachedException : Exception { }
    public class WorkspaceTemplateLimitException : Exception { }
    public class ParentTemplateException : Exception { }
    public class WorkspaceNotIsolatedException : Exception { }
}
