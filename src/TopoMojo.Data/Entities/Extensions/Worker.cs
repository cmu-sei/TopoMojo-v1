// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Data.Entities.Extensions
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