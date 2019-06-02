// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;
using TopoMojo.Core.Privileged;
using TopoMojo.Data;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.EntityFrameworkCore;

namespace Tests
{
    public class TestSession : IDisposable
    {
        public TestSession(
            TopoMojoDbContext ctx,
            CoreOptions options,
            ILoggerFactory mill
        )
        {
            _ctx = ctx;
            _coreOptions = new CoreOptions();
            _mill = mill;

            _proman = new ProfileService(new ProfileRepository(_ctx));
            // _proman = new ProfileManager(
            //     new ProfileRepository(_ctx),
            //     _mill,
            //     _coreOptions,
            //     new ProfileResolver(new Profile
            //     {
            //         Name = "admin@test",
            //         IsAdmin = true
            //     }),
            // //     new MemoryCache()
            // );
            AddUser("tester@test", true, true);
        }

        private readonly TopoMojoDbContext _ctx = null;
        private readonly CoreOptions _coreOptions;
        private readonly ILoggerFactory _mill;
        private IProfileResolver _ur;
        private readonly ProfileService _proman;
        private Profile _actor;
        public Profile Actor
        {
            get { return _actor;}
            set
            {
                _actor = value;
                _ur = new ProfileResolver(_actor);
            }
        }

        public TopoMojoDbContext Context { get { return _ctx;}}

        #region Managers

        private Dictionary<string, Profile> _actors = new Dictionary<string, Profile>();
        private Dictionary<Profile, Dictionary<string, object>> _mgrStore = new Dictionary<Profile, Dictionary<string, object>>();

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

        public TopologyManager GetTopologyManager()
        {
            Type t = typeof(TopologyManager);
            object mgr = FindManager(t);
            if (mgr == null)
            {
                mgr = Activator.CreateInstance(t, new ProfileRepository(_ctx),
                    new TopologyRepository(_ctx),
                    new GamespaceRepository(_ctx),
                    _mill, _coreOptions, _ur, null);
                _mgrStore[_actor].Add(t.Name, mgr);
            }
            return mgr as TopologyManager;
        }

        public TemplateManager GetTemplateManager()
        {
            Type t = typeof(TemplateManager);
            object mgr = FindManager(t);
            if (mgr == null)
            {
                mgr = Activator.CreateInstance(t, new ProfileRepository(_ctx),
                    new TemplateRepository(_ctx),
                    _mill, _coreOptions, _ur, null);
                _mgrStore[_actor].Add(t.Name, mgr);
            }
            return mgr as TemplateManager;
        }

        #endregion

        public Profile AddActor(string name)
        {
            return AddUser(name, true);
        }

        public Profile AddUser(string name)
        {
            return AddUser(name, false);
        }

        public Profile AddUser(string name, bool makeActor, bool isAdmin = false)
        {
            if (!_actors.ContainsKey(name))
            {
                Profile target = new Profile
                {
                    Name = name,
                    IsAdmin = isAdmin,
                    GlobalId = Guid.NewGuid().ToString()
                };

                target = _proman.Add(target).Result;

                _actors.Add(name, target);
                _mgrStore.Add(target, new Dictionary<string, object>());
            }

            Profile person = _actors[name];
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