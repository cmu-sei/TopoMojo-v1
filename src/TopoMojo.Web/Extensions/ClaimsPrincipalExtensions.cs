// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System.Security.Claims;
using TopoMojo.Web;

namespace TopoMojo
{
    public static class ClaimsPrincipalExtensions
    {
        public static string Subject(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.SubjectClaimName);
        }

        public static string Name(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AppConstants.NameClaimName);
        }

    }
}
