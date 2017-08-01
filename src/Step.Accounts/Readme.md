# Step.Accounts

> author: jmattson@sei.cmu.edu

## Overview

This library provides "compliant" account management to support identity management workflows.
Generally that means the AccountManager enforces password complexity, age, lockout and reuse.

Additionally, this manager stores no Personally Identifiable Information.  It does, however, expect the inject of an IProfileService to generate any claims to be included in the authentication token.

It provides registration and authentication for both credentials and certificates.

## Notes

*   Users can have multiple account tokens ( certificate-key, email, phone, etc.)
*   Users can have multiple password tokens to support history requirements.  Only the most recent password is checked when authenticating with credentials.
*   User accounts are assumed 'confirmed', so confirm them before adding them if desired.
    * Confirmation workflow: generate a code for an account, send it out-of-band, validate the account/code; if valid register/auth/add as applicable.
    *   The AccountCodes pattern can be used for password reset, account confirmation, and 2fa.

## Usage

Dependency graph:
* IAccountManager, AccountManager
  * AccountDbContext
  * AccountOptions
  * IX509IssuerStore, X509IssuerStore
    * X509IssuerStoreOptions
  * ITokenService, DefaultTokenService
    * IProfileService, DefaultProfileService

Presumably, the *Default---Service* classes will be implemented by the consuming application, and thus add to the dependency graph.

Consuming in AspNetCore `Startup.cs` might look something like this:
```cs
using Step.Accounts;

// add Account services
services.Configure<AccountOptions>(Configuration.GetSection("Account"))
    .AddDbContext<AccountDbContext>(builder => builder.UseConfiguredDatabase(Configuration))
    .AddScoped(sp => sp.GetService<IOptionsSnapshot<AccountOptions>>().Value)
    .AddScoped<IAccountManager<Account>, AccountManager<Account>>();

// add X509IssuerStore
services.Configure<X509IssuerStoreOptions>(options => { options.RootPath = _rootPath; })
    .AddScoped(sp => sp.GetService<IOptions<X509IssuerStoreOptions>>().Value)
    .AddSingleton<IX509IssuerStore, X509IssuerStore>();

// add TokenService
services.Configure<TokenOptions>(Configuration.GetSection("Account:Token"))
    .AddScoped(sp => sp.GetService<IOptions<TokenOptions>>().Value)
    .AddScoped<ITokenService, DefaultTokenService>();

// add ProfileService
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
    .AddScoped<IProfileService, DefaultProfileService>();

```