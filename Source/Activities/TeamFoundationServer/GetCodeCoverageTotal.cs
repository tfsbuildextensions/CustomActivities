//-----------------------------------------------------------------------
// <copyright file="GetCodeCoverageTotal.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;

    /// <summary>
    /// Activity to query the Test Manager service and get the total code coverage for a build.
    /// This task is heavily "time-dependent" since the code coverage results are calculates asynchronously on the TFS server.
    /// Therefore sometimes this activity returns 0 (since it runs before the results are calculated). Unfortunately there is
    /// no event to hook into, so you need to "sleep" a while before executing this task.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetCodeCoverageTotal : BaseCodeActivity<int>
    {
        /// <summary>
        /// The BuildDetail object for the build
        /// </summary>
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        /// <summary>
        /// Calculates the total code coverage for the build
        /// </summary>
        /// <returns>Code coverage total (percentage)</returns>
        protected override int InternalExecute()
        {
            var buildDetail = this.BuildDetail.Get(this.ActivityContext);
            var testService = buildDetail.BuildServer.TeamProjectCollection.GetService<ITestManagementService>();
            var project = testService.GetTeamProject(buildDetail.TeamProject);
            var runs = project.TestRuns.ByBuild(buildDetail.Uri);
            var covManager = project.CoverageAnalysisManager;

            var totalBlocksCovered = 0;
            var totalBlocksNotCovered = 0;

            foreach (var run in runs)
            {
                var coverageInfo = covManager.QueryTestRunCoverage(run.Id, CoverageQueryFlags.Modules);
                totalBlocksCovered += coverageInfo.Sum(c => c.Modules.Sum(m => m.Statistics.BlocksCovered));
                totalBlocksNotCovered += coverageInfo.Sum(c => c.Modules.Sum(m => m.Statistics.BlocksNotCovered));
            }

            var totalBlocks = totalBlocksCovered + totalBlocksNotCovered;
            if (totalBlocks == 0)
            {
                return 0;
            }

            return (int)(totalBlocksCovered * 100d / totalBlocks);
        }
    }
}
