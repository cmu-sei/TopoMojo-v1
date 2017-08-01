using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Text;
using Step.Accounts;

namespace Tests
{
    public class AccountManagerTests
    {

        public AccountManagerTests()
        {
            _options = new AccountOptions();
            _options.Password.History = 3;
            _options.Password.ComplexityText = "At least 10 characters containing uppercase and lowercase letters, numbers, and symbols";
            _options.Password.ComplexityExpression = @"(?=^.{10,}$)(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[`~!@#$%^&*\(\)\-_=+\[\]\{\}\\|;:'"",<\.>/?\ ]).*$";
            _options.Registration.AllowedDomains = @"^.+@.*jam\.net|test\.com$";
            _options.Authentication.LockThreshold = 5;
            //_options = Options.Create(_options);

             _dbOptions = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Register")
                .Options;

            _mill = new TestLoggerFactory();
            _logger = _mill.CreateLogger<AccountManagerTests>();

            _profileSvc = new DefaultProfileService();
            _tokenSvc = new DefaultTokenService(_options.Token, _profileSvc);
            Environment.SetEnvironmentVariable("COMPlus_ReadyToRunExcludeList", "System.Security.Cryptography.X509Certificates");
        }

        AccountOptions _options = null;
        //IOptionsSnapshot<AccountOptions> _options = null;
        DbContextOptions<AccountDbContext> _dbOptions = null;
        ILoggerFactory _mill = null;
        ILogger<AccountManagerTests> _logger = null;
        X509IssuerStore _certStore = null;
        ITokenService _tokenSvc = null;
        IProfileService _profileSvc = null;

        [Theory]
        [InlineData("Jeff Matts0n")]
        [InlineData("Tartans@@1")]
        public void Good_Passwords(string password)
        {
                IAccountManager<Account> mgr = CreateAccountManager(null);
                Assert.True(mgr.IsPasswordComplex(password));
        }

        [Theory]
        [InlineData("tartans@1")]
        [InlineData("tartans@11")]
        public void Bad_Passwords(string password)
        {
            IAccountManager<Account> mgr = CreateAccountManager(null);
            Assert.False(mgr.IsPasswordComplex(password));
        }

        [Fact]
        public void Certificate_Subject_regex_works()
        {
            string subject = "C=US, O=U.S. Government, OU=DoD, OU=PKI, OU=CONTRACTOR, CN=LAST.FIRST.MIDDLE.0123456789";
            Match idMatch = Regex.Match(subject, @"CN=\S*(\d{10})");
            Match nameMatch = Regex.Match(subject, @"CN=(\S*)\.\d");
            string id = idMatch.Groups[idMatch.Groups.Count-1].Value;
            string name = nameMatch.Groups[idMatch.Groups.Count-1].Value;
            Assert.True(id == "0123456789");
            Assert.True(name == "LAST.FIRST.MIDDLE");
        }

        [Fact]  //This crashes on Windows due to R2R bug.  "set COMPlus_ReadyToRunExcludeList=System.Security.Cryptography.X509Certificates" to make it work, pending dotnet 1.2
        public void Certificate_is_valid()
        {

            X509IssuerStore store = new X509IssuerStore(_options, new X509IssuerStoreOptions { RootPath = ""}, _mill);
            store.Add(LoadCertificate(rootCertificate));
            store.Add(LoadCertificate(intCertificate));
            X509Certificate2 certificate = LoadCertificate(userCertificate);
            Assert.True(store.Validate(certificate));
        }

        [Fact]
        public void RegisterCredentials_adds_user()
        {
            Account userAdded = null;
            Account userFound = null;
            Credentials creds = new Credentials
            {
                Username = "test-register-creds@test.com",
                Password = "~Tartans@1~"
            };

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                ctx.Database.EnsureCreated();
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                userAdded = mgr.RegisterWithCredentialsAsync(creds).Result;
            }

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                userFound = ctx.Accounts.Include(o=>o.Tokens).LastOrDefault();
            }

            Assert.True(userAdded != null && userFound != null
                && userFound.Id == userAdded.Id
                && userFound.VerifyPasswordHash(creds.Password));
        }

        [Fact]
        public void AuthenicateWithCredential_works()
        {
            Account userAdded = null;
            Account userFound = null;
            Credentials creds = new Credentials
            {
                Username = "test-auth-creds@test.com",
                Password = "~Tartans@1~"
            };
            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                ctx.Database.EnsureCreated();
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                userAdded = mgr.RegisterWithCredentialsAsync(creds).Result;
            }

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                userFound = mgr.AuthenticateWithCredentialAsync(creds, "127.0.0.1").Result;
            }

            Assert.True(userAdded != null && userFound != null
                && userFound.Id == userAdded.Id);

        }

        [Fact]
        public void Reset_yields_expired_account_with_no_lockouts()
        {
            Account userAdded = null;
            Account userFound = null;
            Credentials creds = new Credentials
            {
                Username = "test-reset@test.com",
                Password = "~Tartans@1~"
            };

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                ctx.Database.EnsureCreated();
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                userAdded = mgr.RegisterWithCredentialsAsync(creds).Result;
                creds.Code = mgr.GenerateAccountCodeAsync(creds.Username).Result;
                creds.Password = PassGen(creds.Code);
            }

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                mgr.AuthenticateWithResetAsync(creds).Wait();
                userFound = mgr.FindAsync(userAdded.Id).Result;
            }

            Assert.True(userAdded != null && userFound != null
                && userFound.Id == userAdded.Id
                && userFound.Status == AccountStatus.Active
                && userFound.AuthenticationFailures == 0);
        }

        // [Fact]
        // public void Entity_Principal_conversion_works()
        // {
        //     Account userAdded = null;
        //     Account userFound = null;
        //     string account = "test-principal@test.com";
        //     string password = "~Tartans@1~";

        //     using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
        //     {
        //         ctx.Database.EnsureCreated();
        //         IAccountManager<Account> mgr = new AccountManager<Account>(ctx, _options, _mill, _certStore, _profileSvc);
        //         userAdded = mgr.RegisterWithCredentialsAsync(account, password).Result;
        //         ClaimsPrincipal principal = mgr.ConvertToPrincipal(userAdded);
        //         userFound = mgr.FindAsync(principal).Result;
        //     }

        //     // using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
        //     // {
        //     //     IAccountManager<Account> mgr = new AccountManager<Account>(ctx, _options);
        //     //     userFound = mgr.AuthenticateWithResetToken(resetToken, "127.0.0.1");
        //     // }

        //     Assert.True(userAdded != null && userFound != null
        //         && userFound.Id == userAdded.Id);
        // }

        [Fact]
        public void AuthFailures_yields_locked_account()
        {
            Account userAdded = null;
            Account userFound = null;
            Credentials creds = new Credentials
            {
                Username = "test-lock@test.com",
                Password = "~Tartans@1~"
            };

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                ctx.Database.EnsureCreated();
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                userAdded = mgr.RegisterWithCredentialsAsync(creds).Result;

                for (int i=0; i < 5; i++)
                {
                    try
                    {
                        creds.Password += i.ToString();
                        mgr.AuthenticateWithCredentialAsync(creds, "").Wait();
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }

            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                userFound = ctx.Accounts.Find(userAdded.Id);
            }

            Assert.True(userAdded != null && userFound != null
                && userFound.Id == userAdded.Id
                && userFound.LockedMinutes > 0);
        }

        [Fact]
        public void TestPassGen()
        {
            using (AccountDbContext ctx = new AccountDbContext(_dbOptions))
            {
                IAccountManager<Account> mgr = CreateAccountManager(ctx);
                string pass = "";
                for (int i = 0; i < 10; i++)
                {
                    pass = PassGen(i);
                    Console.WriteLine($"password: {pass}");
                }
                Assert.True(mgr.IsPasswordComplex(pass));
            }
        }

        private IAccountManager<Account> CreateAccountManager(AccountDbContext ctx)
        {
            IAccountManager<Account> mgr = new AccountManager(ctx, _options, _mill, _certStore, _tokenSvc, _profileSvc);
            return mgr;
        }

        private string PassGen(int seed)
        {
            Random rand = new Random(seed);
            byte[][] pool = new byte[4][];
            pool[0] = Encoding.ASCII.GetBytes("!@#$%^&*()_-+=[{]};:<>|./?");
            pool[1] = Encoding.ASCII.GetBytes("01234567890123456789012345");
            pool[2] = Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwxyz");
            pool[3] = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            byte[] passbytes = new byte[20];

            int i = 0;
            while (i < passbytes.Length)
            {
                passbytes[i++] = pool[rand.Next(4)][rand.Next(26)];
            }
            return Encoding.ASCII.GetString(passbytes);
        }

        string userCertificate = @"
        -----BEGIN CERTIFICATE-----
        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
        -----END CERTIFICATE-----
        ";

        string intCertificate = @"
        -----BEGIN CERTIFICATE-----
        MIIF5TCCA82gAwIBAgIFEAAAAAAwDQYJKoZIhvcNAQELBQAwgYYxCzAJBgNVBAYT
        AlVTMRUwEwYDVQQIDAxQZW5uc3lsdmFuaWExEzARBgNVBAcMClBpdHRzYnVyZ2gx
        KTAnBgNVBAoMIENFUlQgQ3liZXIgV29ya2ZvcmNlIERldmVsb3BtZW50MSAwHgYD
        VQQDDBdDV0QgRGV2ZWxvcG1lbnQgUm9vdCBDQTAeFw0xNzAxMjcwNDM0MzhaFw0y
        NzAxMjUwNDM0MzhaMHkxCzAJBgNVBAYTAlVTMRUwEwYDVQQIDAxQZW5uc3lsdmFu
        aWExKTAnBgNVBAoMIENFUlQgQ3liZXIgV29ya2ZvcmNlIERldmVsb3BtZW50MSgw
        JgYDVQQDDB9DV0QgRGV2ZWxvcG1lbnQgSW50ZXJtZWRpYXRlIENBMIICIjANBgkq
        hkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA0YjvDgccrk9cG7VfEz7ij9llfAbr8IrQ
        F+ZJXa5569ro60IPB1NqafLPcIewIy+9NKSonh2GJFMBUXlYX0+520I/tH3+PdVl
        jmxovukq/7BepzIxReqtHcrPWVMTQZWX3TEpci7g66A8C9MDQj2TwQ5phJhmfNck
        R+oZY1DS3SBQm9dN1DGKnBVmzAo7j0PvDssS143Vw1ePT8Zdi+3ri+LCLpATeg/V
        bo3156ZCjY0yacQGxpQTUHbVCssubiDixM7dIPhNtqFc3P6R+nhhVkWkuTywIBV6
        4ZCBc7hIQaYcrcRSkIr+JxIGit9Oar2paM0ZkOuD/NWYHRHjrwTNLGFBE0Ecr7F2
        oC4Zn26J82xxgcAwBmAY36is1uENiLtRFxx+TsxUF5M4FJnqmO0ITaCJf97W2Ww0
        DFRNW5YwIZBDp06ivxrQVUB5DdAappZmaYx3rPe3PYnQK4pobKhRF4jhAGB8CYDG
        xBAXhxO+5WdtVXu3ACSNoYJK20YZBvsIfyRA75qxPhNy5P9mu2/46DH564RIlYBZ
        rOmz/d1k9Zg8SXWJjxnpYJQ4vHobL7hhCLojjrYACweAYuhAwrtR6zY+7HkZfIJ7
        2jsG6L4YtiS0bkquefJ8Eo8KEIJWYMmjmSWM2RZXnfwkZELqTz+80sWNU1PyqeI4
        2IqZPE+v8F8CAwEAAaNmMGQwHQYDVR0OBBYEFKuY8NHgMxYGiNr2X0wuuMiq6wvx
        MB8GA1UdIwQYMBaAFFRd3qos+MaIt8TbJZZREdB9hJGUMBIGA1UdEwEB/wQIMAYB
        Af8CAQAwDgYDVR0PAQH/BAQDAgGGMA0GCSqGSIb3DQEBCwUAA4ICAQBu+MCG3c9y
        gAOxZNN1gz4+mzz0nYbZNaNCpYNnbhSQ7+QO7LsEQE6xEfip4bJUFMdHjA5p9crC
        GMvnV9W/qqdQTEfE1x8TcGXLplSKzFriexGH0GnuJAbmZFyyH77gT2GmEWv1WnSW
        M6igwunQOzrf5SI5f54OaZEGJPiWfEZzkZoECIy/tf7LxG+Gc9AQvJPj23pZ+zl9
        v4d+lLDFn0a9rbIe2hHR2Nyw80SQ6cMywkHk2Yx5BMe82sGrOWvaDxgE4RmHhgBM
        gbYt0/OQ8UYxSU3drsMznBL3UmIIZEZ5wBBbtTAegCCh9IydgJ6H7cWkUNjH77m+
        qH57vmP7F88oFhI38xAUIB/MHJ/UtOqT0GKBHAS5LQaGIwXeaoWyMzXh0IthlOBe
        qaoI4zYY5nr7RpHKEG5dBAtZcsqOISlnnfTDLiryWILq98fxqlU75AhWpk5Uln80
        vq9cndSLjBRG0i9T6bFQmDUywY2qdEWMAX1g+eHnXaDEIKdsz3V7a55c9AakIpcW
        dQ/kV/9FFcMF0EAQtE8CgPMO5XDW1Nb8gJacXx3vnWitKPJVbnRgzApCjIyNaJT3
        PU+jZDgamK95Fak4EroiNDqe2emYq1MUK4UEljd+r3abqlVq74MfPATAWpaFypoj
        ZCDnFQ9pPFS0NsrSdjegToVWfdIgeZbXGQ==
        -----END CERTIFICATE-----
        ";

        string rootCertificate = @"
        -----BEGIN CERTIFICATE-----
        MIIF9DCCA9ygAwIBAgIJAJmMJFPZfkxLMA0GCSqGSIb3DQEBCwUAMIGGMQswCQYD
        VQQGEwJVUzEVMBMGA1UECAwMUGVubnN5bHZhbmlhMRMwEQYDVQQHDApQaXR0c2J1
        cmdoMSkwJwYDVQQKDCBDRVJUIEN5YmVyIFdvcmtmb3JjZSBEZXZlbG9wbWVudDEg
        MB4GA1UEAwwXQ1dEIERldmVsb3BtZW50IFJvb3QgQ0EwHhcNMTcwMTI3MDQyMjQ5
        WhcNMzcwMTIyMDQyMjQ5WjCBhjELMAkGA1UEBhMCVVMxFTATBgNVBAgMDFBlbm5z
        eWx2YW5pYTETMBEGA1UEBwwKUGl0dHNidXJnaDEpMCcGA1UECgwgQ0VSVCBDeWJl
        ciBXb3JrZm9yY2UgRGV2ZWxvcG1lbnQxIDAeBgNVBAMMF0NXRCBEZXZlbG9wbWVu
        dCBSb290IENBMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAtfxMhg87
        pTDvLC1FoQqW9X+qhuYOkMmdobYWNnv/7m+78VdutP92DGqyBr5d04oDT59dwCjG
        nWmCAkHdjGRwUIdDAJ3ylcKJavd7bKQMz1OrincWQU1c27i0viBZWWeFD7qyXNcT
        dIwapI2G8ISB6VJ6SNS+RAfe6rNlIcMMKzOkMKx1Cf5zR2vwTgFSCZa46gvbrwdn
        xD3bDBWrhSSRfc28qChFI8xCwV784tFYT1rG6X09iOboDIXDmQnIOILuP+RvhBCp
        FhaL5yRTgh8b6LoqW0DTSmpciZE8g3tNdcXRTnWbG7/qYJ8LaWycIFqnkjGzHyRl
        YYxLOeOWqhqK/3bN1wIkbrSNykUDc/z+6vCMpDUqSoUJSVKdVe6HKl6wKuI8y7Gj
        HWhqm1kwkUC4XyD1Bkk0RtOflaPtKLwcAv7Mkke6Ad5u3UOsqDPLE/M+kdufrxbJ
        +4mS+LG9lk19RrS+uS18xMkyl0iNvehgPTw5ehVdYmM2fpuA47wRgT7b4LrmxCdo
        KsOYZf8yLGrpSjwsmHhW8V1L/Uk+bzyGTR/laoAlJQDXM3Uart/MnfInubTnKfVs
        PnSnKK/kAoHVi0qpXJwhlVqDmWTDupYStbZk3EDuiOXABH7m2B1YSnCrXLzAi+zd
        CLx2x6F8lR2UuJuiSBRl4v6AyBT+7Ndc2n0CAwEAAaNjMGEwHQYDVR0OBBYEFFRd
        3qos+MaIt8TbJZZREdB9hJGUMB8GA1UdIwQYMBaAFFRd3qos+MaIt8TbJZZREdB9
        hJGUMA8GA1UdEwEB/wQFMAMBAf8wDgYDVR0PAQH/BAQDAgGGMA0GCSqGSIb3DQEB
        CwUAA4ICAQAXMo1BrV8DC22pEDDNU+83O89ZkpN33f4qnu+zW66qMdPaM6jhv1Oa
        m3+XLkb1e4YqyBgmmWYUb+XiwCi5L+eXhFObeHkTK2fUPePuFpqsJrmI9CFx9uKt
        UG4Fte771N+n9BdZ+Ush7lzIFmhE8uG9LNERrzA7fx6uUxnBzA3o6mK3a+ZTVDIW
        pFUC2dlfYgVuVHrw93mBFz5tmlKHuTDN6NxBBZV94UzgXiqkWgSGjwUmw5WyvpkB
        qopE9pFNif5c8g1CSFsYtqz14XbsabIFTm1/OTkIXOWlZkBMOzTqHzuHr9W1cjFr
        XParljTxS4f6BgXhW5X0D3dlQSQ6uZc615kB+ntScZPQn0jt2XjUExzIREEkHhnU
        cVRVW4cEFfMKL7hOQwMmmfmaH5g6+PQJVFExCyEqysbV4ad8ocpVpmg7Hm1fRuDl
        gQWiuWY5zstc9TD/EhtZcmR/tfoIK2va0q2POkP1fRwyrwKGLsE/CCIuUgXlXWJX
        u6Etyc09zK/WJQNw0/OEyuW+ga2Q9ycUt+nRhBqHCtp/kOnduzu0fNPHjrsb1U1f
        fHN21OC/ManIpIid0L2eOH//gys3MCfmnBU6b3PtrHYeJXfsS0Oy6xoEtWK1uDUR
        D5FwhVU+a+Li4f95bkfmdwmKl2LPCJOIQy3S/qiOhjqlJ3Vq/5HV/A==
        -----END CERTIFICATE-----
        ";

        private X509Certificate2 LoadCertificate(string text)
        {
            string[] lines = Regex.Split(text, @"\r\n?|\n")
                .Where(x => !x.Trim().StartsWith("-"))
                .Select(x => x.Trim()).ToArray();
            string encoded = String.Join("", lines).Trim();
            byte[] bytes = Convert.FromBase64String(encoded);
            return new X509Certificate2(bytes);
        }
    }
}
