//-----------------------------------------------------------------------
// <copyright file="RefreshWorkItem.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Ensures that the work item object is up to date.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class RefreshWorkItem : CodeActivity
    {
        /// <summary>
        /// The work item to refresh
        /// </summary>
        public InArgument<WorkItem> WorkItem { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            this.WorkItem.Get(context).SyncToLatest();
        }
    }
}
