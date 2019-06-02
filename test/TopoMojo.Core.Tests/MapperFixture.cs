// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using AutoMapper;
using TopoMojo.Core.Mappers;

namespace Tests
{
    public class MapperFixture
    {
        public MapperFixture()
        {
            Mapper.Initialize(cfg => {
                cfg.AddProfile<ActorProfile>();
                cfg.AddProfile<TopologyProfile>();
                cfg.AddProfile<TemplateProfile>();
            });
            // Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}