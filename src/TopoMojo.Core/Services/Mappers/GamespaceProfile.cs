// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using AutoMapper;
using TopoMojo.Models;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;

namespace TopoMojo.Services
{
    public class GamespaceProfile : Profile
    {
        public GamespaceProfile()
        {
            CreateMap<Data.Gamespace, Gamespace>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Name.ToSlug()))
            ;

            CreateMap<Data.Gamespace, GameState>()
                .ForMember(d => d.WhenCreated, opt => opt.MapFrom(s => s.WhenCreated.ToString("u")))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Topology.Name))
            ;

            CreateMap<Data.Player, Player>()
                .ForMember(d => d.CanManage, opt => opt.MapFrom((s) => s.Permission.CanManage()))
                .ForMember(d => d.CanEdit, opt => opt.MapFrom((s) => s.Permission.CanEdit()))
            ;

        }
    }
}
