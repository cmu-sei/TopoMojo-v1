// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using TopoMojo.Models;

namespace TopoMojo.Services
 {
    public static class ResolutionContextExtensions
    {
        public static User GetActor(this AutoMapper.ResolutionContext res)
        {
            if (res.Items.ContainsKey("Actor"))
                return res.Items["Actor"] as User;

            return new User();
        }
    }
 }
