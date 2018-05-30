using System;

namespace TopoMojo.Core
{
    public class GamespaceLimitException : Exception { }
    public class WorkspaceLimitException : Exception { }
    public class WorkspaceTemplateLimitException : Exception { }
    public class ParentTemplateException : Exception { }
    public class WorkspaceNotIsolatedException : Exception { }
}
