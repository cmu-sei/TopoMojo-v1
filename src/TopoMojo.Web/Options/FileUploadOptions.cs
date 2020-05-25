// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Web
{
    public class FileUploadOptions
    {
        public long MaxFileBytes { get; set; }
        public string IsoRoot { get; set; } = "tm";
        public string TopoRoot { get; set; } = "tm";
    }
}
