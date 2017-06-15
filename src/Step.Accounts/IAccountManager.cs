using System;
//using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Step.Accounts
{
    public interface IAccountManager : IAccountManager<Account> { }
    public interface IAccountManager<TAccount>
    {
        Task AddAccountAsync(int userId, Credentials creds);
        Task<TAccount> AuthenticateWithCertificateAsync(X509Certificate2 certificate, string location);
        Task<TAccount> AuthenticateWithCodeAsync(Credentials creds);
        Task<TAccount> AuthenticateWithCredentialAsync(Credentials creds, string location);
        Task<TAccount> AuthenticateWithResetAsync(Credentials creds);
        Task<TAccount> AuthenticateWithValidatedSubjectAsync(string subject, string location);
        Task<TAccount> FindAsync(int id);
        Task<TAccount> FindByAccountAsync(string account);
        Task<TAccount> FindByGuidAsync(string guid);
        Task<int> GenerateAccountCodeAsync(string account);
        Task<string> GenerateAuthenticationTokenAsync(int userId);
        object GenerateJwtToken(TAccount user);
        bool IsDomainValid(string account);
        bool IsExpired(DateTime dt);
        bool IsPasswordComplex(string password);
        Task<bool> IsTokenUniqueAsync(string account);
        Task<TAccount> RegisterWithCredentialsAsync(Credentials credentials);
        Task<TAccount> RegisterWithCredentialsAsync(Credentials credentials, string globalId);
        Task<TAccount> RegisterWithCertificateAsync(X509Certificate2 certificate);
        Task<TAccount> SetStatus(int userId, AccountStatus status);
        Task UpdatePasswordAsync(int userId, string password);
        Task<bool> ValidateAccountCodeAsync(string account, int code);
        // Task<bool> AddCredentialsToAccount(int userId, string username, string confirmUsername, string password, string confirmPassword);
        // Task<bool> UpdateUsername(int userId, string existingUsername, string newUsername, string confirmNewUsername);
        // Task<bool> DeleteUsername(int userId, string username);
    }

    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Code { get; set; }
    }
}