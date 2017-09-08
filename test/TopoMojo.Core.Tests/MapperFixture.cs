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