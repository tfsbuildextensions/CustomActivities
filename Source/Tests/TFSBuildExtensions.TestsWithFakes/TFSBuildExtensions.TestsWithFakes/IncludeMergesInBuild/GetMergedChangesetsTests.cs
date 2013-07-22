//-----------------------------------------------------------------------
// <copyright file="GetMergedChangesetsTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.QualityTools.Testing.Fakes;
	using Microsoft.TeamFoundation.Client;
	using Microsoft.TeamFoundation.VersionControl.Client;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild;
	using TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils;

	[TestClass]
	public class GetMergedChangesetsTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsChangeset_When_MergesExist_1Level()
		{
			using (var context = ShimsContext.Create())
			{
				var targetChangeset = FakeUtils.CreateChangeset(2, new List<int>() { 1 });

				var changesets = new List<ChangesetInfo>()
				{
					FakeUtils.CreateChangeset(1),
					targetChangeset
				};

				var args = new Dictionary<string, object>()
				{
					{ "AssociatedChangesets", new List<Changeset>() { targetChangeset.ToChangeset() } },
					{ "VersionControlServer", FakeUtils.GetFakeVCServer(changesets) }
				};

				var merges = WorkflowInvoker.Invoke(new GetMergedChangesets(), args);
				Assert.AreEqual(1, merges.Count());
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_GetsChangeset_When_MergesExist_2Levels()
		{
			using (var context = ShimsContext.Create())
			{
				var targetChangeset = FakeUtils.CreateChangeset(4, new List<int>() { 3 });

				var changesets = new List<ChangesetInfo>()
				{
					FakeUtils.CreateChangeset(1),
					FakeUtils.CreateChangeset(2),
					FakeUtils.CreateChangeset(3, new List<int>() { 1, 2 }),
					targetChangeset
				};

				var args = new Dictionary<string, object>()
				{
					{ "AssociatedChangesets", new List<Changeset>() { targetChangeset.ToChangeset() } },
					{ "VersionControlServer", FakeUtils.GetFakeVCServer(changesets) }
				};

				var merges = WorkflowInvoker.Invoke(new GetMergedChangesets(), args);
				Assert.AreEqual(3, merges.Count());
			}
		}

		[TestMethod]
		[TestCategory("ConnectedTests")]
		[Ignore]
		public void Test_GetsChangeset_ForLiveServer()
		{
			var server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("http://localhost:8080/tfs/defaultcollection")).GetService<VersionControlServer>();
			var changeset = server.GetChangeset(100); // 73 -> 3 changes

			var args = new Dictionary<string, object>()
			{
				{ "AssociatedChangesets", new List<Changeset>() { changeset } },
				{ "VersionControlServer", server }
			};

			var merges = WorkflowInvoker.Invoke(new GetMergedChangesets(), args);
			Assert.AreEqual(3, merges.Count());
		}
	}
}
