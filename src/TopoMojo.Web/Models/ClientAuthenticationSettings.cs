namespace TopoMojo.Web
{
    public class ClientAuthenticationSettings
    {
        public string authority { get; set; }
        public string client_id { get; set; }
        public string redirect_uri { get; set; }
        public string post_logout_redirect_uri { get; set; }
        public string response_type { get; set; }
        public string scope { get; set; }
        public bool automaticSilentRenew { get; set; }
        public string silent_redirect_uri { get; set; }
        public bool filterProtocolClaims { get; set; }
        public bool loadUserInfo { get; set; }

        public string apiUrl { get; set; }

//     const settings: UserManagerSettings = {
//     authority: 'http://localhost:5000',
//     client_id: 'sketch-browser',
//     redirect_uri: 'http://localhost:5002/auth',
//     post_logout_redirect_uri: 'http://localhost:5002',
//     response_type: 'id_token token',
//     scope: 'openid profile sketch-api',
//     automaticSilentRenew: false,
//     silent_redirect_uri: 'http://localhost:5000',
//     //silentRequestTimeout:10000,
//     filterProtocolClaims: true,
//     loadUserInfo: true
//     //userStore: new WebStorageStateStore({})
// };
    }
}