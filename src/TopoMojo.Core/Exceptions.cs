// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo
{
    public class GamespaceLimitReachedException : Exception { }
    public class WorkspaceLimitReachedException : Exception { }
    public class TemplateLimitReachedException : Exception { }
    public class ParentTemplateException : Exception { }
    public class WorkspaceNotIsolatedException : Exception { }
    public class ActionForbiddenException: Exception { }
}
