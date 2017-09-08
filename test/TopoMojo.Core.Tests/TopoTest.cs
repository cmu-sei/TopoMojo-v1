using System;
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
                Assert.True(topo.Name.Contains("Changed"));
                Assert.True(topo.Description == "original");
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

                var list = mgr.ListMine(new Search {
                    Take = 50,
                    Term = "2",
                    Filters = new SearchFilter[] {
                        new SearchFilter {Name = "published" }
                    }
                }).Result;
                Assert.True(list.Total == 1);
            }
        }
    }
}
