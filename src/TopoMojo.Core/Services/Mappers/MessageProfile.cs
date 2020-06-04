// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using AutoMapper;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<NewMessage, Data.Message>();

            CreateMap<ChangedMessage, Data.Message>();

            CreateMap<Data.Message, Message>();
        }
    }
}
