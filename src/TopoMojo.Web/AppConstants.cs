// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

namespace TopoMojo.Web
{
    internal static class AppConstants
    {
        public const string Audience = "topomojo-api";
        public const string PrivilegedAudience = "topomojo-api-privileged";
        public const string PrivilegedPolicy = "topomojo-api-privileged";
        public const string ManagerRole = "manager";
        public const string AdminRole = "administrator";
        public const string DataProtectionPurpose = "_dp:TopoMojo";

    }

    internal static class AuditId
    {
        public const int RegisteredCertificate = 1000;
        public const int RegisteredCredential = 1001;
        public const int LoginCertificate = 1002;
        public const int LoginCredential = 1003;
        public const int LoginExternal = 1004;
        public const int ResetPassword = 1005;
        public const int MergeAccount = 1006;
        public const int AcceptInvite = 1007;
        public const int GenerateTotp = 1008;
        public const int ClientState = 1009;
        public const int ClientDelete = 1010;
        public const int ResourceState = 1011;
        public const int ResourceDelete = 1012;
        public const int UserRole = 2000;
        public const int UserState = 2001;
        public const int ClearLock = 2002;

    }

}
