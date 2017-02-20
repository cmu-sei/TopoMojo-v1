using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Core;

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
            _coreOptions = Options.Create(options);
            _mill = mill;
        }

        private readonly TopoMojoDbContext _ctx = null;
        private readonly IOptions<CoreOptions> _coreOptions;
        private readonly ILoggerFactory _mill;
        private UserResolver _ur;

        private Person _actor;
        public Person Actor
        {
            get { return _actor;}
            set
            {
                _actor = value;
                _ur = new UserResolver(_actor);
            }
        }

        public TopoMojoDbContext Context { get { return _ctx;}}

        #region Managers

        private Dictionary<string, Person> _actors = new Dictionary<string, Person>();
        private Dictionary<Person, Dictionary<string, object>> _mgrStore = new Dictionary<Person, Dictionary<string, object>>();

        public object GetManager(Type t)
        {
            if (!_mgrStore.ContainsKey(_actor))
                _mgrStore.Add(_actor, new Dictionary<string, object>());

            if (!_mgrStore[_actor].ContainsKey(t.Name))
            {
                object mgr = Activator.CreateInstance(t, _ctx, _ur, _coreOptions, _mill);
                _mgrStore[_actor].Add(t.Name, mgr);
            }

            return _mgrStore[_actor][t.Name];
        }

        public TopologyManager GetTopologyManager()
        {
            return GetManager(typeof(TopologyManager)) as TopologyManager;
        }

        #endregion

        public Person AddActor(string name)
        {
            return AddUser(name, true);
        }
        public Person AddUser(string name)
        {
            return AddUser(name, false);
        }
        public Person AddUser(string name, bool makeActor)
        {
            // Person person = _userManager.FindByAccountAsync(name).Result;
            // if (person == null)
            // {
            //     person = _userManager.RegisterWithCredentialsAsync(name, "321ChangeMe!").Result;
            // }
            // _ctx.Entry(person).Reference(o => o.Profile).Load();

            if (!_actors.ContainsKey(name))
                _actors.Add(name, new Person { Name = name });

            Person person = _actors[name];
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