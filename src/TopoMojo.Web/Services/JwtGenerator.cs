using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public static class JwtTokenGeneratorObsolete
    {
        public static object Generate(dynamic id, IdentityOptions options)
        {
            DateTime now = DateTime.UtcNow;
            long nowSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            // ClaimsIdentity identity = (ClaimsIdentity)principal.Identity;
            // string sub = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            // string name = identity.FindFirst(ClaimTypes.Name).Value;

            Claim[] claims = new Claim[]
            {
                //new Claim(JwtRegisteredClaimNames.Sub, id.sub),
                new Claim("sub", id.sub.ToString()),
                new Claim("tmnm", id.tmnm.ToString()),
                new Claim("tmid", id.tmid.ToString()),
                new Claim("tmad", id.tmad.ToString()),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, nowSeconds.ToString(), ClaimValueTypes.Integer64)
            };

            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(
                    options.Authentication.TokenKey));

            SigningCredentials signer = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: options.Authentication.TokenIssuer,
                audience: options.Authentication.TokenAudience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(options.Authentication.TokenExpirationMinutes),
                signingCredentials: signer);

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new
            {
                token_type = "Bearer",
                access_token = encodedJwt,
                expires_in = options.Authentication.TokenExpirationMinutes * 60
            };
        }

    }

}