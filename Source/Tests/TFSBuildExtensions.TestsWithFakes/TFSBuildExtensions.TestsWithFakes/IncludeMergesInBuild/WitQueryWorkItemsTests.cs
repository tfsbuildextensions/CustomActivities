//-----------------------------------------------------------------------
// <copyright file="WitQueryWorkItemsTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.QualityTools.Testing.Fakes;
	using Microsoft.TeamFoundation.Client;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild;
	using TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils;

	[TestClass]
	public class WitQueryWorkItemsTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsWorkItemsAndParents_Success()
		{
			using (var context = ShimsContext.Create())
			{
				var workItems = new List<WorkItemInfo>()
				{
					new WorkItemInfo() { Id = 1 },
					new WorkItemInfo() { Id = 2, ParentId = 1 },
					new WorkItemInfo() { Id = 3, ParentId = 1 },
					new WorkItemInfo() { Id = 4 },
				};

				var args = new Dictionary<string, object>()
				{
					{ "Fields", new List<string>() { CoreFieldReferenceNames.History, UpdateWorkItem.IntegrationBuildFieldRef } },
					{ "Ids", new List<int>() { 2, 4 } },
					{ "IncludeParentWorkItems", true },
					{ "WorkItemStore", FakeUtils.GetFakeWorkItemStore(workItems) },
				};

				IDictionary<string, object> output;
				var workItemResults = WorkflowInvoker.Invoke(new WitQueryWorkItems(), args, out output, TimeSpan.FromMinutes(5));
				Assert.IsTrue(output.ContainsKey("ParentChildLinks"));
				Assert.AreEqual(3, ((IEnumerable<WorkItemLinkInfo>)output["ParentChildLinks"]).Count());
				Assert.AreEqual(3, workItemResults.Count);
				var items = workItemResults.Cast<WorkItem>().ToList();
				Assert.IsFalse(items.Any(i => i.Id == 3));
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsWorkItemsAndParents_SuccessWithNoParent()
		{
			using (var context = ShimsContext.Create())
			{
				var workItems = new List<WorkItemInfo>()
				{
					new WorkItemInfo() { Id = 1 },
					new WorkItemInfo() { Id = 2, ParentId = 1 },
					new WorkItemInfo() { Id = 3, ParentId = 1 },
					new WorkItemInfo() { Id = 4 },
				};

				var args = new Dictionary<string, object>()
				{
					{ "Fields", new List<string>() { CoreFieldReferenceNames.History, UpdateWorkItem.IntegrationBuildFieldRef } },
					{ "Ids", new List<int>() { 4 } },
					{ "IncludeParentWorkItems", true },
					{ "WorkItemStore", FakeUtils.GetFakeWorkItemStore(workItems) },
				};

				IDictionary<string, object> output;
				var workItemResults = WorkflowInvoker.Invoke(new WitQueryWorkItems(), args, out output, TimeSpan.FromMinutes(5));
				Assert.IsTrue(output.ContainsKey("ParentChildLinks"));
				Assert.AreEqual(1, ((IEnumerable<WorkItemLinkInfo>)output["ParentChildLinks"]).Count());
				Assert.AreEqual(1, workItemResults.Count);
				var items = workItemResults.Cast<WorkItem>().ToList();
				Assert.IsTrue(items.Any(i => i.Id == 4));
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsWorkItemsOnly_WhenNoParentsSpecified()
		{
			using (var context = ShimsContext.Create())
			{
				var workItems = new List<WorkItemInfo>()
				{
					new WorkItemInfo() { Id = 1 },
					new WorkItemInfo() { Id = 2, ParentId = 1 },
					new WorkItemInfo() { Id = 3, ParentId = 1 },
					new WorkItemInfo() { Id = 4 },
				};

				var args = new Dictionary<string, object>()
				{
					{ "Fields", new List<string>() { CoreFieldReferenceNames.History, UpdateWorkItem.IntegrationBuildFieldRef } },
					{ "Ids", new List<int>() { 2, 4 } },
					{ "IncludeParentWorkItems", false },
					{ "WorkItemStore", FakeUtils.GetFakeWorkItemStore(workItems) },
				};

				IDictionary<string, object> output;
				var workItemResults = WorkflowInvoker.Invoke(new WitQueryWorkItems(), args, out output, TimeSpan.FromMinutes(5));
				Assert.IsTrue(output.ContainsKey("ParentChildLinks"));
				Assert.AreEqual(0, ((IEnumerable<WorkItemLinkInfo>)output["ParentChildLinks"]).Count());
				Assert.AreEqual(2, workItemResults.Count);
				var items = workItemResults.Cast<WorkItem>().ToList();
				Assert.IsTrue(items.Any(i => i.Id == 2));
				Assert.IsTrue(items.Any(i => i.Id == 4));
			}
		}

		[TestMethod]
		[TestCategory("ConnectedTests")]
		[Ignore]
		public void Test_WitQueryWorkItems_GetsWorkItemsAndParents_AgainstLiveServer()
		{
			// work items for this:
			// User Story 36
			//   -> Task 37
			//   -> Task 38
			//   -> Task 39
			//       -> Task 41
			// Task 40
			var args = new Dictionary<string, object>()
			{
				{ "Fields", new List<string>() { CoreFieldReferenceNames.History, UpdateWorkItem.IntegrationBuildFieldRef } },
				{ "Ids", new List<int>() { 38, 40, 41 } },
				{ "IncludeParentWorkItems", true },
				{ "WorkItemStore", TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("http://localhost:8080/tfs/defaultcollection")).GetService<WorkItemStore>() },
			};

			IDictionary<string, object> output;
			var workItemResults = WorkflowInvoker.Invoke(new WitQueryWorkItems(), args, out output, TimeSpan.FromMinutes(5));
			Assert.IsTrue(output.ContainsKey("ParentChildLinks"));
			Assert.AreEqual(4, ((IEnumerable<WorkItemLinkInfo>)output["ParentChildLinks"]).Count());
			Assert.AreEqual(4, workItemResults.Count);
			var items = workItemResults.Cast<WorkItem>().ToList();
			Assert.IsTrue(items.Any(i => i.Id == 36));
			Assert.IsTrue(items.Any(i => i.Id == 38));
			Assert.IsTrue(items.Any(i => i.Id == 40));
		}
	}
}
