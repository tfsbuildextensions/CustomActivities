//-----------------------------------------------------------------------
// <copyright file="GetBuildController.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Workflow activity that gets an <see cref="Microsoft.TeamFoundation.Build.Client.IBuildController"/>
    /// based on the Build Server and controller name provided.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetBuildController : BaseCodeActivity<IBuildController>
    {
        /// <summary>
        /// The build controller's name that exists on the TFS server.
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> BuildControllerName { get; set; }

        /// <summary>
        /// The <see cref="Microsoft.TeamFoundation.Build.Client.IBuildServer"/>
        /// object for the Team Foundation Server and Team
        /// Project Collection to use that contains the build controller.
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<IBuildServer> BuildServer { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity.
        /// </summary>
        /// <returns>The <see cref="Microsoft.TeamFoundation.Build.Client.IBuildController"/>
        /// that is specified.</returns>
        protected override IBuildController InternalExecute()
        {
            IBuildServer buildServer = this.BuildServer.Get(this.ActivityContext);
            string buildControllerName = this.BuildControllerName.Get(this.ActivityContext);

            return buildServer.GetBuildController(buildControllerName);
        }
    }
}