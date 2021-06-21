// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
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
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Workspace.Name))
                .ForMember(d => d.Challenge, opt => opt.Ignore())
            ;

            CreateMap<Data.Player, Player>()
            ;

            CreateMap<ChallengeSpec, ChallengeView>()
            ;

            CreateMap<SectionSpec, SectionView>()
            ;

            CreateMap<QuestionSpec, QuestionView>()
                .ForMember(d => d.Answer, opt => opt.Ignore())
            ;
        }
    }
}
