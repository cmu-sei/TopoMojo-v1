namespace TopoMojo
{
    public class AuthorizationOptions
    {
        public string Authority { get; set; }
        public string AuthorizationUrl { get; set; }
        public string AuthorizationScope { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }
        public bool RequireHttpsMetadata { get; set; }

    }
}