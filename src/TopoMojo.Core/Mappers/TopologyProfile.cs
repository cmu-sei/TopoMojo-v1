using System;
using System.Linq;
using AutoMapper;
using TopoMojo.Data.Entities.Extensions;

namespace TopoMojo.Core.Mappers
{
    public class TopologyProfile : Profile
    {
        public TopologyProfile()
        {
            CreateMap<Data.Entities.Topology, Models.Topology>()
                .ForMember(d => d.Author, opt =>
                    opt.MapFrom((s, d, m, r) => s.Author ?? s.Workers.FirstOrDefault()?.Person?.Name))
                .ForMember(d => d.CanManage, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanManage()))))
                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanEdit()))))
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.GamespaceCount, opt => opt.MapFrom(s => s.Gamespaces.Count))
                .ReverseMap();

            CreateMap<Data.Entities.Topology, Models.TopologySummary>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.CanManage, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanManage()))))
                .ForMember(d => d.CanEdit, opt =>
                    opt.MapFrom((s, d, m, r) => r.GetActor().IsAdmin || (s.Workers.Any(w => w.PersonId == r.GetActor().Id && w.CanEdit()))));

            CreateMap<Data.Entities.Topology, Models.TopologyState>();
            CreateMap<Models.NewTopology, Data.Entities.Topology>();

            CreateMap<Models.ChangedTopology, Data.Entities.Topology>();
            CreateMap<Models.PrivilegedWorkspaceChanges, Data.Entities.Topology>();

            CreateMap<Data.Entities.Worker, Models.Worker>()
                .ForMember(d => d.CanManage, opt => opt.MapFrom((s) => s.CanManage()))
                .ForMember(d => d.CanEdit, opt => opt.MapFrom((s) => s.CanEdit()));

        }
    }
}
