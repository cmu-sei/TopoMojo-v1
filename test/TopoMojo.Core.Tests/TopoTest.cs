using System;
using TopoMojo.Core;
using Xunit;

namespace Tests
{
    public class TopologyTests : CoreTest
    {
        [Fact]
        public void topology()
        {
            using (TestSession test = CreateSession())
            {
                test.AddActor("jam@this.ws");
                TopologyManager mgr = test.GetTopologyManager();
                mgr.SaveAsync(new Topology {
                    Name = "JamOn"
                }).Wait();

                Search<Topology> search = mgr.ListAsync(new Search<Topology>()).Result;
                Assert.True(search.Total > 0);
            }
        }
    }
}
