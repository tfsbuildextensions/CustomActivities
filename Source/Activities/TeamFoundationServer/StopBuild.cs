//-----------------------------------------------------------------------
// <copyright file="StopBuild.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Will stop another build from running with a given build id.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class StopBuild : BaseCodeActivity<IQueuedBuild>
    {
        private IBuildServer bs;

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the BuildId to stop
        /// </summary>
        public InArgument<int> BuildId { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>IQueuedBuild</returns>
        protected override IQueuedBuild InternalExecute()
        {
            this.ConnectToTfs();
            IQueuedBuild build = this.HaltBuild();
            this.ActivityContext.SetValue(this.Result, build);
            return build;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
        }

        private IQueuedBuild HaltBuild()
        {
            IQueuedBuild buildToRetrieve = this.bs.GetQueuedBuild(this.BuildId.Get(this.ActivityContext), QueryOptions.None);

            switch (buildToRetrieve.Status)
            {
                case QueueStatus.Queued:
                case QueueStatus.Postponed:
                    buildToRetrieve.Cancel();
                    break;
                case QueueStatus.InProgress:
                    buildToRetrieve.Build.Stop();
                    break;
            }

            return buildToRetrieve;
        }
    }
}
