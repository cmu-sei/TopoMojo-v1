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
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive()))
                .ForMember(d => d.Challenge, opt => opt.Ignore())
            ;

            CreateMap<Data.Player, Player>()
            ;

            CreateMap<ChallengeSpec, Challenge>()
            ;

            CreateMap<QuestionSpec, Question>()
                .ForMember(d => d.Answer, opt => opt.Ignore())
            ;

            CreateMap<Models.v2.ChallengeSpec, Models.v2.ChallengeView>()
            ;
            CreateMap<Models.v2.SectionSpec, Models.v2.SectionView>()
            ;
            CreateMap<Models.v2.QuestionSpec, Models.v2.QuestionView>()
                .ForMember(d => d.Answer, opt => opt.Ignore())
            ;
        }
    }
}
