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
            this IApplicationBuilder builder,
            string tokenName = "access_token"
        )
        {
            return builder.UseMiddleware<TopoMojo.Middleware.QuerystringBearerTokenMiddleware>(tokenName);
        }
    }
}
