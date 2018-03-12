using AutoMapper;

namespace TopoMojo.Core.Mappers
{
    public class ActorProfile : Profile
    {
        public ActorProfile()
        {
            CreateMap<Data.Entities.Profile, Models.Profile>().ReverseMap();
            CreateMap<Models.ChangedProfile, Data.Entities.Profile>();
        }
    }
}