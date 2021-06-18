// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo
{
    public class ClientGamespaceLimitReached : Exception { }
    public class PlayerGamespaceLimitReached : Exception { }
    public class WorkspaceLimitReached : Exception { }
    public class TemplateLimitReached : Exception { }
    public class TemplateHasDescendents : Exception { }
    public class TemplateNotPublished : Exception { }
    public class WorkspaceNotIsolated : Exception { }
    public class ActionForbidden: Exception { }
    public class AuthenticationFailedException : Exception {}
    public class InvalidClientAudience: Exception { }
    public class ResourceNotFound: Exception { }
    public class ResourceIsLocked: Exception { }
    public class SessionLimitReached: Exception { }
    public class AttemptLimitReached: Exception { }
    public class GamespaceIsExpired: Exception { }
    public class GamespaceNotRegistered: Exception {
        public GamespaceNotRegistered() : base() { }
        public GamespaceNotRegistered(string message) : base(message) { }
        public GamespaceNotRegistered(string message, Exception ex) : base(message, ex) { }
    }
}
