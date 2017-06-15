using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Step.Accounts
{
    public interface IProfileService
    {
        Task<Claim[]> GetClaimsAsync(string globalId);
        Task<object> GetProfileAsync(string globalId);
        Task AddProfileAsync(string globalId, string name);
    }

}