// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo
{
    public static class ClaimsPrincipalExtensions
    {
        public static Models.User ToModel(this ClaimsPrincipal principal)
        {
            return new Models.User
            {
                Id = principal.LocalId(),
                GlobalId = principal.Subject(),
                Name = principal.Name(),
                Role = Enum.Parse<UserRole>(
                    string.Join(',',
                    principal.FindAll(AppConstants.RoleClaimName)
                        .Select(c => c.Value)
                        .ToArray()
                    )
                ),
                Client = new Client
                {
                    Id = principal.ClientId(),
                    Scope = principal.ClientScope(),
                    Url = principal.ClientUrl()
                }
            };
        }
        public static string Subject(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.SubjectClaimName);
        }

        public static string Name(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.NameClaimName);
        }

        public static string ClientId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.ClientIdClaimName);
        }

        public static string ClientScope(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.ClientScopeClaimName);
        }

        public static string ClientUrl(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.ClientUrlClaimName);
        }

        public static int LocalId(this ClaimsPrincipal user)
        {
            int id = 0;
            int.TryParse(
                user.FindFirstValue(AppConstants.NameIdClaimName),
                out id
            );
            return id;
        }

        public static ClaimsPrincipal AddUserClaims(this ClaimsPrincipal principal, User user)
        {
            ((ClaimsIdentity)principal.Identity).AddClaims(new Claim[]
            {
                new Claim(AppConstants.NameIdClaimName, user.Id.ToString()),
                new Claim(AppConstants.NameClaimName, user.Name),
                new Claim(AppConstants.RoleClaimName, user.Role.ToString().ToLower())
            });

            return principal;
        }

    }
}
