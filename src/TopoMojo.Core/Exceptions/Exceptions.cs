// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo.Core
{
    public class GamespaceLimitException : Exception { }
    public class WorkspaceLimitException : Exception { }
    public class WorkspaceTemplateLimitException : Exception { }
    public class ParentTemplateException : Exception { }
    public class WorkspaceNotIsolatedException : Exception { }
}
