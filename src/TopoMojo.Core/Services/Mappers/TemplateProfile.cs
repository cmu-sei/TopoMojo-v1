// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using AutoMapper;
using TopoMojo.Data.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class TemplateProfile : Profile
    {
        public TemplateProfile()
        {

            CreateMap<Data.Template, TemplateDetail>().ReverseMap();

            CreateMap<Data.Template, TemplateSummary>();

            CreateMap<Data.Template, Template>()

                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => s.Workspace.CanEdit(r.GetActor())))

                .ReverseMap()
            ;

            CreateMap<NewTemplateDetail, Data.Template>();

            CreateMap<ChangedTemplate, Data.Template>();

            CreateMap<Data.Template, ConvergedTemplate>()
                .ForMember(d => d.Detail, opt => opt.MapFrom(s => s.Detail ?? s.Parent.Detail))
            ;

        }
    }
}
