// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo
{
    public class AuthorizationOptions
    {
        public string Authority { get; set; }
        public string AuthorizationUrl { get; set; }
        public string AuthorizationScope { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public OpenIdClient SwaggerClient { get; set; }
    }

    public class OpenIdClient
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }

    }
}
