// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo
{
    public class ControlOptions
    {
        public string ApplicationName { get; set; }
        public bool ShowExceptionDetail { get; set; }
        public int ProfileCacheSeconds { get; set; } = 300;
        public string DemoCode { get; set; }
    }
}
