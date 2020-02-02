// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using AutoMapper;
using TopoMojo.Data.Extensions;
using TopoMojo.Models.Workspace;

namespace TopoMojo.Core.Mappers
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Data.Topology, Workspace>()
                .ForMember(d => d.Author, opt =>
                    opt.MapFrom((s, d, m, r) => s.Author ?? s.Workers.FirstOrDefault()?.Person?.Name))
                .ForMember(d => d.CanManage, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanManage()))))
                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanEdit()))))
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.GamespaceCount, opt => opt.MapFrom(s => s.Gamespaces.Count))
                .ReverseMap();

            CreateMap<Data.Topology, WorkspaceSummary>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.CanManage, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanManage()))))
                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanEdit()))));

            CreateMap<Data.Topology, WorkspaceState>();
            CreateMap<NewWorkspace, Data.Topology>();

            CreateMap<ChangedWorkspace, Data.Topology>();
            CreateMap<PrivilegedWorkspaceChanges, Data.Topology>();

            CreateMap<Data.Worker, Worker>()
                .ForMember(d => d.CanManage, opt => opt.MapFrom((s) => s.CanManage()))
                .ForMember(d => d.CanEdit, opt => opt.MapFrom((s) => s.CanEdit()));

        }
    }
}
