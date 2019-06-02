// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

﻿using System;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using Xunit;

namespace Tests
{
    public class TopologyTests : CoreTest
    {
        [Fact]
        public void CanCreateAndEditWorkspace()
        {
            using (TestSession test = CreateSession())
            {
                _options.DefaultWorkspaceLimit = 3;
                test.AddActor("jam@this.ws");
                TopologyManager mgr = test.GetTopologyManager();
                Topology topo = mgr.Create(new NewTopology {
                    Name = "JamOn",
                    Description = "original"
                }).Result;

                Assert.True(topo.Id > 0);
                Assert.True(topo.CanManage);
                Assert.True(topo.CanEdit);

                topo = mgr.Update(new ChangedTopology {
                    Id = topo.Id,
                    Name = topo.Name + "Changed",
                    Description = topo.Description}).Result;
                Assert.Matches("Changed", topo.Name);
                Assert.Equal("original", topo.Description);
            }
        }

        [Fact]
        public void ListReturnsList()
        {
            using (TestSession test = CreateSession())
            {
                test.AddActor("jam@this.ws");
                TopologyManager mgr = test.GetTopologyManager();
                for (int i = 0; i < 5; i++)
                {
                    Topology topo = mgr.Create(new NewTopology {
                        Name = "JamOn" + i.ToString(),
                        Description = i.ToString()
                    }).Result;

                    if (i > 2)
                        mgr.Publish(topo.Id, false).Wait();
                }

                var list = mgr.List(new Search {
                    Take = 50,
                    //Term = "2",
                    Filters = new string[] {
                        "published",
                        "mine"
                    }
                }).Result;
                Assert.True(list.Total == 2);
            }
        }
    }
}
