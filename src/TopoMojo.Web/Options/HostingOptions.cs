namespace TopoMojo
{
    public class HostingOptions
    {
        // List of urls to listen on (i.e. http://localhost:5000)
        public string[] Urls { get; set; }

        // Filepath of server certificate (for https)
        public string Certificate { get; set; }

        // Password for certificate key, if any
        public string CertificatePassword { get; set; }
    }
}