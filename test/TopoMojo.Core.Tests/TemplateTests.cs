using System;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using Xunit;

namespace Tests
{
    public class TemplateTests : CoreTest
    {
        [Fact]
        public void CanCreateAndEditTemplates()
        {
            using (TestSession test = CreateSession())
            {
                TemplateManager mgr = test.GetTemplateManager();
                TemplateDetail template = mgr.Create(new TemplateDetail {
                    Name = "JamOn",
                    Detail = "original"
                }).Result;

                Assert.True(template.Id > 0);

                template.Name += "Changed";
                template.Detail = "detail";
                template = mgr.Configure(template).Result;
                Assert.True(template.Name.Contains("Changed"));
                Assert.True(template.Detail == "detail");
            }
        }

        [Fact]
        public void ListReturnsList()
        {
            using (TestSession test = CreateSession())
            {
                //test.AddActor("jam@this.ws");
                TemplateManager mgr = test.GetTemplateManager();
                for (int i = 0; i < 5; i++)
                {
                    TemplateDetail template = mgr.Create(new TemplateDetail {
                        Name = "JamOn" + i.ToString(),
                        Detail = i.ToString(),
                        IsPublished = (i%2==0)
                    }).Result;
                }

                var list = mgr.List(new Search {
                    Take = 50,
                    //Term = "2",
                    Filters = new string[] { "published" }
                }).Result;
                Assert.True(list.Total == 3);
            }
        }

        [Fact]
        public void CanLinkTemplate()
        {
            using (TestSession test = CreateSession())
            {
                //test.AddActor("jam@this.ws");
                TopologyManager topoman = test.GetTopologyManager();
                Topology topo = topoman.Create(new NewTopology{
                    Name = "jamTopo"
                }).Result;
                TemplateManager mgr = test.GetTemplateManager();
                for (int i = 1; i < 6; i++)
                {
                    TemplateDetail template = mgr.Create(new TemplateDetail {
                        Name = "JamOn" + i.ToString(),
                        // Detail = "",
                        IsPublished = (i%2==1)
                    }).Result;
                }

                Template t = mgr.Link(5, topo.Id).Result;
                t = mgr.Unlink(t.Id).Result;
                var list = mgr.List(new Search {
                    Take = 50,
                    //Term = "2",
                    Filters = new string[] {
                        //"published"
                    }
                }).Result;
                Assert.True(list.Total == 6);
            }
        }
    }
}
