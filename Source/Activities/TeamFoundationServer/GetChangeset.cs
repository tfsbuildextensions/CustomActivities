//-----------------------------------------------------------------------
// <copyright file="GetChangeset.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Get a specific changeset.
    /// </summary>
    [System.ComponentModel.Description("Activity to get change items from a changeset.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetChangeset : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the changeset specification.
        /// </summary>
        public InArgument<int> ChangesetId { get; set; }

        /// <summary>
        /// Gets or sets the version control server to use.
        /// </summary>
        public InArgument<VersionControlServer> VersionControlServer { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        public OutArgument<Changeset> Changeset { get; set; }

        /// <summary>
        /// Get the changeset details.
        /// </summary>
        protected override void InternalExecute()
        {
            var change = this.ChangesetId.Get(this.ActivityContext);
            var vcserver = this.VersionControlServer.Get(this.ActivityContext);

            var changeset = vcserver.GetChangeset(change);

            this.Changeset.Set(this.ActivityContext, changeset);
        }
    }
}
