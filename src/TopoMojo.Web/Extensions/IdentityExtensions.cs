using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TopoMojo.Models;

namespace TopoMojo.Extensions
{
    public static class IdentityExtensions
    {
        public static ClaimsPrincipal AddUserClaims(this ClaimsPrincipal principal, User user)
        {
            ((ClaimsIdentity)principal.Identity).AddClaims(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("role", user.Role)
            });

            return principal;
        }
    }
}
