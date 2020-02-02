// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using TopoMojo.Core;
using TopoMojo.Models;
using TopoMojo.Models.Workspace;
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
                TemplateService mgr = test.GetTemplateManager();
                TemplateDetail template = mgr.Create(new TemplateDetail {
                    Name = "JamOn",
                    Detail = "original"
                }).Result;

                Assert.True(template.Id > 0);

                template.Name += "Changed";
                template.Detail = "detail";
                template = mgr.Configure(template).Result;
                Assert.Matches("Changed", template.Name);
                Assert.Equal("detail", template.Detail);
            }
        }

        [Fact]
        public void ListReturnsList()
        {
            using (TestSession test = CreateSession())
            {
                //test.AddActor("jam@this.ws");
                TemplateService mgr = test.GetTemplateManager();
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
                WorkspaceService topoman = test.GetTopologyManager();
                Workspace topo = topoman.Create(new NewWorkspace{
                    Name = "jamTopo"
                }).Result;
                TemplateService mgr = test.GetTemplateManager();
                for (int i = 1; i < 6; i++)
                {
                    TemplateDetail template = mgr.Create(new TemplateDetail {
                        Name = "JamOn" + i.ToString(),
                        // Detail = "",
                        IsPublished = (i%2==1)
                    }).Result;
                }

                Template t = mgr.Link(new TemplateLink { TemplateId = 5, TopologyId = topo.Id}).Result;
                t = mgr.Unlink(new TemplateLink { TemplateId = t.Id }).Result;
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
