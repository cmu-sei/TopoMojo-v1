// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Core
{
    public class EntityService<T>
        where T : class, IEntity, new()
    {
        public EntityService(
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
        private Data.Profile _user;
        protected Data.Profile User
        {
            get
            {
                if (_user == null)
                    _user = Mapper.Map<Data.Profile>(_profileResolver.User);
                return _user;
            }
        }

        protected Action<IMappingOperationOptions> WithActor()
        {
            return opts => {
                opts.Items["Actor"] = User;
            };
        }

    }
}
