using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using Step.Accounts.Extensions;

namespace Step.Accounts
{
    public class AccountManager : AccountManager<Account>
    {
        public AccountManager(
            AccountDbContext db,
            AccountOptions options,
            ILoggerFactory mill,
            X509IssuerStore issuerStore,
            ITokenService tokenService,
            IProfileService profileService
        )
        : base(db, options, mill, issuerStore, tokenService, profileService)
        {
        }
    }

    /// <summary>
    /// The AccountManager implements functionality to manage user
    /// registration and authentication. It is bound to an EntityFrameworkCore
    /// datastore.
    /// </summary>
    /// <typeparam name="TAccount">Inherits from Account</typeparam>
    public class AccountManager<TAccount> : IAccountManager<TAccount> where TAccount : Account, new()
    {
        public AccountManager(
            AccountDbContext db,
            AccountOptions options,
            ILoggerFactory mill,
            IX509IssuerStore issuerStore,
            ITokenService tokenService,
            IProfileService profileService
        ){
            _db = db;
            _options = options;
            _logger = mill.CreateLogger(this.GetType());
            _certStore = issuerStore ?? new X509IssuerStore(options, new X509IssuerStoreOptions(), mill);
            _profileService = profileService ?? new DefaultProfileService();
            _tokenService = tokenService ?? new DefaultTokenService(options.Token, _profileService);
            _rand = new Random();
        }

        protected readonly AccountDbContext _db;
        protected readonly AccountOptions _options;
        protected readonly ILogger _logger;
        protected readonly IX509IssuerStore _certStore;
        protected readonly ITokenService _tokenService;
        protected readonly IProfileService _profileService;
        protected Random _rand;

        #region Registration

        public async Task<TAccount> RegisterWithCredentialsAsync(Credentials credentials)
        {
            return await RegisterWithCredentialsAsync(credentials, Guid.NewGuid().ToString());
        }

        public async Task<TAccount> RegisterWithCredentialsAsync(Credentials credentials, string globalId)
        {
            if (!IsDomainValid(credentials.Username))
                throw new RegistrationDomainException();

            if (!IsPasswordComplex(credentials.Password))
                throw new PasswordComplexityException();

            if (_options.Registration.RequireConfirmation
                && !await ValidateAccountCodeAsync(credentials.Username, credentials.Code))
                throw new AccountNotConfirmedException();

            TAccount user = await Register(credentials.Username, credentials.Username, AccountTokenType.Credential, globalId);

            await UpdatePasswordAsync(user, credentials.Password);

            return user;
        }

        public async Task<TAccount> RegisterWithCertificateAsync(X509Certificate2 certificate)
        {
            _certStore.Validate(certificate);  //throws on invalid

            string account = certificate.ParseSubject(_options.Registration.CertificateSubjectIdRegex);
            string name = certificate.ParseSubject(_options.Registration.CertificateSubjectNameRegex);
            return await Register(account, name, AccountTokenType.Certificate);
        }

        protected async Task<TAccount> Register(string account, string name, AccountTokenType type, string globalId = "")
        {
            if (!IsTokenUniqueAsync(account).Result)
                throw new AccountNotUniqueException();

            TAccount user = new TAccount {
                GlobalId = (globalId.HasValue()) ? globalId : Guid.NewGuid().ToString(),
                WhenCreated = DateTime.UtcNow,
            };

            user.Tokens.Add(new AccountToken {
                Hash = account.ToHash(),
                WhenCreated = DateTime.UtcNow,
                Type = type
            });

            _db.Accounts.Add(user);
            await _db.SaveChangesAsync();
            await _profileService.AddProfileAsync(user.GlobalId, name);
            return user;
        }

        #endregion

        #region Authentication

        public async Task<TAccount> AuthenticateWithCredentialAsync(Credentials creds, string location)
        {
            if (_options.Authentication.RequireCertificate)
                throw new AuthenticationFailureException();

            if (_options.Authentication.Require2FA
                && ! await ValidateAccountCodeAsync(creds.Username, creds.Code))
                throw new AuthenticationFailureException();

            return await AuthenticateAsync(creds.Username, creds.Password, location);
        }

        public async Task<TAccount> AuthenticateWithCertificateAsync(X509Certificate2 certificate, string location)
        {
            _certStore.Validate(certificate);  //throws on invalid
            return await AuthenticateWithValidatedSubjectAsync(certificate.Subject, location);
        }

        public async Task<TAccount> AuthenticateWithValidatedSubjectAsync(string subject, string location)
        {
            string account = subject.Extract(_options.Registration.CertificateSubjectIdRegex);
            string name = subject.Extract(_options.Registration.CertificateSubjectNameRegex);

            bool registered = await FindByAccountAsync(account) != null;
            if (!registered)
                await Register(account, name, AccountTokenType.Certificate);

            return await AuthenticateAsync(account, null, location);
        }

        protected async Task<TAccount> AuthenticateAsync(string accountId, string password, string location)
        {
            string hash = accountId.ToHash();
            AccountToken token = await _db.AccountTokens
                .Include(o=>o.User)
                .ThenInclude(p=>p.Tokens)
                .Where(o => o.Hash == hash)
                .SingleOrDefaultAsync();

            if (token == null)
                throw new AuthenticationFailureException();

            if (token.User.Status == AccountStatus.Disabled)
                throw new AccountDisabledException();

            if (token.User.IsLocked())
            {
                //TODO: send hh:mm:ss until lock released
                _logger.LogDebug("{0} lock duration is {1}", accountId, token.User.LockDuration());
                throw new AccountLockedException();
            }

            if (token.Type == AccountTokenType.Credential)
            {
                if (token.User.VerifyPasswordHash(password))
                {
                    Unlock((TAccount)token.User);
                    if (_options.Password.Age > 0
                        && DateTime.UtcNow.Subtract(token.User.CurrentPassword().WhenCreated).TotalDays > _options.Password.Age)
                    {
                        throw new PasswordExpiredException();
                    }
                }
                else
                {
                    token.User.AuthenticationFailures += 1;
                    if (_options.Authentication.LockThreshold > 0
                        && token.User.AuthenticationFailures >= _options.Authentication.LockThreshold)
                    {
                        Lock((TAccount)token.User);
                    }
                    await _db.SaveChangesAsync();
                    throw new AuthenticationFailureException();
                }
            }

            MarkUserAuthenticated((TAccount)token.User, location);
            OnUserAuthenticated((TAccount)token.User);
            await _db.SaveChangesAsync();
            return token.User as TAccount;
        }

        private void MarkUserAuthenticated(TAccount user, string location)
        {
            user.WhenLastAuthenticated = user.WhenAuthenticated;
            user.WhereLastAuthenticated = user.WhereAuthenticated;
            user.WhenAuthenticated = DateTime.UtcNow;
            user.WhereAuthenticated = location;
            user.AuthenticationFailures = 0;
        }

        protected virtual void OnUserAuthenticated(TAccount user)
        {
        }

        public async Task<TAccount> AuthenticateWithCodeAsync(Credentials creds)
        {
            if (! await ValidateAccountCodeAsync(creds.Username, creds.Code))
                throw new AuthenticationFailureException();

            string hash = creds.Username.ToHash();
            AccountToken token = await _db.AccountTokens
                .Include(o=>o.User)
                .Where(o => o.Hash == hash)
                .SingleOrDefaultAsync();

            if (token == null)
                throw new AuthenticationFailureException();

            return token.User as TAccount;
        }

        public async Task<TAccount> AuthenticateWithResetAsync(Credentials creds)
        {
            if (!IsPasswordComplex(creds.Password))
                throw new PasswordComplexityException();

            TAccount user = await AuthenticateWithCodeAsync(creds);
            await _db.Entry(user).Collection(u => u.Tokens).LoadAsync();
            await UpdatePasswordAsync(user, creds.Password);
            return user;
        }

        #endregion

        #region Token Generation

        public async Task<int> GenerateAccountCodeAsync(string account)
        {
            string hash = account.ToHash();
            int code = _rand.Next(100000, 1000000);
            AccountCode ac = await _db.AccountCodes.FindAsync(hash);
            if (ac == null)
            {
                ac = new AccountCode { Hash = account.ToHash() };
                _db.AccountCodes.Add(ac);
            }
            ac.Code = code;
            ac.WhenCreated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return code;
        }

        public async Task<bool> ValidateAccountCodeAsync(string account, int code)
        {
            bool result = false;
            AccountCode token = await _db.AccountCodes.FindAsync(account.ToHash());
            if (token != null)
            {
                result = token.Code == code && !IsExpired(token.WhenCreated);
                // todo: maybe implement multiple validation attempts
                // if (result)
                // {
                    _db.AccountCodes.Remove(token);
                    await _db.SaveChangesAsync();
                // }
            }
            return result;
        }

        public async Task<string> GenerateAuthenticationTokenAsync(int userId)
        {
            //TODO: return a SAML token or something that can be passed to a Relying Party
            await Task.Delay(0);
            return "";
        }

        public virtual object GenerateJwtToken(TAccount user)
        {
            return _tokenService.GenerateJwt(user.GlobalId);
        }
        #endregion

        #region Account Modification

        public async Task UpdatePasswordAsync(int userId, string password)
        {
            TAccount user = (TAccount) await _db.Accounts
                .Include(o=>o.Tokens)
                .Where(o => o.Id == userId)
                .SingleOrDefaultAsync();
            await UpdatePasswordAsync(user, password);
        }

        private async Task UpdatePasswordAsync(TAccount user, string password)
        {
            if (user == null)
                throw new InvalidOperationException();

            if (!IsPasswordComplex(password))
                throw new PasswordComplexityException();

            string passwordHash = user.GeneratePasswordHash(password);

            int history = _options.Password.History;
            AccountToken[] existingPasswords = user.Tokens
                .Where(o => o.Type == AccountTokenType.Password)
                .OrderByDescending(o => o.WhenCreated)
                .ToArray();

            for (int i = 0; i < existingPasswords.Length; i++)
            {
                if (i < history)
                {
                    if (existingPasswords[i].Hash == passwordHash)
                        throw new PasswordHistoryException();
                }
                else
                {
                    user.Tokens.Remove(existingPasswords[i]);
                }
            }
            await _db.SaveChangesAsync();
            Unlock(user);

            user.Tokens.Add(new AccountToken
            {
                Hash = passwordHash,
                WhenCreated = DateTime.UtcNow,
                Type = AccountTokenType.Password
            });

            await _db.SaveChangesAsync();
        }

        public async Task AddAccountAsync(int userId, Credentials creds)
        {
            if (! await IsTokenUniqueAsync(creds.Username))
                throw new AccountNotUniqueException();

            if (_options.Registration.RequireConfirmation
                && ! await ValidateAccountCodeAsync(creds.Username, creds.Code))
                throw new AccountNotConfirmedException();

            TAccount user = (TAccount) await _db.Accounts
                .Include(o => o.Tokens)
                .Where(o => o.Id == userId)
                .SingleOrDefaultAsync();

            if (user == null)
                throw new InvalidOperationException();

            user.Tokens.Add(new AccountToken
            {
                Hash = creds.Username.ToHash(),
                WhenCreated = DateTime.UtcNow,
                Type = AccountTokenType.Credential
            });

            await _db.SaveChangesAsync();
        }

        public async Task RemoveAccountAsync(int userId, string account)
        {
            TAccount user = (TAccount) await _db.Accounts
                .Include(o => o.Tokens)
                .Where(o => o.Id == userId)
                .SingleOrDefaultAsync();

            if (user == null)
                throw new InvalidOperationException();

            if (user.Tokens.Where(t => t.Type != AccountTokenType.Password).Count() < 2)
                throw new InvalidOperationException();

            AccountToken token = user.Tokens.Where(t => t.Hash == account.ToHash()).Single();
            user.Tokens.Remove(token);
            await _db.SaveChangesAsync();
        }

        public async Task<TAccount> SetStatus(int userId, AccountStatus status)
        {
            TAccount user = await _db.Accounts.FindAsync(userId) as TAccount;
            user.Status = status;
            await _db.SaveChangesAsync();
            return user;
        }

        private void Lock(TAccount user)
        {
            user.LockedMinutes = (user.LockedMinutes > 0) ? user.LockedMinutes * 2 : 1;
            user.WhenLocked = DateTime.UtcNow;
        }

        private void Unlock(TAccount user)
        {
            user.Status = AccountStatus.Active;
            user.LockedMinutes = 0;
        }

        #endregion

        // #region Manage Account

        // public async Task<bool> AddCredentialsToAccount(int userId, string username, string confirmUsername, string password, string confirmPassword)
        // {
        //     // we are only allowing a user to have one username/email associated with an account
        //     Account account = await _db.Accounts.FindAsync(userId);

        //     if (account == null)
        //     {
        //         throw new AccountNotFoundException();
        //     }

        //     if (account.Status != AccountStatus.Active)
        //     {
        //         throw new AccountDisabledException();
        //     }

        //     if (!IsEmailValid(username))
        //     {
        //         throw new EmailInvalidException();
        //     }

        //     if (!IsTokenUniqueAsync(username).Result)
        //     {
        //         throw new AccountNotUniqueException();
        //     }

        //     if (!IsDomainValid(username))
        //     {
        //         throw new RegistrationDomainException();
        //     }

        //     if (!username.Equals(confirmUsername))
        //     {
        //         throw new UsernamesDoNotMatchException();
        //     }

        //     if (!password.Equals(confirmPassword))
        //     {
        //         throw new PasswordsDoNotMatchException();
        //     }

        //     if (account.Tokens.Where(t => t.Type == AccountTokenType.Email).Any())
        //     {
        //         throw new InvalidRegistrationException();
        //     }

        //     account.Tokens.Add(new AccountToken
        //     {
        //         Hash = username.ToHash(),
        //         WhenCreated = DateTime.UtcNow,
        //         Type = AccountTokenType.Email
        //     });

        //     await _db.SaveChangesAsync();

        //     try
        //     {
        //         await UpdatePasswordAsync(userId, password);
        //     }
        //     catch(Exception exc)
        //     {
        //         // the password save failed, try to remove the account.
        //         // TODO: a better way would be to refactor this method to use a transcation to add the username and password
        //         if(account != null)
        //         {
        //             AccountToken accountToken = account.Tokens.Where(t => t.Type == AccountTokenType.Email && t.Hash == username.ToHash()).SingleOrDefault();
        //             account.Tokens.Remove(accountToken);
        //             await _db.SaveChangesAsync();
        //         }

        //         throw exc;
        //     }

        //     return true;
        // }

        // public async Task<bool> UpdateUsername(int userId, string existingUsername, string newUsername, string confirmNewUsername)
        // {
        //     Account account = await _db.Accounts.FindAsync(userId);

        //     if (account == null)
        //     {
        //         throw new AccountNotFoundException();
        //     }

        //     if (account.Status != AccountStatus.Active)
        //     {
        //         throw new AccountDisabledException();
        //     }

        //     if (!IsEmailValid(newUsername))
        //     {
        //         throw new EmailInvalidException();
        //     }

        //     if (!newUsername.Equals(confirmNewUsername))
        //     {
        //         throw new UsernamesDoNotMatchException();
        //     }

        //     if (!IsTokenUniqueAsync(newUsername).Result)
        //     {
        //         throw new AccountNotUniqueException();
        //     }

        //     if (!IsDomainValid(newUsername))
        //     {
        //         throw new RegistrationDomainException();
        //     }

        //     AccountToken accountToken = account.Tokens.Where(t => t.Type == AccountTokenType.Email && t.Hash == existingUsername.ToHash()).SingleOrDefault();

        //     if (accountToken != null)
        //     {
        //         // the Hash property if part of the key and EF doesn't support editing such a property at this time.
        //         // we are going to delete the existing key and add a new one

        //         using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
        //         {
        //             try
        //             {
        //                 _db.AccountTokens.Remove(accountToken);

        //                 await _db.AccountTokens.AddAsync(new AccountToken
        //                 {
        //                     Hash = newUsername.ToHash(),
        //                     Type = AccountTokenType.Email,
        //                     UserId = userId,
        //                     WhenCreated = DateTime.UtcNow
        //                 });

        //                 await _db.SaveChangesAsync();
        //                 transaction.Commit();

        //                 return true;
        //             }
        //             catch
        //             {
        //                 transaction.Rollback();
        //                 throw;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         throw new AccountNotFoundException();
        //     }
        // }

        // public async Task<bool> DeleteUsername(int userId, string existingUsername)
        // {
        //     Account account = await _db.Accounts.FindAsync(userId);

        //     if (account == null)
        //     {
        //         throw new AccountNotFoundException();
        //     }

        //     if (account.Status != AccountStatus.Active)
        //     {
        //         throw new AccountDisabledException();
        //     }

        //     // you must have a client certificate before we can delete your email address or there will be no way for the user to authenticate
        //     if (!account.Tokens.Where(t => t.Type == AccountTokenType.Certificate).Any())
        //     {
        //         throw new InvalidOperationException();
        //     }

        //     AccountToken accountToken = account.Tokens.Where(t => t.Type == AccountTokenType.Email && t.Hash == existingUsername.ToHash()).SingleOrDefault();

        //     if (accountToken != null)
        //     {
        //         _db.AccountTokens.Remove(accountToken);
        //         await _db.SaveChangesAsync();

        //         return true;
        //     }
        //     else
        //     {
        //         throw new AccountNotFoundException();
        //     }
        // }

        // #endregion

        #region Query

        public async Task<TAccount> FindAsync(int id)
        {
            return await _db.Accounts.FindAsync(id) as TAccount;
        }

        public async Task<TAccount> FindByAccountAsync(string account)
        {
            return await _db.AccountTokens
                    .Include(o => o.User)
                    .Where(o => o.Hash == account.ToHash())
                    .Select(o => o.User)
                    .SingleOrDefaultAsync() as TAccount;
        }

        public async Task<TAccount> FindByGuidAsync(string guid)
        {
            return (TAccount) await _db.Accounts.Include(u => u.Tokens).Where(u => u.GlobalId == guid).SingleOrDefaultAsync();
        }

        public bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            System.ComponentModel.DataAnnotations.EmailAddressAttribute attr = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            return attr.IsValid(email);
        }

        public bool IsPasswordComplex(string password)
        {
            return !_options.Password.ComplexityExpression.HasValue()
                || Regex.IsMatch(password, _options.Password.ComplexityExpression);
        }

        public bool IsDomainValid(string account)
        {
            return !_options.Registration.AllowedDomains.HasValue()
                || Regex.IsMatch(account, _options.Registration.AllowedDomains, RegexOptions.IgnoreCase);
        }

        public bool IsExpired(DateTime dt)
        {
            return (_options.Password.ResetTokenExpirationMinutes > 0
                && DateTime.UtcNow.Subtract(dt).TotalMinutes > _options.Password.ResetTokenExpirationMinutes);
        }

        public async Task<bool> IsTokenUniqueAsync(string token)
        {
            return await _db.AccountTokens.FindAsync(token.ToHash()) == null;
        }

        // private AccountTokenType GuessTokenType(string token)
        // {
        //     //todo: email
        //     if (Regex.IsMatch(token, @".+@.+\..+"))
        //         return AccountTokenType.Email;

        //     //todo: phone
        //     return AccountTokenType.Email;
        // }

        #endregion
    }
}