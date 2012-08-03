//-----------------------------------------------------------------------
// <copyright file="GetBuildDetail.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Retrieves IBuildDetail object for completed build
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetBuildDetail : BaseCodeActivity<IBuildDetail>
    {
        private IBuildServer bs;

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the BuildURI to retrieve
        /// </summary>
        [RequiredArgument]
        public InArgument<Uri> BuildUri { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>IBuildDetail</returns>
        protected override IBuildDetail InternalExecute()
        {
            this.ConnectToTfs();
            IBuildDetail build = this.RetrieveBuild();
            this.ActivityContext.SetValue(this.Result, build);
            return build;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
        }

        private IBuildDetail RetrieveBuild()
        {
            IBuildDetail buildToRetrieve = this.bs.GetBuild(this.BuildUri.Get(this.ActivityContext));
            if (buildToRetrieve == null)
            {
                this.LogBuildError("Build: " + this.BuildUri.Get(this.ActivityContext).AbsoluteUri + " does not exist.");
            }

            return buildToRetrieve;
        }
    }
}
