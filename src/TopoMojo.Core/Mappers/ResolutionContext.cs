// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using TopoMojo.Core.Models;

namespace TopoMojo.Core.Mappers
 {
    public static class ResolutionContextExtensions
    {
        public static Data.Entities.Profile GetActor(this AutoMapper.ResolutionContext res)
        {
            if (res.Items.ContainsKey("Actor"))
                return res.Items["Actor"] as Data.Entities.Profile;

            return new Data.Entities.Profile();
        }
    }
 }
