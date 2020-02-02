// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Extensions
{
    public static class WorkerExtensions
    {
        public static bool CanManage(this Worker worker)
        {
            return worker.Permission.CanManage();
        }
        public static bool CanEdit(this Worker worker)
        {
            return worker.Permission.CanEdit();
        }
        public static bool IsPending(this Worker worker)
        {
            return worker.Permission == Permission.None;
        }
    }
}
