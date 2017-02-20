namespace TopoMojo.Models
{
    public class IdentityOptions
    {
        public PasswordOptions Password { get; set; } = new PasswordOptions();
        public RegistrationOptions Registration { get; set; } = new RegistrationOptions();
        public AuthenticationOptions Authentication { get; set; } = new AuthenticationOptions();
    }

    public class PasswordOptions
    {
        public string ComplexityExpression { get; set; } = @"(?=^.{10,}$)(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[`~!@#$%^&*\(\)\-_=+\[\]\{\}\\|;:'"",<\.>/?\ ]).*$";
        public string ComplexityText { get; set; } = "At least 10 characters containing uppercase and lowercase letters, numbers, and symbols";
        public int History { get; set; }
        public int Age { get; set; }
        public int ResetTokenExpirationMinutes { get; set; } = 60;
    }

    public class RegistrationOptions
    {
        public bool AllowManual { get; set; } = false;
        public string AllowedDomains { get; set; } = "";
        // public bool AllowCertificate { get; set; } = true;
        // public string AllowedCertificates { get; set; } = "";
        // public bool AutoRegisterCertificates { get; set; } = true;
        public string IssuerCertificatesPath { get; set; } = "issuers";
        public string CertificateSubjectIdRegex { get; set; } = @"CN=\S*(\d{10})";
        public string CertificateSubjectNameRegex { get; set; } = @"CN=(\S*)\.\d";
    }

    public class AuthenticationOptions
    {
        public bool RequireCertificate { get; set; } = false;
        public bool CheckCertificateRevocation { get; set; } = false;
        public int LockThreshold { get; set; } = 5;
        public string Scheme { get; set; } = "app-identity";
        public string TokenKey { get; set; } = "app-token-secret-key";
        public string TokenIssuer { get; set; } = "app-issuer";
        public string TokenAudience { get; set; } = "app-audience";
        public int TokenExpirationMinutes { get; set; } = 60;
        public int CookieExpirationMinutes { get; set; } = 20;

    }
}