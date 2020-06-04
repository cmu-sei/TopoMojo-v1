// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public abstract class _Service
    {
        public _Service(
            ILogger logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        )
        {
            _logger = logger;
            _options = options;
            _identityResolver = identityResolver;
            Mapper = mapper;
        }

        protected IMapper Mapper { get; }
        protected ILogger _logger { get; }
        protected CoreOptions _options { get; }
        private IIdentityResolver _identityResolver { get; }
        // private Data.Profile _user;
        // protected Data.Profile User
        // {
        //     get
        //     {
        //         if (_user == null)
        //             _user = Mapper.Map<Data.Profile>(_identityResolver.User);
        //         return _user;
        //     }
        // }

        protected User User => _identityResolver.User;
        protected Client Client => _identityResolver.Client;

        protected Action<IMappingOperationOptions> WithActor()
        {
            return opts => {
                opts.Items["Actor"] = User;
            };
        }

    }
}
