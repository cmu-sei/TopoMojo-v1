// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo;
using TopoMojo.Abstractions;
using TopoMojo.Models;
using TopoMojo.Data;
using TopoMojo.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Tests
{
    public class TestSession : IDisposable
    {
        public TestSession(
            TopoMojoDbContext ctx,
            CoreOptions options,
            ILoggerFactory mill,
            IMapper mapper,
            IDistributedCache cache,
            IMemoryCache memoryCache
        )
        {
            _ctx = ctx;
            _coreOptions = new CoreOptions();
            _mill = mill;
            _mapper = mapper;
            _cache = cache;
            _memoryCache = memoryCache;
            _proman = new TopoMojo.Services.IdentityService(_mapper, new UserStore(_ctx, memoryCache, cache));
            // _proman = new ProfileManager(
            //     new ProfileRepository(_ctx),
            //     _mill,
            //     _coreOptions,
            //     new IdentityResolver(new Profile
            //     {
            //         Name = "admin@test",
            //         IsAdmin = true
            //     }),
            // //     new MemoryCache()
            // );
            AddUser("tester@test", true, true);
        }

        IMapper _mapper;
        private readonly TopoMojoDbContext _ctx = null;
        private readonly CoreOptions _coreOptions;
        private readonly ILoggerFactory _mill;
        private IIdentityResolver _ur;
        private readonly TopoMojo.Services.IdentityService _proman;
        private IDistributedCache _cache;
        private IMemoryCache _memoryCache;
        private TopoMojo.Models.User _actor;
        public TopoMojo.Models.User Actor
        {
            get { return _actor;}
            set
            {
                _actor = value;
                _ur = new IdentityResolver(_actor);
            }
        }

        public TopoMojoDbContext Context { get { return _ctx;}}

        #region Managers

        private Dictionary<string, TopoMojo.Models.User> _actors = new Dictionary<string, TopoMojo.Models.User>();
        private Dictionary<TopoMojo.Models.User, Dictionary<string, object>> _mgrStore = new Dictionary<TopoMojo.Models.User, Dictionary<string, object>>();

        private object FindManager(Type t)
        {
            if (_mgrStore[_actor].ContainsKey(t.Name))
                return _mgrStore[_actor][t.Name];
            return null;
        }

        // public object GetManager(Type t)
        // {
        //     if (!_mgrStore.ContainsKey(_actor))
        //         _mgrStore.Add(_actor, new Dictionary<string, object>());

        //     if (!_mgrStore[_actor].ContainsKey(t.Name))
        //     {
        //         object mgr = Activator.CreateInstance(t, _mill, _coreOptions, _ur);
        //         _mgrStore[_actor].Add(t.Name, mgr);
        //     }

        //     return _mgrStore[_actor][t.Name];
        // }

        public WorkspaceService GetTopologyManager()
        {
            Type t = typeof(WorkspaceService);
            object mgr = FindManager(t);
            if (mgr == null)
            {
                mgr = Activator.CreateInstance(t, new UserStore(_ctx, _memoryCache, _cache),
                    new WorkspaceStore(_ctx, _memoryCache, _cache),
                    new GamespaceStore(_ctx, _memoryCache, _cache),
                    _mill, _mapper, _coreOptions, _ur, null);
                _mgrStore[_actor].Add(t.Name, mgr);
            }
            return mgr as WorkspaceService;
        }

        public TemplateService GetTemplateManager()
        {
            Type t = typeof(TemplateService);
            object mgr = FindManager(t);
            if (mgr == null)
            {
                mgr = Activator.CreateInstance(t, new UserStore(_ctx, _memoryCache, _cache),
                    new TemplateStore(_ctx, _memoryCache, _cache),
                    _mill, _mapper, _coreOptions, _ur, null);
                _mgrStore[_actor].Add(t.Name, mgr);
            }
            return mgr as TemplateService;
        }

        #endregion

        public TopoMojo.Models.User AddActor(string name)
        {
            return AddUser(name, true);
        }

        public TopoMojo.Models.User AddUser(string name)
        {
            return AddUser(name, false);
        }

        public TopoMojo.Models.User AddUser(string name, bool makeActor, bool isAdmin = false)
        {
            if (!_actors.ContainsKey(name))
            {
                TopoMojo.Models.User target = new TopoMojo.Models.User
                {
                    Name = name,
                    IsAdmin = isAdmin,
                    GlobalId = Guid.NewGuid().ToString()
                };

                target = _proman.Add(target).Result;

                _actors.Add(name, target);
                _mgrStore.Add(target, new Dictionary<string, object>());
            }

            TopoMojo.Models.User person = _actors[name];
            if (makeActor)
                Actor = person;

            return person;
        }

        public void Dispose()
        {
            if (_ctx != null)
                _ctx.Dispose();
        }
    }
}
