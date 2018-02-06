namespace TopoMojo
{
    public class ClientSettings
    {
        public BrandingOptions Branding {get; set;}
        public UserManagerSettings Oidc { get; set; }
        public ClientUrlSettings Urls { get; set; }
        public LoginOptions Login { get; set; }
        public string Lang { get; set; }
        public string MaintenanceMessage { get; set; }
    }

    public class BrandingOptions
    {
        public string ApplicationName { get; set; }
        public string LogoUrl { get; set; }
    }

    public class LoginOptions
    {
        public bool AllowLocalLogin { get; set; } = true;
        public bool AllowExternalLogin { get; set; } = true;
        public string AllowedDomains { get; set; }
        public string PasswordComplexity { get; set; }
    }

    public class UserManagerSettings
    {
        public string name { get; set; }
        public string authority { get; set; }
        public string client_id { get; set; }
        public string redirect_uri { get; set; }
        public string post_logout_redirect_uri { get; set; }
        public string response_type { get; set; }
        public string scope { get; set; }
        public bool filterProtocolClaims { get; set; }
        public bool loadUserInfo { get; set; }
        public bool automaticSilentRenew { get; set; }
        public string silent_redirect_uri { get; set; }
        public int silentRequestTimeout { get; set; }
        public bool monitorSession { get; set; }
        public int checkSessionInterval { get; set; }
        public int accessTokenExpiringNotificationTime { get; set; }

    }

    public class ClientUrlSettings
    {
        public string apiUrl { get; set; }
    }
}