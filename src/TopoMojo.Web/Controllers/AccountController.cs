using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Jam.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TopoMojo.Extensions;
using TopoMojo.Services;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    //[Route("api/[controller]/[action]")]
    public class AccountController : _Controller
    {
        private readonly IAccountManager _accountManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        public AccountController(
            IAccountManager accountManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IServiceProvider sp
        ) : base(sp)
        {
            _accountManager = accountManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        // login(u,p), return token
        // register(u, code), return token
        // reset(p, code)
        // confirm(account), return bool
        // [Authorize] refresh(), return token

        [HttpPost("api/account/login")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Login([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting login for {model.Username}");
            AccountSummary account = await _accountManager.AuthenticateWithCredentialAsync(model, "");
            if (account == null)
                throw new AuthenticationFailedException();

            return Json(GetJWT(account.GlobalId, model.Username));
        }

        [HttpPost("api/account/otp")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Otp([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting login for {model.Username}");
            AccountSummary account = await _accountManager.AuthenticateWithCodeAsync(model, "");
            if (account == null)
                throw new AuthenticationFailedException();

            return Json(GetJWT(account.GlobalId, model.Username));
        }

        [HttpPost("api/account/tfa")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Tfa([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting login for {model.Username}");
            AccountSummary account = await _accountManager.AuthenticateWithCredentialAsync(model, "");
            if (account == null)
                throw new AuthenticationFailedException();

            return Json(GetJWT(account.GlobalId, model.Username));
        }

        [HttpPost("api/account/register")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Register([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting registration for {model.Username}");

            AccountSummary account = await _accountManager.RegisterWithCredentialsAsync(model, "");
            if (account == null)
                throw new AuthenticationFailedException();

            return Json(GetJWT(account.GlobalId, model.Username));
        }

        [HttpPost("api/account/reset")]
        [JsonExceptionFilter]
         public async Task<IActionResult> Reset([FromBody] Credentials model)
        {
            if (model == null)
                throw new AuthenticationFailedException();

            _logger.LogDebug($"Attempting reset for {model?.Username}");

            AccountSummary account = model.Password.HasValue()
                ? await _accountManager.AuthenticateWithResetAsync(model, "")
                : await _accountManager.AuthenticateWithCodeAsync(model, "");


            if (account == null)
                throw new AuthenticationFailedException();

            return Json(GetJWT(account.GlobalId, model.Username));
        }

        [HttpPost("api/account/confirm")]
        [JsonExceptionFilter]
        public async Task<bool> Confirm([FromBody] Credentials model)
        {
            int code = await _accountManager.GenerateAccountCodeAsync(model.Username);
            _logger.LogDebug("Confirmation code {0} {1}", model.Username, code);

            //TODO: send code via email or text
            string msg = $"{_options.ApplicationName} Code: {code}";
            Task task = _emailSender.SendEmailAsync(model.Username, msg, msg);

            return true;
        }

        [Authorize]
        [HttpGet("api/account/refresh")]
        public async Task<IActionResult> Refresh()
        {
            string subject = HttpContext.User.FindFirstValue(JwtClaimTypes.Subject);
            string name = HttpContext.User.FindFirstValue(JwtClaimTypes.Name);
            _logger.LogDebug($"Attempting refresh for {subject}");
            AccountSummary account = await _accountManager.FindByGuidAsync(subject);
            if (account == null)
                throw new AuthenticationFailedException();

            return Json(GetJWT(account.GlobalId, name));
        }

        private object GetJWT(string guid, string name)
        {
            return (_accountManager.GenerateJwtToken(guid, name.ExtractBefore("@")));
        }
        // private async Task SignIn(Account account, string username)
        // {
        //     ClaimsIdentity id = new ClaimsIdentity();
        //     id.AddClaim(new Claim(JwtClaimTypes.Subject, account.GlobalId));
        //     id.AddClaim(new Claim(JwtClaimTypes.Name, username));
        //     ClaimsPrincipal user = new ClaimsPrincipal();
        //     user.AddIdentity(id);
        //     await HttpContext.Authentication.SignInAsync("Cookies", user);
        // }
    }
}
