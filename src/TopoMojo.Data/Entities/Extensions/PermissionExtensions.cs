// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Data.Entities.Extensions
{
    public static class PermissionExtensions
    {
        public static bool CanManage(this Permission flag)
        {
            return flag.HasFlag(Permission.Manager);
        }
        public static bool CanEdit(this Permission flag)
        {
            return flag.HasFlag(Permission.Editor);
        }
        public static bool IsPending(this Permission flag)
        {
            return flag == Permission.None;
        }
    }
}