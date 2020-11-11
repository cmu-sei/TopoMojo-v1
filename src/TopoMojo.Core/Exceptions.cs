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
    public class InvalidClientAudience: Exception { }
    public class ResourceNotFound: Exception { }
    public class GamespaceNotRegistered: Exception {
        public GamespaceNotRegistered() : base() { }
        public GamespaceNotRegistered(string message) : base(message) { }
        public GamespaceNotRegistered(string message, Exception ex) : base(message, ex) { }
    }
}
