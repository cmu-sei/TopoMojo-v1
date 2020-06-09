// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Web
{
    public class AuthorizationOptions
    {
        public string Authority { get; set; }
        public string Audience { get; set; } = "topomojo-api";
        public bool RequireHttpsMetadata { get; set; } = true;
        public OAuth2Client SwaggerClient { get; set; }
    }

    public class OpenIdClient
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }

    }

    public class OAuth2Client
    {
        public string AuthorizationUrl { get; set; }
        public string TokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }
    }
}
