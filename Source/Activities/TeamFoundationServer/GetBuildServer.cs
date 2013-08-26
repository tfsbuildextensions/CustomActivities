//-----------------------------------------------------------------------
// <copyright file="GetBuildServer.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Workflow activity that gets an <see cref="Microsoft.TeamFoundation.Build.Client.IBuildServer"/>
    /// based on the Team Foundation Server URL that is provided.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetBuildServer : BaseCodeActivity<IBuildServer>
    {
        /// <summary>
        /// The URL for the Team Foundation Server to use.
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> TeamFoundationServerUrl { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity.
        /// </summary>
        /// <returns>The <see cref="Microsoft.TeamFoundation.Build.Client.IBuildServer"/>
        /// that is specified.</returns>
        protected override IBuildServer InternalExecute()
        {
            string serverUrl = this.TeamFoundationServerUrl.Get(this.ActivityContext);

            using (TfsTeamProjectCollection tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(serverUrl)))
            {
                return (IBuildServer)tfs.GetService<IBuildServer>();
            }
        }
    }
}
