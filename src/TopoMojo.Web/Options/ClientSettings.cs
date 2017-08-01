namespace TopoMojo
{
    public class ClientSettings
    {
        public BrandingOptions branding {get; set;}
        public UserManagerSettings oidc { get; set; }
        public ClientUrlSettings urls { get; set; }
    }

    public class BrandingOptions
    {
        public string applicationName { get; set; }
        public string logoUrl { get; set; }
    }

    public class UserManagerSettings
    {
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