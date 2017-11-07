using AutoMapper;
using TopoMojo.Data.Entities.Extensions;

namespace TopoMojo.Core.Mappers
{
    public class GamespaceProfile : Profile
    {
        public GamespaceProfile()
        {
            CreateMap<Data.Entities.Gamespace, Models.Gamespace>();
            CreateMap<Data.Entities.Gamespace, Models.GameState>()
                .ForMember(d => d.Name, opt => opt.ResolveUsing(s => s.Topology.Name));

            CreateMap<Data.Entities.Player, Models.Player>()
                .ForMember(d => d.CanManage, opt => opt.ResolveUsing((s) => s.Permission.CanManage()))
                .ForMember(d => d.CanEdit, opt => opt.ResolveUsing((s) => s.Permission.CanEdit()));

        }
    }
}