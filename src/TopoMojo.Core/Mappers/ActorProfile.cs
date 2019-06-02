// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

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
