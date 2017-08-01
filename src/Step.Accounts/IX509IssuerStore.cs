using System.Security.Cryptography.X509Certificates;

namespace Step.Accounts
{
    public interface IX509IssuerStore
    {
        void Load();
        void Add(X509Certificate2 certificate);
        bool Validate(X509Certificate2 certificate);
    }
}