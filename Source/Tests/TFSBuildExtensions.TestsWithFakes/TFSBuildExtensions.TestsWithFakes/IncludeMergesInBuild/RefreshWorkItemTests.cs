//-----------------------------------------------------------------------
// <copyright file="RefreshWorkItemTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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

	[TestClass]
	public class RefreshWorkItemTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_SyncIsCalled()
		{
			using (var context = ShimsContext.Create())
			{
				bool syncCalled = false;
				var workItem = new ShimWorkItem()
					{
						SyncToLatest = () => { syncCalled = true; }
					};
				var args = new Dictionary<string, object>()	{
						{ "WorkItem", (WorkItem)workItem }
					};
				WorkflowInvoker.Invoke(new RefreshWorkItem(), args);

				Assert.IsTrue(syncCalled);
			}
		}
	}
}
