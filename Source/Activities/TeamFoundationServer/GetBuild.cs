//-----------------------------------------------------------------------
// <copyright file="GetBuild.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Retrieves IQueuedBuild object for build that is currently running
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetBuild : BaseCodeActivity<IQueuedBuild>
    {
        private IBuildServer bs;

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the BuildId to get
        /// </summary>
        public InArgument<int> BuildId { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>IQueuedBuild</returns>
        protected override IQueuedBuild InternalExecute()
        {
            this.ConnectToTfs();
            IQueuedBuild build = this.RetrieveBuild();
            this.ActivityContext.SetValue(this.Result, build);
            return build;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
        }

        private IQueuedBuild RetrieveBuild()
        {
            IQueuedBuild buildToRetrieve = this.bs.GetQueuedBuild(this.BuildId.Get(this.ActivityContext), QueryOptions.All);
            if (buildToRetrieve == null)
            {
                this.LogBuildError("Queued build: " + this.BuildId.Get(this.ActivityContext) + " does not exist.");
            }

            return buildToRetrieve;
        }
    }
}
