//-----------------------------------------------------------------------
// <copyright file="GetCodeCoverageTotalTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using Microsoft.QualityTools.Testing.Fakes;
	using Microsoft.TeamFoundation.Build.Client;
	using Microsoft.TeamFoundation.Build.Client.Fakes;
	using Microsoft.TeamFoundation.Client.Fakes;
	using Microsoft.TeamFoundation.TestManagement.Client;
	using Microsoft.TeamFoundation.TestManagement.Client.Fakes;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TfsBuildExtensions.Activities.TeamFoundationServer;

	[TestClass]
	public class GetCodeCoverageTotalTests
	{
		[TestMethod]
		[TestCategory("GetCodeCoverage")]
		public void Test_GetsZeroWhenNoCoverage()
		{
			using (var context = ShimsContext.Create())
			{
				var args = new Dictionary<string, object>()	{
					{ "BuildDetail", GetFakeBuildDetail(true) }
				};

				var result = WorkflowInvoker.Invoke(new GetCodeCoverageTotal(), args);
				Assert.IsNotNull(result);
				Assert.AreEqual(0, result);
			}
		}

		[TestMethod]
		[TestCategory("GetCodeCoverage")]
		public void Test_GetsCorrectWhenCoverage()
		{
			using (var context = ShimsContext.Create())
			{
				var args = new Dictionary<string, object>()	{
					{ "BuildDetail", GetFakeBuildDetail() }
				};

				var result = WorkflowInvoker.Invoke(new GetCodeCoverageTotal(), args);
				Assert.IsNotNull(result);
				Assert.AreEqual(71, result);
			}
		}

		private IBuildDetail GetFakeBuildDetail(bool zero = false)
		{
			var fakeRuns = new List<ITestRun>()
			{
				new StubITestRun() { IdGet = () => 1 },
				new StubITestRun() { IdGet = () => 2 }
			};
			if (zero)
			{
				fakeRuns.RemoveAt(0);
			}

			var fakeTpc = new ShimTfsTeamProjectCollection();
			var fakeCon = new ShimTfsConnection(fakeTpc);
			fakeCon.GetServiceOf1<ITestManagementService>(() => new StubITestManagementService()
			{
				GetTeamProjectString = (s) => new StubITestManagementTeamProject()
				{
					TestRunsGet = () => new StubITestRunHelper()
					{
						ByBuildUri = (u) => fakeRuns
					},
					CoverageAnalysisManagerGet = () => new StubICoverageAnalysisManager()
					{
						QueryTestRunCoverageInt32CoverageQueryFlags = (id, f) =>
						{
							if (id == 1)
							{
								return new List<ITestRunCoverage>()
								{
									new StubITestRunCoverage()
									{
										ModulesGet = () => new List<IModuleCoverage>()
										{
											new StubIModuleCoverage() 
											{
												StatisticsGet = () => new StubICoverageStatistics() 
												{
													BlocksCoveredGet = () => 6,
													BlocksNotCoveredGet = () => 0
												}
											},
											new StubIModuleCoverage() 
											{
												StatisticsGet = () => new StubICoverageStatistics() 
												{
													BlocksCoveredGet = () => 4,
													BlocksNotCoveredGet = () => 4
												}
											},
										}.ToArray()
									}
								}.ToArray();
							}
							return new List<ITestRunCoverage>().ToArray();
						}
					}
				},
			});

			var detail = new StubIBuildDetail()
			{
				BuildServerGet = () => new StubIBuildServer()
				{
					TeamProjectCollectionGet = () => fakeTpc
				},
				UriGet = () => new Uri("http://test/Build"),
				TeamProjectGet = () => "Test"
			};
			return detail;
		}
	}
}
