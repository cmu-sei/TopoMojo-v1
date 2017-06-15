using System;

namespace Step.Accounts
{
    public class AccountDisabledException : Exception { }
    public class AccountLockedException : Exception { }
    public class AccountNotUniqueException : Exception { }
    public class AccountNotConfirmedException : Exception { }
    public class AuthenticationFailureException : Exception { }
    public class PasswordComplexityException : Exception { }
    public class PasswordHistoryException : Exception { }
    public class PasswordExpiredException : Exception { }
    public class RegistrationDomainException : Exception { }
}
