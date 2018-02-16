using AutoMapper;

namespace TopoMojo.Core.Mappers
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Models.NewMessage, Data.Entities.Message>();
            CreateMap<Models.ChangedMessage, Data.Entities.Message>();
            CreateMap<Data.Entities.Message, Models.Message>();
        }
    }
}