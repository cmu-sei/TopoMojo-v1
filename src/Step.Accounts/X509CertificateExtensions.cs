using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Step.Accounts.Extensions
{
    public static class X509CertificateExtensions
    {
        public static string ParseSubject(this X509Certificate2 certificate, string pattern)
        {
            Match match = Regex.Match(certificate.Subject, pattern);
            return match.Groups[match.Groups.Count-1].Value;
        }
    }
}