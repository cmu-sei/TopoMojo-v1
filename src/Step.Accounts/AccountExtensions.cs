using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Step.Accounts
{
    public static class AccountExtensions
    {
        public static string GeneratePasswordHash(this Account account, string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(account.GlobalId + password)))
                    .Replace("-", "").ToLower();
            }
        }

        public static bool VerifyPasswordHash(this Account account, string password)
        {
            return account.Tokens.Where(o => o.Type == AccountTokenType.Password).Any()
                && account.CurrentPassword().Hash == account.GeneratePasswordHash(password);
        }

        public static AccountToken CurrentPassword(this Account account)
        {
            return account.Tokens.Where(o => o.Type == AccountTokenType.Password)
                .OrderByDescending(o=>o.WhenCreated)
                .FirstOrDefault();
        }

        public static bool IsLocked(this Account account)
        {
            return account.WhenLocked.AddMinutes(account.LockedMinutes) > DateTime.UtcNow;
        }

        public static string LockDuration(this Account account)
        {
            TimeSpan ts = account.WhenLocked.AddMinutes(account.LockedMinutes).Subtract(DateTime.UtcNow);
            return ts.ToString("hh\\:mm\\:ss");
        }
    }
}