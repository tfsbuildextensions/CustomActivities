//-----------------------------------------------------------------------
// <copyright file="GetLastGoodBuild.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Retrieves the last good completed build (IBuildDetail object), that was launched last, where the drop location exists, and where the build status is "Succeeded"
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetLastGoodBuild : BaseCodeActivity<IBuildDetail>
    {
        private IBuildServer bs;
        private IBuildDetail build;

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the BuildDefinition to query
        /// </summary>
        public InArgument<string> BuildDefinition { get; set; }

        /// <summary>
        /// TODO: document this
        /// </summary>
        public InArgument<IBuildDetail> ParentBuild { get; set; }

        /// <summary>
        /// Specifies the TeamProject. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>IBuildDetail</returns>
        protected override IBuildDetail InternalExecute()
        {
            this.build = this.ParentBuild.Get(this.ActivityContext);

            this.ConnectToTfs();
            this.build = this.GetGoodBuild() ?? this.build;            
            this.ActivityContext.SetValue(Result, this.build);
            return this.build;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
        }

        private IBuildDetail GetGoodBuild()
        {
            if (this.TeamProject.Expression == null)
            {
                var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                this.TeamProject.Set(this.ActivityContext, buildDetail.TeamProject);
            }

            IBuildDefinition buildDefinition = this.bs.GetBuildDefinition(this.TeamProject.Get(this.ActivityContext), this.BuildDefinition.Get(this.ActivityContext));
            IBuildDetail[] builds = this.bs.QueryBuilds(buildDefinition);
            IBuildDetail latestBuild = null;

            foreach (IBuildDetail b in builds)
            {
                if (latestBuild == null)
                {
                    latestBuild = b;
                }

                if (b.Status == BuildStatus.Succeeded && Directory.Exists(b.DropLocation))
                {
                    if (b.StartTime > latestBuild.StartTime)
                    {
                        latestBuild = b;
                    }
                }
            }

            return latestBuild;
        }
    }
}
