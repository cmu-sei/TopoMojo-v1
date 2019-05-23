using AutoMapper;

namespace TopoMojo.Core.Mappers
{
    public class ActorProfile : Profile
    {
        public ActorProfile()
        {
            CreateMap<Data.Entities.Profile, Models.Profile>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
            .ReverseMap();
            CreateMap<Models.ChangedProfile, Data.Entities.Profile>();
        }
    }
}
