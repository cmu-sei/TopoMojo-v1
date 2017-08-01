using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Step.Accounts
{
    public class DefaultProfileService : IProfileService
    {
        public async Task<Claim[]> GetClaimsAsync(string globalId)
        {
            await Task.Delay(0);
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, globalId));
            return claims.ToArray();
        }

        public async Task<object> GetProfileAsync(string globalId)
        {
            await Task.Delay(0);
            return new { Name = globalId };
        }

        public async Task AddProfileAsync(string globalId, string name)
        {
            await Task.Delay(0);
        }
    }
}