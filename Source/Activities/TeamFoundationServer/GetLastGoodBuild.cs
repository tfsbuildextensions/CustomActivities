//-----------------------------------------------------------------------
// <copyright file="GetLastGoodBuild.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Activity based on CodeActivity
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "TODO: Need to resolve this."), BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class GetLastGoodBuild : BaseCodeActivity<IBuildDetail>
    {
        private TfsTeamProjectCollection mtfs;
        private IBuildServer bs;
        private IBuildDetail build;
        private string buildDefinition;
        private string teamProject;
        private string teamFoundationServer;
        private string buildQuality;

        /// <summary>
        /// The BuildDefinition to check
        /// </summary>
        public InArgument<string> BuildDefinition { get; set; }

        /// <summary>
        /// The TeamProject to connect to
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// The TeamFoundationServer to connect to.
        /// If parameter is empty then the server to which the build server is connected is used
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// The ParentBuild
        /// </summary>
        public InArgument<IBuildDetail> ParentBuild { get; set; }

        /// <summary>
        /// The build quality. If set only the last build with this
        /// build quality value will be returned (the comparison
        /// is case insensitive)
        /// </summary>
        public InArgument<string> BuildQuality { get; set; }

        /// <summary>
        /// Executes the workflow
        /// </summary>
        /// <returns>IBuildDetail</returns>
        protected override IBuildDetail InternalExecute()
        {
            this.buildDefinition = this.BuildDefinition.Get(this.ActivityContext);
            this.teamProject = this.TeamProject.Get(this.ActivityContext);
            this.teamFoundationServer = this.TeamFoundationServer.Get(this.ActivityContext);
            this.build = this.ParentBuild.Get(this.ActivityContext);
            this.buildQuality = this.BuildQuality.Get(this.ActivityContext);

            this.ConnectToTFS();
            this.build = this.GetGoodBuild() ?? this.build;
            this.Result.Set(this.ActivityContext, this.build);
            
            return this.build;
        }

        private void ConnectToTFS()
        {
            this.mtfs = string.IsNullOrEmpty(this.teamFoundationServer) ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.teamFoundationServer));
            this.bs = (IBuildServer)this.mtfs.GetService(typeof(IBuildServer));
        }

        private IBuildDetail GetGoodBuild()
        {
            IBuildDefinition buildDef = this.bs.GetBuildDefinition(this.teamProject, this.buildDefinition);
            IBuildDetailSpec buildDetailSpec = this.bs.CreateBuildDetailSpec(buildDef);
            buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            buildDetailSpec.Status = BuildStatus.Succeeded;
            if (!string.IsNullOrEmpty(this.buildQuality))
            {
                buildDetailSpec.Quality = this.buildQuality;
            }

            IBuildQueryResult builds = this.bs.QueryBuilds(buildDetailSpec);

            IBuildDetail latestBuild = builds.Builds.FirstOrDefault();

            return latestBuild;
        }
    }
}
