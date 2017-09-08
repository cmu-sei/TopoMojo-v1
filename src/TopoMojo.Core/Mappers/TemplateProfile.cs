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
            CreateMap<Data.Entities.Template, Models.Template>()
                .ForMember(d => d.CanEdit, opt =>
                    opt.ResolveUsing((s, d, m, r) => s.Topology.Workers.Any(w => w.PersonId == r.GetActorId())))
                .ReverseMap();

            CreateMap<Data.Entities.Template, Models.TemplateDetail>().ReverseMap();
            CreateMap<Data.Entities.Template, Models.LinkedTemplate>();

        }
    }
}