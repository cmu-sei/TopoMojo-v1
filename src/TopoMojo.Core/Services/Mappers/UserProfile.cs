// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using AutoMapper;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Data.User, User>()

                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))

                .ReverseMap()
            ;

            CreateMap<ChangedUser, Data.User>();

            CreateMap<ChangedUser, User>();
        }
    }
}
