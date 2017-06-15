using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Step.Accounts;
using TopoMojo.Core;

namespace TopoMojo.Services
{
    public class ProfileService : IProfileService
    {
        public ProfileService
        (
            ProfileManager profileManager,
            ILogger<ProfileService> logger
        )
        {
            _profileManager = profileManager;
            _logger = logger;
        }

        protected readonly ProfileManager _profileManager;
        protected readonly ILogger _logger;

        public async Task<Claim[]> GetClaimsAsync(string globalId)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, globalId));

            Person person = await _profileManager.LoadByGlobalId(globalId);
            if (person != null)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.NameId, person.Id.ToString()));
                claims.Add(new Claim("name", person.Name));
                if (person.IsAdmin)
                    claims.Add(new Claim("role", "admin"));
            }

            return claims.ToArray();
        }

        public async Task<object> GetProfileAsync(string globalId)
        {
            Person person = await _profileManager.LoadByGlobalId(globalId);
            return new { Name = person.Name };
        }

        public async Task AddProfileAsync(string globalId, string name)
        {
            await _profileManager.SaveAsync(new Person() {
                GlobalId = globalId,
                Name = name
            });
        }
    }
}