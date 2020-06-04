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

                .ForMember(d => d.IsAdmin, opt => opt.MapFrom(s => s.Role.HasFlag(Data.UserRole.Administrator)))
                .ReverseMap()
            ;

            CreateMap<ChangedUser, Data.User>();
        }
    }
}
