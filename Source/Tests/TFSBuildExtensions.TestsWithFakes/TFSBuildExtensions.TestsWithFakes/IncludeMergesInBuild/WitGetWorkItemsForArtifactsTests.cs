//-----------------------------------------------------------------------
// <copyright file="WitGetWorkItemsForArtifactsTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.QualityTools.Testing.Fakes;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild;
	using TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils;

	[TestClass]
	public class WitGetWorkItemsForArtifactsTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsWorkItems_Success()
		{
			using (var context = ShimsContext.Create())
			{
				var artifactUris = new List<ArtifactLink>()
				{
					new ArtifactLink() { Uri = new Uri("test://changeset/1"), AssociatedWorkItems = new List<int>() { 1, 2 } },
					new ArtifactLink() { Uri = new Uri("test://changeset/2"), AssociatedWorkItems = new List<int>() { 3 } },
					new ArtifactLink() { Uri = new Uri("test://changeset/3") },
				};
				var args = new Dictionary<string, object>()
				{
					{ "ArtifactUris", artifactUris.Select(a => a.Uri) },
					{ "AsOfDate", DateTime.MinValue },
					{ "WorkItemStore", FakeUtils.GetFakeWorkItemStore(artifactUris) },
				};
				var ids = WorkflowInvoker.Invoke(new WitGetWorkItemsForArtifacts(), args);
				Assert.AreEqual(3, ids.Count);
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsNothing_ForNoWorkItems()
		{
			using (var context = ShimsContext.Create())
			{
				var artifactUris = new List<ArtifactLink>()
				{
					new ArtifactLink() { Uri = new Uri("test://changeset/1") },
					new ArtifactLink() { Uri = new Uri("test://changeset/2") },
					new ArtifactLink() { Uri = new Uri("test://changeset/3") },
				};
				var args = new Dictionary<string, object>()
				{
					{ "ArtifactUris", artifactUris.Select(a => a.Uri) },
					{ "AsOfDate", DateTime.MinValue },
					{ "WorkItemStore", FakeUtils.GetFakeWorkItemStore(artifactUris) },
				};
				var ids = WorkflowInvoker.Invoke(new WitGetWorkItemsForArtifacts(), args);
				Assert.AreEqual(0, ids.Count);
			}
		}
	}
}
