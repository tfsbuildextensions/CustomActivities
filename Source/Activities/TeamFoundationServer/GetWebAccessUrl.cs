//-----------------------------------------------------------------------
// <copyright file="GetWebAccessUrl.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.TfsUtilities;

    /// <summary>
    /// GuidAction
    /// </summary>
    public enum GetWebAccessUrlAction
    {
        /// <summary>
        /// BuildDetails
        /// </summary>
        BuildDetails,

        /// <summary>
        /// WorkItemEditor
        /// </summary>
        WorkItemEditor
    }

    /// <summary>
    /// <b>Valid Action values are:</b>
    /// <para><i>BuildDetails</i> - <b>Required: </b>TeamProjectCollection, ItemId <b>Output: </b> Result</para>
    /// <para><i>WorkItemEditor</i> - <b>Required: </b>TeamProjectCollection, ItemUri <b>Output: </b> Result</para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [System.ComponentModel.Description("Activity to return TFS Web Access Urls for items.")]
    public sealed class GetWebAccessUrl : TFBaseCodeActivity
    {
        private GetWebAccessUrlAction action = GetWebAccessUrlAction.BuildDetails;

        /// <summary>
        /// Specifies the action to perform. Default is BuildDetails.
        /// </summary>
        public GetWebAccessUrlAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Team Foundation Uri for specific items
        /// <list type="bullet">
        ///     <listheader>
        ///     <description>Used for:</description>
        ///     </listheader>
        ///     <item>
        ///         <description>BuildDetails - Uri for the build.</description>
        ///     </item>
        /// </list>
        /// </summary>
        [System.ComponentModel.Description("Team Foundation Uri for the item.")]
        public InArgument<System.Uri> ItemUri { get; set; }

        /// <summary>
        /// Team Foundation ID for the linked item
        /// <list type="bullet">
        ///     <listheader>
        ///     <description>Used for:</description>
        ///     </listheader>
        ///     <item>
        ///         <description>WorkItemEditor - the work item id.</description>
        ///     </item>
        /// </list>
        /// </summary>
        [System.ComponentModel.Description("Team Foundation Id for the item.")]
        public InArgument<int> ItemId { get; set; }

        /// <summary>
        /// Result Url for the item in TFS Web Access.
        /// </summary>
        [System.ComponentModel.Description("Result Url for the item in TFWA.")]
        public OutArgument<System.Uri> Result { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            var itemUri = this.ItemUri.Get(ActivityContext);
            var itemId = this.ItemId.Get(ActivityContext); 

            System.Uri result; 
            switch (this.Action)
            {
                case GetWebAccessUrlAction.BuildDetails:
                    result = WebAccess.GetBuildDetailsUri(ProjectCollection, itemUri); 
                    break;

                case GetWebAccessUrlAction.WorkItemEditor:
                    result = WebAccess.GetWorkItemEditorUri(ProjectCollection, itemId); 
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }

            this.Result.Set(ActivityContext, result); 
        }
    }
}