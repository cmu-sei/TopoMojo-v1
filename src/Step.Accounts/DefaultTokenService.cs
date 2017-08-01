using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Step.Accounts
{

    public class DefaultTokenService : ITokenService
    {
        public DefaultTokenService (
            TokenOptions accountOptions,
            IProfileService profileService
        ){
            _options = accountOptions;
            _profileService = profileService;
        }
        protected readonly TokenOptions _options;
        protected readonly IProfileService _profileService;

        public object GenerateJwt(string globalId)
        {
            DateTime now = DateTime.UtcNow;
            long nowSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(
                    _options.Key));

            SigningCredentials signer = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: _profileService.GetClaimsAsync(globalId).Result,
                //notBefore: now,
                expires: now.AddMinutes(_options.ExpirationMinutes),
                signingCredentials: signer);

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new
            {
                token_type = "Bearer",
                access_token = encodedJwt,
                expires_in = _options.ExpirationMinutes * 60,
                profile = _profileService.GetProfileAsync(globalId).Result
            };
        }
    }
}