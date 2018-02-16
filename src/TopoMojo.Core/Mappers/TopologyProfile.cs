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
                    opt.ResolveUsing((s, d, m, r) => s.Author ?? s.Workers.FirstOrDefault()?.Person?.Name))
                .ForMember(d => d.CanManage, opt =>
                    opt.ResolveUsing((s, d, m, r) => s.Workers.Any(w => w.PersonId == r.GetActorId() && w.CanManage())))
                .ForMember(d => d.CanEdit, opt =>
                    opt.ResolveUsing((s, d, m, r) => s.Workers.Any(w => w.PersonId == r.GetActorId() && w.CanEdit())))
                .ReverseMap();

            CreateMap<Data.Entities.Topology, Models.TopologySummary>()
                .ForMember(d => d.Author, opt =>
                    opt.ResolveUsing((s, d, m, r) => s.Author ?? s.Workers.FirstOrDefault()?.Person?.Name))
                .ForMember(d => d.CanManage, opt =>
                    opt.ResolveUsing((s, d, m, r) => s.Workers.Any(w => w.PersonId == r.GetActorId() && w.CanManage())))
                .ForMember(d => d.CanEdit, opt =>
                    opt.ResolveUsing((s, d, m, r) => s.Workers.Any(w => w.PersonId == r.GetActorId() && w.CanEdit())));

            CreateMap<Data.Entities.Topology, Models.TopologyState>();
            CreateMap<Models.NewTopology, Data.Entities.Topology>();

            CreateMap<Models.ChangedTopology, Data.Entities.Topology>();

            CreateMap<Data.Entities.Worker, Models.Worker>()
                .ForMember(d => d.CanManage, opt => opt.ResolveUsing((s) => s.CanManage()))
                .ForMember(d => d.CanEdit, opt => opt.ResolveUsing((s) => s.CanEdit()));

        }
    }
}