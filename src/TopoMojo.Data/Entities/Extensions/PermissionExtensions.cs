// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Extensions
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
