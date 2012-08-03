//-----------------------------------------------------------------------
// <copyright file="IsBuildRunning.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Checks to see if a build is currently running either by the BuildDefinition or BuildId
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class IsBuildRunning : BaseCodeActivity<bool>
    {
        private IBuildServer bs;

        /// <summary>
        /// Specifies the BuildDefinition to query
        /// </summary>
        public InArgument<string> BuildDefinition { get; set; }
        
        /// <summary>
        /// Specifies the TeamProject. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the BuildId to query
        /// </summary>
        public InArgument<int> BuildId { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>bool</returns>
        protected override bool InternalExecute()
        {
            this.ConnectToTfs();
            bool isBuildRunning = this.BuildId.Get(this.ActivityContext) == 0 ? this.CheckDefinition() : this.CheckBuildId();
            this.ActivityContext.SetValue(this.Result, isBuildRunning);
            return isBuildRunning;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
        }

        private bool CheckDefinition()
        {
            if (this.TeamProject.Expression == null)
            {
                var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                this.TeamProject.Set(this.ActivityContext, buildDetail.TeamProject);
            }

            IQueuedBuildSpec buildSpec = this.bs.CreateBuildQueueSpec(this.TeamProject.Get(this.ActivityContext), this.BuildDefinition.Get(this.ActivityContext));
            IQueuedBuildQueryResult builds = this.bs.QueryQueuedBuilds(buildSpec);

            return builds.QueuedBuilds.Any(this.IsQueuedBuildRunning);
        }

        private bool CheckBuildId()
        {
            IQueuedBuild queuedBuild = this.bs.GetQueuedBuild(this.BuildId.Get(this.ActivityContext), QueryOptions.All);
            return this.IsQueuedBuildRunning(queuedBuild);
        }

        private bool IsQueuedBuildRunning(IQueuedBuild build)
        {
            bool isQueuedBuildRunning = false;
            DateTime timeStampIfBuildIsRunning = new DateTime();
            if (build != null)
            {
                if (build.Status == QueueStatus.Queued || build.Status == QueueStatus.InProgress || (build.Build != null && (build.Build.Status == BuildStatus.InProgress && (build.Build.FinishTime == timeStampIfBuildIsRunning || build.Build.Status == BuildStatus.NotStarted))))
                {
                    isQueuedBuildRunning = true;
                }
            }

            return isQueuedBuildRunning;
        }
    }
}
