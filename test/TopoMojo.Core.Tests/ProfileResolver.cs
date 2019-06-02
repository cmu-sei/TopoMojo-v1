// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;

namespace Tests
{
    public class ProfileResolver : IProfileResolver
    {
        public ProfileResolver(Profile user)
        {
            Profile = user;
        }

        public Profile Profile { get; private set;}

    }
}