//-----------------------------------------------------------------------
// <copyright file="GetParentIdTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
	public class GetParentIdTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_ReturnsParent_IfExists()
		{
			using (var context = ShimsContext.Create())
			{
				var args = new Dictionary<string, object>()	{
					{ "WorkItem", GetFakeWorkItem() },
					{ "ParentChildLinks", GetFakeLinks(1, 2) }
				};

				var result = WorkflowInvoker.Invoke(new GetParentId(), args);
				Assert.IsNotNull(result);
				Assert.AreEqual(1, result);
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_Returns0_IfNoParentExists()
		{
			using (var context = ShimsContext.Create())
			{
				var args = new Dictionary<string, object>()	{
					{ "WorkItem", GetFakeWorkItem() },
					{ "ParentChildLinks", GetFakeLinks(2, 99) }
				};

				var result = WorkflowInvoker.Invoke(new GetParentId(), args);
				Assert.AreEqual(0, result);
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_Returns0_IfParentLinksIsNull()
		{
			using (var context = ShimsContext.Create())
			{
				var args = new Dictionary<string, object>() {
					{ "WorkItem", GetFakeWorkItem() },
					{ "ParentChildLinks", null }
				};

				var result = WorkflowInvoker.Invoke(new GetParentId(), args);
				Assert.AreEqual(0, result);
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_Returns0_IfParentLinksIsEmpty()
		{
			using (var context = ShimsContext.Create())
			{
				var args = new Dictionary<string, object>() {
					{ "WorkItem", GetFakeWorkItem() },
					{ "ParentChildLinks", new List<WorkItemLinkInfo>() }
				};

				var result = WorkflowInvoker.Invoke(new GetParentId(), args);
				Assert.AreEqual(0, result);
			}
		}
		
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_Returns0_IfWorkItemIsNull()
		{
			var args = new Dictionary<string, object>() {
				{ "WorkItem", null },
				{ "ParentChildLinks", null }
			};

			var result = WorkflowInvoker.Invoke(new GetParentId(), args);
			Assert.AreEqual(0, result);
		}

		private WorkItem GetFakeWorkItem()
		{
			var wi = new ShimWorkItem()
				{
					IdGet = () => 2
				};
			return wi;
		}

		private IEnumerable<WorkItemLinkInfo> GetFakeLinks(int sourceId, int targetId)
		{
			var list = new List<WorkItemLinkInfo>()
			{
				new WorkItemLinkInfo() { LinkTypeId = 1, SourceId = sourceId, TargetId = targetId }
			};

			return list;
		}
	}
}
