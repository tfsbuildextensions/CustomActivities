//-----------------------------------------------------------------------
// <copyright file="GetLatestChangeset.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Get the most recent changeset.
    /// </summary>
    [System.ComponentModel.Description("Activity to get the most recent changeset.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetLatestChangeset : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the version control server to use.
        /// </summary>
        public InArgument<VersionControlServer> VersionControlServer { get; set; }
        
        /// <summary>
        /// Gets or sets the server path to find the latest changeset for. Defaults to $/
        /// </summary>
        public InArgument<string> VersionControlPath { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        public OutArgument<Changeset> Changeset { get; set; }

        /// <summary>
        /// Get the label details.
        /// </summary>
        protected override void InternalExecute()
        {
            string versionControlPath = this.VersionControlPath.Get(this.ActivityContext);
            if (string.IsNullOrEmpty(versionControlPath))
            {
                versionControlPath = "$/";
            }

            var vcserver = this.VersionControlServer.Get(this.ActivityContext);
            var queryHistoryResult = vcserver.QueryHistory(versionControlPath, VersionSpec.Latest, 0, RecursionType.Full, null, null, null, 1, true, false).Cast<Changeset>();
            if (!queryHistoryResult.Any())
            {
                throw new ChangesetNotFoundException("No current changeset available.");
            }

            var changeset = queryHistoryResult.First();
            this.Changeset.Set(this.ActivityContext, changeset);
        }
    }
}
