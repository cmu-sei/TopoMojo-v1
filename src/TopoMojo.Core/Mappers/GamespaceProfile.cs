using AutoMapper;

namespace TopoMojo.Core.Mappers
{
    public class GamespaceProfile : Profile
    {
        public GamespaceProfile()
        {
            CreateMap<Data.Entities.Gamespace, Models.Gamespace>();
            CreateMap<Data.Entities.Gamespace, Models.GameState>()
                .ForMember(d => d.Name, opt => opt.ResolveUsing(s => s.Topology.Name));
        }
    }
}