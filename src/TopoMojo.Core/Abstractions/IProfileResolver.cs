// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Security.Principal;
using System.Threading.Tasks;
using TopoMojo.Core.Models;

namespace TopoMojo.Core.Abstractions
{
    public interface IProfileResolver
    {
        Profile Profile { get; }
    }
}