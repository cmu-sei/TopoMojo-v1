// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Data.Entities.Extensions
{
    // public static class IQueryableExtensions
    // {
    //     public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, Search search)
    //         where T : Entity
    //     {
    //         if (search.Skip > 0)
    //         {
    //             q = q.OrderBy(o => o.Name);
    //             q = q.Skip(search.Skip);
    //         }

    //         if (search.Take > 0)
    //         {
    //             q = q.Take(search.Take);
    //         }

    //         return q;
    //     }
    // }
}