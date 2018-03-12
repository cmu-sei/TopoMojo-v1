using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Jam.Accounts;
using Microsoft.Extensions.Logging;
using TopoMojo.Core;
using TopoMojo.Core.Models;

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

        public async Task<Claim[]> GetClaimsAsync(string globalId, string name)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, globalId));

            Profile profile = await _profileManager.FindByGlobalId(globalId);
            if (profile != null)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.NameId, profile.Id.ToString()));
                claims.Add(new Claim(JwtClaimTypes.Name, profile.Name));
                if (profile.IsAdmin)
                    claims.Add(new Claim(JwtClaimTypes.Role, "admin"));
            }
            else {
                claims.Add(new Claim(JwtClaimTypes.Name, name));
            }

            return claims.ToArray();
        }

        public async Task<object> GetProfileAsync(string globalId, string name)
        {
            Profile profile = await _profileManager.FindByGlobalId(globalId);
            if (profile != null)
            {
                return new {
                    Id = globalId,
                    Name = profile.Name,
                    IsAdmin = profile.IsAdmin
                };
            }

            return new {Name = name};
        }

        public async Task AddProfileAsync(string globalId, string name)
        {
            await Task.Delay(0);
            // await _profileManager.SaveAsync(new Profile() {
            //     GlobalId = globalId,
            //     Name = name
            // });
        }
    }
}
