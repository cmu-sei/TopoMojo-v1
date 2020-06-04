// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using TopoMojo.Models;

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

        public static bool CanManage(this Worker worker)
        {
            return worker.Permission.CanManage();
        }

        public static bool CanEdit(this Worker worker)
        {
            return worker.Permission.CanEdit();
        }

        public static bool CanManage(this Player player)
        {
            return player.Permission.CanManage();
        }

        public static bool CanEdit(this Player player)
        {
            return player.Permission.CanEdit();
        }

        public static bool CanManage(this Workspace workspace, Models.User user)
        {
            return user.IsAdmin
                || (workspace.Workers
                    .FirstOrDefault(p => p.Id == user.Id)?
                    .Permission.CanManage() ?? false);
        }

        public static bool CanEdit(this Workspace workspace, Models.User user)
        {
            return user.IsAdmin
                || (workspace.Workers
                    .FirstOrDefault(p => p.Id == user.Id)?
                    .Permission.CanEdit() ?? false);
        }

        public static bool CanManage(this Gamespace player, Models.User user)
        {
            return user.IsAdmin
                || (player.Players
                    .FirstOrDefault(p => p.Id == user.Id)?
                    .Permission.CanManage() ?? false);
        }

        public static bool CanEdit(this Gamespace player, Models.User user)
        {
            return user.IsAdmin
                || (player.Players
                    .FirstOrDefault(p => p.Id == user.Id)?
                    .Permission.CanEdit() ?? false);
        }
    }
}
