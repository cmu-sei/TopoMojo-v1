namespace Step.Accounts
{
    public class AccountOptions
    {
        public PasswordOptions Password { get; set; } = new PasswordOptions();
        public RegistrationOptions Registration { get; set; } = new RegistrationOptions();
        public AuthenticationOptions Authentication { get; set; } = new AuthenticationOptions();
        public TokenOptions Token { get; set; } = new TokenOptions();
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
        public bool RequireConfirmation { get; set; } = false;
        public string IssuerCertificatesPath { get; set; } = "issuers";
        public string CertificateSubjectIdRegex { get; set; } = @"CN=\S*(\d{10})";
        public string CertificateSubjectNameRegex { get; set; } = @"CN=(\S*)\.\d";
    }

    public class AuthenticationOptions
    {
        public bool RequireCertificate { get; set; } = false;
        public bool Require2FA { get; set; } = false;
        public bool CheckCertificateRevocation { get; set; } = false;
        public int LockThreshold { get; set; } = 5;

        public bool AllowLocalLogin = true;
        public bool AllowRememberLogin = true;
        public int RememberMeLoginDays = 30;

        public bool ShowLogoutPrompt = true;
        public bool AutomaticRedirectAfterSignOut = false;

        public bool WindowsAuthenticationEnabled = true;
        public readonly string[] WindowsAuthenticationSchemes = new string[] { "Negotiate", "NTLM" };
        public readonly string WindowsAuthenticationDisplayName = "Windows";
        public string SigningCertificate { get; set; }
        public string SigningCertificatePassword { get; set; }

        public string ProxiedCertificateHeader { get; set; } = "X-ARR-ClientCert";
        public string ValidatedSubjectHeader { get; set; } = "X-Validated-Subject";
    }

    public class TokenOptions
    {
        public string Scheme { get; set; } = "app-scheme";
        public string Key { get; set; } = "app-token-secret-key";
        public string Issuer { get; set; } = "app-issuer";
        public string Audience { get; set; } = "app-audience";
        public int ExpirationMinutes { get; set; } = 60;
    }

}