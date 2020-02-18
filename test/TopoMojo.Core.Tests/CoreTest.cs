// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Data.EntityFrameworkCore;
using Xunit;

namespace Tests
{
    public class CoreTest : IClassFixture<MapperFixture>
    {
        public CoreTest()
        {
            Initialize();
        }

        protected IIdentityResolver _ur = null;
        protected const string _complexPassword = "~Tartans@1~";
        protected IOptions<CoreOptions> _optAccessor = null;
        protected CoreOptions _options = null;

        protected User _user = null;
        protected ILoggerFactory _mill = null;
        private DbContextOptions<TopoMojoDbContext> _dbOptions;

        protected TestSession CreateSession()
        {
            return new TestSession(
                CreateContext(),
                _options,
                _mill
            );
        }

        protected TopoMojoDbContext CreateContext()
        {
            return new TopoMojoDbContext(_dbOptions);
        }

        protected void Initialize()
        {
            _options = new CoreOptions();
            _mill = new LoggerFactory();

            _dbOptions = new DbContextOptionsBuilder<TopoMojoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (TopoMojoDbContext ctx = new TopoMojoDbContext(_dbOptions))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
                _user = new User { Id = 1, Name = "tester" };
                _ur = new IdentityResolver(_user);
            }

        }
    }
}
