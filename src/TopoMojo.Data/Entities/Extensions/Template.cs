// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Entities.Extensions
{
    public static class TemplateExtensions
    {
        public static bool IsLinked(this Template template)
        {
            return template.ParentId != null;
        }
    }
}