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
            IProfileResolver profileResolver,
            IProfileCache profileCache
        )
        {
            _profileRepo = profileRepo;
            _logger = mill?.CreateLogger(this.GetType());
            _options = options;
            _profileResolver = profileResolver;
            _profileCache = profileCache;
        }

        protected readonly IProfileRepository _profileRepo;
        protected readonly IProfileCache _profileCache;
        private Data.Entities.Profile _user;
        protected Data.Entities.Profile Profile
        {
            get
            {
                if (_user == null)
                {
                    string id = _profileResolver.Profile.GlobalId;
                    if (id.HasValue())
                    {
                        _user = _profileCache.Find(id);
                        if (_user == null)
                        {
                            _user = _profileRepo.FindByGlobalId(id).Result;
                            if (_user == null)
                            {
                                _user.WorkspaceLimit = _options.DefaultWorkspaceLimit;
                                _user = _profileRepo.Add(_user).Result;
                            }
                            _profileCache.Add(_user);
                        }
                    }
                    else
                    {
                        _user = Mapper.Map<Data.Entities.Profile>(_profileResolver.Profile);
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
