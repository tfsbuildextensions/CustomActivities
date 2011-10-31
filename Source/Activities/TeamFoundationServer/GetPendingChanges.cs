//-----------------------------------------------------------------------
// <copyright file="GetPendingChanges.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Get the most recent pending changes in the workspace.
    /// </summary>
    [System.ComponentModel.Description("Activity to get the most recent pending changes from a workspace.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetPendingChanges : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the workspace to use.
        /// </summary>
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// Gets or sets the changed items.
        /// </summary>
        public OutArgument<IEnumerable<string>> PendingItems { get; set; }

        /// <summary>
        /// Get the pending changes.
        /// </summary>
        protected override void InternalExecute()
        {
            var workspace = this.Workspace.Get(this.ActivityContext);

            var changeArray = workspace.GetPendingChanges("$/", RecursionType.Full, false);

            this.PendingItems.Set(this.ActivityContext, changeArray.Select(x => x.ServerItem));
        }
    }
}
