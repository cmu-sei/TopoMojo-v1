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

        public static IApplicationBuilder UseQuerystringBearerToken (
            this IApplicationBuilder builder
        )
        {
            return builder.UseMiddleware<TopoMojo.Middleware.QuerystringBearerTokenMiddleware>();
        }
    }
}
