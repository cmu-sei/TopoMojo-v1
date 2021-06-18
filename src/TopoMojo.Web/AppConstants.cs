// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

namespace TopoMojo
{
    internal static class AppConstants
    {
        public const string Audience = "topomojo-api";
        public const string PrivilegedAudience = "topomojo-api-privileged";
        public const string PrivilegedPolicy = "topomojo-api-privileged";
        public const string DataProtectionPurpose = "_dp:TopoMojo";
        public const string SubjectClaimName = "sub";
        public const string NameClaimName = "name";
        public const string NameIdClaimName = "nameid";
        public const string RoleClaimName = "role";
        public const string ClientIdClaimName = "client_id";
        public const string ClientScopeClaimName = "client_scope";
        public const string ClientUrlClaimName = "client_url";
        public const string RegistrationCachePrefix = "lp:";
        public const string CookieScheme = ".TopoMojo.Cookies";
        public const string MarkdownCutLine = "<!-- cut -->";

    }

    internal static class AuditId
    {

    }

}
