// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

namespace TopoMojo.Web
{
    public class BrandingOptions
    {
        public string ApplicationName { get; set; } = "TopoMojo";
        public string LogoUrl { get; set; }
        public string Title { get; set; } = "TopoMojo";
        public string[] Meta { get; set; } = new string[] {
            @"<meta name=""description"" content=""Serving #title virtual lab exercise and simulation"">",
            @"<meta property=""og:type"" content=""website"">",
            @"<meta property=""og:title"" content=""#app | #title"">",
            @"<meta property=""og:url"" content=""#url"">",
            @"<meta property=""og:image"" content=""#logo"">",
            @"<meta property=""og:description"" content=""Serving #title virtual lab exercise and simulation"">"
        };

        public bool IncludeSwagger { get; set; } = true;
        public string PathBase { get; set; }
    }
}
