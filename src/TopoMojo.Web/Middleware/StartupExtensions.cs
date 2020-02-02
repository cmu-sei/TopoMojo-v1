// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace Microsoft.AspNetCore.Builder
{
    public static class StartUpExtensions
    {
        public static IApplicationBuilder UseHeaderInspection (
            this IApplicationBuilder builder,
            bool enabled
        )
        {
            return builder.UseMiddleware<TopoMojo.Middleware.HeaderInspectionMiddleware>(enabled);
        }

    }
}
