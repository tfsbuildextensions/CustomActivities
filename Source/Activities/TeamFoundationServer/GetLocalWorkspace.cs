//-----------------------------------------------------------------------
// <copyright file="GetLocalWorkspace.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Get the workspace for the local machine.
    /// </summary>
    [System.ComponentModel.Description("Activity to retrieve the workspace definition for the current machine.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetLocalWorkspace : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the project collection.
        /// </summary>
        public InArgument<TfsTeamProjectCollection> Collection { get; set; }

        /// <summary>
        /// Gets or sets the changeset items.
        /// </summary>
        public OutArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// Get the changeset details.
        /// </summary>
        protected override void InternalExecute()
        {
            var collection = this.Collection.Get(this.ActivityContext);
            var wkstation = Workstation.Current;

            var info = wkstation.GetAllLocalWorkspaceInfo();

            // TODO: This needs some care to test the info result before going ahead...
            var ws = info[0].GetWorkspace(collection);

            this.Workspace.Set(this.ActivityContext, ws);
        }
    }
}
