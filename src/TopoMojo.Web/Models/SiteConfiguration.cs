using System;
using TopoMojo.Abstractions;

namespace TopoMojo.Models
{

    public class ApplicationOptions
    {
        public SiteConfiguration Site { get; set; }
        public FileUploadConfiguration FileUpload { get; set; }
        public PodConfiguration Pod { get; set; }
    }

    public class SiteConfiguration
    {
        public string Name { get; set; }
        public string ExternalUrl { get; set; }
        public EmailConfiguration Email { get; set; }
        public string PodManagerType { get; set; }
        public string DataRepositoryType { get; set; }
        public bool AllowRegistration { get; set; }
        public string AllowedDomains { get; set; }

    }

    public class EmailConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
    }
}