//-----------------------------------------------------------------------
// <copyright file="WitBatchSaveWorkItemsTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
	using System.Activities;
	using System.Collections.Generic;
	using Microsoft.QualityTools.Testing.Fakes;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using Microsoft.TeamFoundation.WorkItemTracking.Client.Fakes;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild;
	using TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils;

	[TestClass]
	public class WitBatchSaveWorkItemsTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_Works_WhenNoErrors()
		{
			using (var context = ShimsContext.Create())
			{
				var savedIds = new List<int>();
				var store = FakeUtils.GetFakeWorkItemStore(savedIds);
				var workItems = new List<WorkItem>()
				{
					new ShimWorkItem() { IdGet = () => 1, StoreGet = () => store },
					new ShimWorkItem() { IdGet = () => 2, StoreGet = () => store }
				};

				var args = new Dictionary<string, object>()
				{
					{ "WorkItems", workItems }
				};
				var errs = WorkflowInvoker.Invoke(new WitBatchSaveWorkItems(), args);
				Assert.AreEqual(0, errs.Count);
				Assert.AreEqual(2, savedIds.Count);
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_Works_WhenErrors()
		{
			using (var context = ShimsContext.Create())
			{
				var savedIds = new List<int>();
				var store = FakeUtils.GetFakeWorkItemStore(savedIds);
				var workItems = new List<WorkItem>()
				{
					new ShimWorkItem() { IdGet = () => -1, StoreGet = () => store },
					new ShimWorkItem() { IdGet = () => 2, StoreGet = () => store }
				};

				var args = new Dictionary<string, object>()
				{
					{ "WorkItems", workItems }
				};
				var errs = WorkflowInvoker.Invoke(new WitBatchSaveWorkItems(), args);
				Assert.AreEqual(1, errs.Count);
				Assert.AreEqual(0, savedIds.Count);
			}
		}
	}
}
