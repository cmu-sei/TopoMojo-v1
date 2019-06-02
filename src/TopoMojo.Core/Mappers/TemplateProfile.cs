// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using AutoMapper;
using TopoMojo.Data.Entities.Extensions;

namespace TopoMojo.Core.Mappers
{
    public class TemplateProfile : Profile
    {
        public TemplateProfile()
        {
            CreateMap<Data.Entities.Template, Data.Entities.Template>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Detail, opt => opt.Ignore())
                .ForMember(d => d.IsPublished, opt => opt.Ignore())
                .ForMember(d => d.ParentId, opt => opt.MapFrom(s => s.Id));

            CreateMap<Data.Entities.Template, Models.TemplateDetail>().ReverseMap();
            CreateMap<Data.Entities.Template, Models.TemplateSummary>();
            CreateMap<Data.Entities.Template, Models.Template>()
                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Topology != null && s.Topology.Workers.Any(w => w.PersonId == r.GetActor().Id))))
                .ReverseMap();

            CreateMap<Models.NewTemplateDetail, Data.Entities.Template>();
            CreateMap<Models.ChangedTemplate, Data.Entities.Template>();


        }
    }
}
