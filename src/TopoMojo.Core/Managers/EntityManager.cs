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
            IProfileRepository profileRepo,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        )
        {
            _profileRepo = profileRepo;
            _logger = mill?.CreateLogger(this.GetType());
            _options = options;
            _profileResolver = profileResolver;
        }

        protected readonly IProfileRepository _profileRepo;
        private Data.Entities.Profile _user;
        protected Data.Entities.Profile Profile
        {
            get
            {
                if (_user == null)
                {
                    _user = Mapper.Map<Data.Entities.Profile>(_profileResolver.Profile);
                    if (_user.GlobalId.HasValue() && _user.Id == 0)
                    {
                        var profile = _user.Id > 0
                            ? _profileRepo.Load(_user.Id).Result
                            : _profileRepo.FindByGlobalId(_user.GlobalId).Result;

                        if (profile == null)
                        {
                            _user.WorkspaceLimit = _options.DefaultWorkspaceLimit;
                            _user = _profileRepo.Add(_user).Result;
                        }
                        else
                        {
                            _user = profile;
                        }
                    }
                }
                return _user;
            }
        }

        protected Action<IMappingOperationOptions> WithActor()
        {
            return opts => { opts.Items["ActorId"] = Profile.Id; };
        }

        protected readonly ILogger _logger;
        protected readonly IProfileResolver _profileResolver;
        protected readonly CoreOptions _options;

    }
}
