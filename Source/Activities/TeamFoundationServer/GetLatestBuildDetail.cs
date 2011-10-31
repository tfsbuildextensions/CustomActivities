//-----------------------------------------------------------------------
// <copyright file="GetLatestBuildDetail.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the most recent build for a build definition.
    /// </summary>
    [System.ComponentModel.Description("Activity to get the most build for a build definition.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetLatestBuildDetail : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the build server to use.
        /// </summary>
        public InArgument<IBuildServer> BuildServer { get; set; }

        /// <summary>
        /// Gets or sets the name of the team project to get details for.
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// Gets or sets the name of the build to get details for.
        /// </summary>
        public InArgument<string> BuildName { get; set; }

        /// <summary>
        /// Gets or sets the build detail.
        /// </summary>
        public OutArgument<IBuildDetail> BuildDetail { get; set; }

        /// <summary>
        /// Get the label details.
        /// </summary>
        protected override void InternalExecute()
        {
            var buildServer = this.BuildServer.Get(this.ActivityContext);
            var teamProject = this.TeamProject.Get(this.ActivityContext);
            var buildName = this.BuildName.Get(this.ActivityContext);

            // Create a build spec to find the latest build
            IBuildDetailSpec buildDetailSpec = buildServer.CreateBuildDetailSpec(teamProject, buildName);
            buildDetailSpec.MaxBuildsPerDefinition = 1;
            buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            // Query the build server for the latest build
            IBuildQueryResult results = buildServer.QueryBuilds(buildDetailSpec);

            // Return the build
            this.BuildDetail.Set(this.ActivityContext, results.Builds[0]);
        }
    }
}
