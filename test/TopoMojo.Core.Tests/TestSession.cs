using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;

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
        }

        private readonly TopoMojoDbContext _ctx = null;
        private readonly CoreOptions _coreOptions;
        private readonly ILoggerFactory _mill;
        private IProfileResolver _ur;

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

        public object GetManager(Type t)
        {
            if (!_mgrStore.ContainsKey(_actor))
                _mgrStore.Add(_actor, new Dictionary<string, object>());

            if (!_mgrStore[_actor].ContainsKey(t.Name))
            {
                object mgr = Activator.CreateInstance(t, _ctx, _mill, _coreOptions, _ur);
                _mgrStore[_actor].Add(t.Name, mgr);
            }

            return _mgrStore[_actor][t.Name];
        }

        public TopologyManager GetTopologyManager()
        {
            return GetManager(typeof(TopologyManager)) as TopologyManager;
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
        public Profile AddUser(string name, bool makeActor)
        {
            if (!_actors.ContainsKey(name))
            {
                Profile target = _ctx.Profiles
                    .Where(p => p.Name == name)
                    .FirstOrDefault();

                if (target == null)
                {
                    target = new Profile
                    {
                        Name = name,
                        GlobalId = Guid.NewGuid().ToString()
                    };
                    _ctx.Profiles.Add(target);
                    _ctx.SaveChanges();
                }
                _actors.Add(name, target);
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