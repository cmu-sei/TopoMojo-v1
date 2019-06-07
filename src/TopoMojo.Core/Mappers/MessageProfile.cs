// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

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
