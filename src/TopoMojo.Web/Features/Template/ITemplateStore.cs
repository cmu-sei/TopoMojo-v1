// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface ITemplateStore : IStore<Template>
    {
        Task<Template> Load(string id);
        Task<bool> HasDescendents(string id);
        Task<bool> AtTemplateLimit(string workspaceId);
        Task<Template[]> ListChildren(int parentId);
        Task<string> ResolveKey(string key);
    }
}
