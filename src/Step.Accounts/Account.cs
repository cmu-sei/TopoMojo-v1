using System;
using System.Collections.Generic;

namespace Step.Accounts
{
    public class Account
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public DateTime WhenCreated { get; set; }
        public DateTime WhenAuthenticated { get; set; }
        public string WhereAuthenticated { get; set; }
        public DateTime WhenLastAuthenticated { get; set; }
        public string WhereLastAuthenticated { get; set; }
        public int AuthenticationFailures { get; set; }
        public DateTime WhenLocked { get; set; }
        public int LockedMinutes { get; set; }
        public AccountStatus Status { get; set; }
        public virtual ICollection<AccountToken> Tokens { get; set; } = new List<AccountToken>();
    }

    public enum AccountStatus
    {
        Active,
        Disabled
    }

    public class AccountToken
    {
        public string Hash { get; set; }
        public DateTime WhenCreated { get; set; }
        public AccountTokenType Type { get; set; }
        public int UserId { get; set; }
        public virtual Account User { get; set; }
    }

    public enum AccountTokenType
    {
        Email,
        Phone,
        Credential,
        Certificate,
        Password
    }

    public class AccountCode
    {
        public string Hash { get; set; }
        public int Code { get; set; }
        public DateTime WhenCreated { get; set; }
    }

    public class ClientAccount
    {
        public int Id { get; set; }
        public string UserGlobalId { get; set; }
        public int ClientId { get; set; }
    }
}
