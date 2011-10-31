//-----------------------------------------------------------------------
// <copyright file="GetChangesetItems.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Get the change items from a changeset.
    /// </summary>
    [System.ComponentModel.Description("Activity to get change items from a changeset.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetChangesetItems : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        public InArgument<Changeset> Changeset { get; set; }

        /// <summary>
        /// Gets or sets the changeset items.
        /// </summary>
        public OutArgument<IEnumerable<string>> ChangesetItems { get; set; }

        /// <summary>
        /// Get the changeset details.
        /// </summary>
        protected override void InternalExecute()
        {
            var changes = this.Changeset.Get(this.ActivityContext);

            this.ChangesetItems.Set(this.ActivityContext, changes.Changes.Select(x => x.Item.ServerItem));
        }
    }
}
