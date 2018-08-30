using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo.Core.Abstractions;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Core
{
    public class EntityManager<T>
        where T : class, IEntity, new()
    {
        public EntityManager(
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        )
        {
            _logger = mill?.CreateLogger(this.GetType());
            _options = options;
            _profileResolver = profileResolver;
        }

        protected readonly ILogger _logger;
        protected readonly CoreOptions _options;

        protected readonly IProfileResolver _profileResolver;
        private Data.Entities.Profile _user;
        protected Data.Entities.Profile Profile
        {
            get
            {
                if (_user == null)
                    _user = Mapper.Map<Data.Entities.Profile>(_profileResolver.Profile);
                return _user;
            }
        }

        protected Action<IMappingOperationOptions> WithActor()
        {
            return opts => { opts.Items["ActorId"] = Profile.Id; };
        }

    }
}
