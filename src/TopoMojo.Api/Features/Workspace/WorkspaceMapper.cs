// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Text.Json;
using AutoMapper;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Data.Workspace, Workspace>()

                .ForMember(d => d.GamespaceCount, opt => opt.MapFrom(s => s.Gamespaces.Count))
            ;

            CreateMap<Data.Workspace, WorkspaceSummary>()
            ;

            CreateMap<Data.Workspace, WorkspaceInvitation>();

            CreateMap<NewWorkspace, Data.Workspace>()
                .ForMember(d => d.Challenge, opt => opt.MapFrom(s => s.Challenge ??
                    JsonSerializer.Serialize<ChallengeSpec>(
                        new ChallengeSpec(),
                        null
                    )
                ))
            ;

            CreateMap<ChangedWorkspace, Data.Workspace>();

            CreateMap<Data.Worker, Worker>();

        }
    }
}
