// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo
{
    public class FileUploadOptions
    {
        public long MaxFileBytes { get; set; }
        public string IsoRoot { get; set; }
        public string TopoRoot { get; set; }
        public string MiscRoot { get; set; }
    }
}