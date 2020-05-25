// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface ITemplateStore : IDataStore<Template>
    {
        Task<bool> IsParentTemplate(int id);
        Task<bool> AtTemplateLimit(int topoId);
        Task<Template[]> ListChildren(int parentId);
        Task<string> ResolveKey(string key);
    }
}
