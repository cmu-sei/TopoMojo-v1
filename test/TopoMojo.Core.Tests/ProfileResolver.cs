// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using TopoMojo.Abstractions;
using TopoMojo.Models;

namespace Tests
{
    public class ProfileResolver : IProfileResolver
    {
        public ProfileResolver(User user)
        {
            User = user;
        }

        public User User { get; private set;}

    }
}
