using System;
using System.Linq;

namespace TopoMojo.Data.Extensions
{
    public static class WorkspaceExtensions
    {
        public static bool HasScope(this Topology workspace, string scope)
        {
            var delims = new char[] { ' ', ',', ';' };

            var client = scope.Split(delims, StringSplitOptions.RemoveEmptyEntries);

            var resource = workspace.Audience.Split(delims, StringSplitOptions.RemoveEmptyEntries);

            return client.Intersect(resource).Any();
        }
    }
}
