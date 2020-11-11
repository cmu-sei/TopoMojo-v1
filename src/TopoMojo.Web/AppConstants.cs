// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

namespace TopoMojo.Web
{
    internal static class AppConstants
    {
        public const string Audience = "topomojo-api";
        public const string PrivilegedAudience = "topomojo-api-privileged";
        public const string PrivilegedPolicy = "topomojo-api-privileged";
        public const string DataProtectionPurpose = "_dp:TopoMojo";
        public const string SubjectClaimName = "sub";
        public const string NameClaimName = "name";
        public const string RegistrationCachePrefix = "lp:";
    }

    internal static class AuditId
    {

    }

}
