// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using AutoMapper;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Data.Workspace, Workspace>()

                .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Name.ToSlug()))

                .ForMember(d => d.Author, opt =>
                    opt.MapFrom((s, d, m, r) => s.Author ?? s.Workers.FirstOrDefault()?.Person?.Name))

                .ForMember(d => d.CanManage, opt =>
                    opt.MapFrom((s, d, m, r) => s.CanManage(r.GetActor())))

                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => s.CanEdit(r.GetActor())))

                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))

                .ForMember(d => d.GamespaceCount, opt => opt.MapFrom(s => s.Gamespaces.Count))
            ;

            CreateMap<Data.Workspace, WorkspaceSummary>()

                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))

                .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Name.ToSlug()))

                .ForMember(d => d.CanManage, opt =>
                    opt.MapFrom((s, d, m, r) => s.CanManage(r.GetActor())))

                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => s.CanEdit(r.GetActor())))
            ;

            CreateMap<Data.Workspace, WorkspaceInvitation>();

            CreateMap<NewWorkspace, Data.Workspace>();

            CreateMap<ChangedWorkspace, Data.Workspace>();

            CreateMap<Data.Worker, Worker>()

                .ForMember(d => d.CanManage, opt => opt.MapFrom((s) => s.CanManage()))

                .ForMember(d => d.CanEdit, opt => opt.MapFrom((s) => s.CanEdit()))
            ;

        }
    }
}
