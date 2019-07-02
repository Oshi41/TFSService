using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Windows.Documents;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.TFS;
using TfsAPI.TFS.Build_Defenitions;

namespace Tests
{
    [TestClass]
    public class TfsTests
    {
        [TestMethod]
        public void GetCapacity()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var cap = tfs.GetCapacity();
            }
        }

        //[TestMethod]
        //public void GetLatestTask()
        //{
        //    var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
        //    {
        //        var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        //    }
        //}

        //[TestMethod]
        //public void TryWriteHour()
        //{
        //    var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
        //    {
        //        var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();

        //        tfs.WriteHours(latest, 1);
        //    }
        //}

        [TestMethod]
        public void GetMyQuarries()
        {
            // подключаемся к ТФС
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(
                new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            // Выбираем проект
            var project = tfs.GetService<ITestManagementService>().GetTeamProject("SNES");

            var quarries = string.Empty;

            foreach (var query in project.Queries) quarries += $"{query.Name}\n{query.QueryText}\n\n";

            Trace.WriteLine(quarries);
        }

        [TestMethod]
        public void GetLinkedItems()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var id = 40952;

                var items = tfs.GetAssociateItems(id);
            }
        }

        [TestMethod]
        public void GetMyItems()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var all = tfs.GetMyWorkItems();

                Assert.IsTrue(all.Any());
            }
        }

        [TestMethod]
        public void GetMyName()
        {
            var name = "Щеглов Аркадий";

            using (var project =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security")))
            {
                Assert.AreEqual(name, project.AuthorizedIdentity.DisplayName);
            }
        }

        [TestMethod]
        public void GetAllLinkTypes()
        {
            using (var project =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security")))
            {
                var store = project.GetService<WorkItemStore>();

                var childType = store.WorkItemLinkTypes.Select(x => x.ReferenceName).ToList();
            }
        }

        [TestMethod]
        public void GetTaskLinks()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var items = tfs.GetMyWorkItems().Where(x => x.Type.Name.Equals(WorkItemTypes.Task)).ToList();

                foreach (var task in items)
                {
                    var name = task.Title;

                    foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.Link link in task.Links)
                    {
                    }
                }
            }
        }

        [TestMethod]
        public void CheckMyWriteOff()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var from = DateTime.Today;

                var checkins = tfs.GetWriteoffs(from, from.Add(TimeSpan.FromDays(1)));

                Trace.WriteLine($"Today was - {checkins.Sum(x => x.Value)}");

                from = from.Add(TimeSpan.FromDays(-1));

                checkins = tfs.GetWriteoffs(from, from.Add(TimeSpan.FromDays(1)));

                Trace.WriteLine($"Yesterday was - {checkins.Sum(x => x.Value)}");
            }
        }

        [TestMethod]
        public void GetHours5()
        {
            var date = new DateTime(2019, 5, 6);
            var hours = 13;
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var checkins = tfs.GetWriteoffs(date.AddDays(-1), date.AddDays(1));

            Assert.AreEqual(checkins.Select(x => x.Value).Sum(), hours);
        }

        [TestMethod]
        public void GetHours21()
        {
            var date = new DateTime(2019, 5, 21);
            var hours = 14;
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var checkins = tfs.GetWriteoffs(date, date);

            Assert.AreEqual(checkins.Select(x => x.Value).Sum(), hours);
        }

        [TestMethod]
        public void TestCollectionGuid()
        {
            var tfs =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            var workItemStore = tfs.GetService<WorkItemStore>();
            var project = workItemStore.Projects["SNES"];

            var etalon = Guid.Parse("4274f126-955e-4fc8-97a3-cf301f69679c");
            Assert.AreEqual(etalon, project.Guid);
        }

        [TestMethod]
        public void TestTeamGuid()
        {
            var etalon = Guid.Parse("750f400e-c39b-4317-a660-9f62f528ef5d");

            var tfs =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            var workItemStore = tfs.GetService<WorkItemStore>();
            var teamService = tfs.GetService<TfsTeamService>();

            var project = workItemStore.Projects["SNES"];
            var teams = teamService.QueryTeams(project.Uri.ToString()).ToList();

            foreach (var team in teams)
            {
                if (team.Identity.TeamFoundationId == etalon)
                {
                    return;
                }
            }

            Assert.Fail("Cannot find team");
        }

        [TestMethod]
        public void GetIterationCapacity()
        {
            var tfs =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            var searcher = new CapacitySearcher(tfs);

            var cap = searcher.SearchActualCapacity(tfs.AuthorizedIdentity.DisplayName);

            //var workItemStore = tfs.GetService<WorkItemStore>();
            //var teamService = tfs.GetService<TfsTeamService>();
            //var css = tfs.GetService<ICommonStructureService>();
            //var ims = tfs.GetService<IIdentityManagementService2>();
            //var client = tfs.GetClient<WorkHttpClient>();


            ////var allMineGroups = workItemStore
            ////    .Projects
            ////    .OfType<Project>()
            ////    .SelectMany(x => ims.ListApplicationGroups(x.Uri.ToString(), ReadIdentityOptions.ExtendedProperties))
            ////    .Where(x => ims.IsMember(x.Descriptor, tfs.AuthorizedIdentity.Descriptor))
            ////    .ToList();


            //var project = workItemStore.Projects["SNES"];

            //var iter = client.GetTeamIterationsAsync(new TeamContext(project.Guid), "current").Result;

            //var guid = iter[0].Id;


            //var project = workItemStore.Projects["SNES"];

            ////var mineGroups = teamService
            ////    .QueryTeams(project.Uri.ToString())
            ////    .Where(x => ims.IsMember(x.Identity.Descriptor, tfs.AuthorizedIdentity.Descriptor))
            ////    .ToList();

            //var team = teamService.QueryTeams(project.Uri.ToString()).FirstOrDefault();

            //var node = css
            //           .ListStructures(project.Uri.ToString())
            //           .Single(item => item.StructureType == "ProjectLifecycle");

            //var iter = project
            //    .IterationRootNodes
            //    .OfType<Node>()
            //    .Select(x => css.GetNodeFromPath($"{node.Path}\\{x.Name}"))
            //    .Where(x => x.IsCurrent())
            //    .FirstOrDefault();


            //var request = $"{tfs.Uri}/{project.Store.TeamProjectCollection.SessionId}/{project.Id}/{team.Identity.Descriptor.Identifier}/_apis/work/teamsettings/iterations/{iter.Name}";


            //var allProjects = workItemStore.Projects.OfType<Project>().ToList();

            //foreach (var proj in allProjects)
            //{
            //    try
            //    {
            //        var teams = GetMyTeams(teamService, proj);



            //        
            //        {
            //            Console.WriteLine(sprint.Name);
            //            
            //            Console.WriteLine(iteration.StartDate);
            //            Console.WriteLine(iteration.FinishDate);

            //            if (iteration.IsCurrent())
            //            {




            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        continue;
            //    }

            //}


            // var map = GetProjectsWithSettings(tfs.GetService<ProjectHttpClient>(), tfs.GetService<TeamSettingsConfigurationService>());

            //IIdentityManagementService ims = tfs.GetService<IIdentityManagementService>();

            //var identity = ims.ReadIdentity(IdentitySearchFactor.DisplayName, tfs.AuthorizedIdentity.DisplayName, MembershipQuery.Direct, ReadIdentityOptions.None);

            //WorkItemTrackingHttpClient workItemTrackingClient = tfs.GetClient<WorkItemTrackingHttpClient>();

            //WorkItemClassificationNode result = workItemTrackingClient.GetClassificationNodeAsync(
            //    "SNES",
            //    TreeStructureGroup.Iterations,
            //    null,
            //    4).Result;



            //
            //var teamSettings = tfs.GetService<>();

            //var projects = css.ListAllProjects();

            //TeamContext context = null;
            //string iterID = null;

            //var client = tfs.GetClient<WorkHttpClient>();

            //var iters = client.GetTeamIterationsAsync(context, "current").Result;



            //var iteration = client.Пуе(context).Result;

        }

        [TestMethod]
        public void TestJson()
        {
            var file = @"C:\Users\a.sheglov\Desktop\example.txt";
            var json = File.ReadAllText(file);

            var result = CapacitySearcher.Parse(json);
        }

        [TestMethod]
        public void TestIterations()
        {
            var tfs =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            var store = tfs.GetService<WorkItemStore>();
            var structure = tfs.GetService<ICommonStructureService4>();
            var client = tfs.GetClient<WorkHttpClient>();

            var project = store.Projects["SNES"];

            var iterations = client.GetTeamIterationsAsync(new TeamContext(project.Name)).Result;

            var iterations2 = project
                              .IterationRootNodes
                              .OfType<Node>()
                              .Select(x => structure.GetNode(x.Uri.AbsoluteUri))
                              .AsParallel()
                              .ToList();

            var same = iterations.Count == iterations2.Count;



            //var iters = 
            //        .Where(x => x.InRange(start, end))
            //        .Select(x => new TeamSettingsIteration()
            //        {
            //            Attributes = new TeamIterationAttributes
            //            {
            //                FinishDate = x.FinishDate,
            //                StartDate = x.StartDate,
            //            },
            //            Path = x.Path,
            //            Name = x.Name,
            //            Url = x.Uri,

            //        })
            //        .AsParallel()
            //        .ToList();
        }

        [TestMethod]
        public void LinkItems()
        {
            var tfs =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            var store = tfs.GetService<WorkItemStore>();

            var task = store.GetWorkItem(81046);
            var pbi = store.GetWorkItem(78218);

            var link = store.WorkItemLinkTypes[WorkItems.LinkTypes.ParentLink];

            task.Links.Add(new RelatedLink(link.ReverseEnd, pbi.Id));
            task.Save();
        }

        [TestMethod]
        public void TestBuild()
        {
            var tfs =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            var client = tfs.GetClient<BuildHttpClient>();
            var buildServer = tfs.GetService<IBuildServer>();
            var store = tfs.GetService<WorkItemStore>();

            var project = store.Projects["SNES"];

            var observer = new BuildQueueObserver(tfs.AuthorizedIdentity.DisplayName, client, buildServer, project.Guid);

            observer.FindBuilds(finish: DateTime.Now);

        }



        private TeamSettingsIteration GetCurrentIteration(Project project, TeamFoundationIdentity teamId, WorkHttpClient client)
        {
            var iterations = client.GetTeamIterationsAsync(new TeamContext(project.Name)).Result;
            return iterations.FirstOrDefault(x => x.IsCurrent());
        }

        private List<TeamFoundationTeam> GetMyTeams(TfsTeamService teamService, Project project)
        {
            try
            {
                var properties = new List<string>();

                var teamGroups = teamService.QueryTeams(project.Uri.ToString());

                var allTeams = teamGroups
                               .Select(x => teamService.ReadTeam(x.Identity.Descriptor, properties))
                               .AsParallel()
                               .ToList();

                return allTeams;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return new List<TeamFoundationTeam>();
            }
        }

        private Dictionary<TeamProjectReference, List<TeamConfiguration>> GetProjectsWithSettings(ProjectHttpClient css, TeamSettingsConfigurationService team)
        {
            var result = new Dictionary<TeamProjectReference, List<TeamConfiguration>>();

            var projects = css.GetProjects(null).Result;

            foreach (var project in projects)
            {
                try
                {
                    var settings = team
                        .GetTeamConfigurationsForUser(new[] { project.Url })
                        .ToList();

                    if (!settings.IsNullOrEmpty())
                    {
                        result[project] = settings;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return result;
        }
    }
}