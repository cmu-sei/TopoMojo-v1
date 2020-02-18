// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TopoMojo.Core;
using TopoMojo.Abstractions;
using TopoMojo.Models;
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

            _proman = new TopoMojo.Core.IdentityService(new UserStore(_ctx));
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

        private readonly TopoMojoDbContext _ctx = null;
        private readonly CoreOptions _coreOptions;
        private readonly ILoggerFactory _mill;
        private IIdentityResolver _ur;
        private readonly TopoMojo.Core.IdentityService _proman;
        private User _actor;
        public User Actor
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

        private Dictionary<string, User> _actors = new Dictionary<string, User>();
        private Dictionary<User, Dictionary<string, object>> _mgrStore = new Dictionary<User, Dictionary<string, object>>();

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
                mgr = Activator.CreateInstance(t, new UserStore(_ctx),
                    new WorkspaceStore(_ctx),
                    new GamespaceStore(_ctx),
                    _mill, _coreOptions, _ur, null);
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
                mgr = Activator.CreateInstance(t, new UserStore(_ctx),
                    new TemplateStore(_ctx),
                    _mill, _coreOptions, _ur, null);
                _mgrStore[_actor].Add(t.Name, mgr);
            }
            return mgr as TemplateService;
        }

        #endregion

        public User AddActor(string name)
        {
            return AddUser(name, true);
        }

        public User AddUser(string name)
        {
            return AddUser(name, false);
        }

        public User AddUser(string name, bool makeActor, bool isAdmin = false)
        {
            if (!_actors.ContainsKey(name))
            {
                User target = new User
                {
                    Name = name,
                    IsAdmin = isAdmin,
                    GlobalId = Guid.NewGuid().ToString()
                };

                target = _proman.Add(target).Result;

                _actors.Add(name, target);
                _mgrStore.Add(target, new Dictionary<string, object>());
            }

            User person = _actors[name];
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
