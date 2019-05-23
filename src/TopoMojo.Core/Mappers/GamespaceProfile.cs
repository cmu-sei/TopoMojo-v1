using AutoMapper;
using TopoMojo.Data.Entities.Extensions;

namespace TopoMojo.Core.Mappers
{
    public class GamespaceProfile : Profile
    {
        public GamespaceProfile()
        {
            CreateMap<Data.Entities.Gamespace, Models.Gamespace>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")));
            CreateMap<Data.Entities.Gamespace, Models.GameState>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Topology.Name));

            CreateMap<Data.Entities.Player, Models.Player>()
                .ForMember(d => d.CanManage, opt => opt.MapFrom((s) => s.Permission.CanManage()))
                .ForMember(d => d.CanEdit, opt => opt.MapFrom((s) => s.Permission.CanEdit()));

        }
    }
}
