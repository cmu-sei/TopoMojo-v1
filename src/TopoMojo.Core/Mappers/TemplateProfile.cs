// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using AutoMapper;
using TopoMojo.Models;

namespace TopoMojo.Core.Mappers
{
    public class TemplateProfile : Profile
    {
        public TemplateProfile()
        {

            CreateMap<Data.Template, TemplateDetail>().ReverseMap();
            CreateMap<Data.Template, TemplateSummary>();
            CreateMap<Data.Template, Template>()
                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Topology != null && s.Topology.Workers.Any(w => w.PersonId == r.GetActor().Id))))
                .ReverseMap();

            CreateMap<NewTemplateDetail, Data.Template>();
            CreateMap<ChangedTemplate, Data.Template>();

            CreateMap<Data.Template, ConvergedTemplate>()
                .ForMember(d => d.Detail, opt => opt.MapFrom(s => s.Detail ?? s.Parent.Detail));
        }
    }
}
