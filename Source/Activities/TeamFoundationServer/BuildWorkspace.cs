//-----------------------------------------------------------------------
// <copyright file="BuildWorkspace.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;
    using TfsBuildExtensions.TfsUtilities;

    /// <summary>
    /// BuildWorkspaceAction
    /// </summary>
    public enum BuildWorkspaceAction
    {
        /// <summary>
        /// Add
        /// </summary>
        Add,

        /// <summary>
        /// Remove
        /// </summary>
        Remove,

        /// <summary>
        /// Clear
        /// </summary>
        Clear
    }

    /// <summary>
    /// Modifies the workspace associated with a Team Foundation Build
    /// <b>Valid Action values are:</b>
    /// <para><i>Add</i> - <b>Required: </b>BuildDefinition, LocalItem, ServerItem</para>
    /// <para><i>ClearAll</i> - <b>Required: </b>BuildDefinition</para>
    /// <para><i>Remove</i> - <b>Required: </b>BuildDefinition, LocalItem <i>or</i> ServerItem</para>
    /// </summary>
    [System.ComponentModel.Description("Activity to perform operations on a TFS Build Definition workspace")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class BuildWorkspace : BaseCodeActivity
    {
        private BuildWorkspaceAction action = BuildWorkspaceAction.Add;

        /// <summary>
        /// Specifies the action to perform. Default is Add.
        /// </summary>
        public BuildWorkspaceAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The build definition that will be modified by the activity.
        /// </summary>
        [System.ComponentModel.Description("The Build Definition that will be modified")]
        public InArgument<IBuildDefinition> BuildDefinition { get; set; }

        /// <summary>
        /// Local path for the build definition workspace. Use $(SourceDir)[\Path] to use the build agent's default.
        /// </summary>
        [System.ComponentModel.Description("Local path for the build definition workspace.")]
        public InArgument<string> LocalItem { get; set; }

        /// <summary>
        /// Server (source control) path that will be mapped to the workspace
        /// </summary>
        [System.ComponentModel.Description("Server path for the build definition workspace.")]
        public InArgument<string> ServerItem { get; set; }

        /// <summary>
        /// Internal Execute
        /// </summary>
        protected override void InternalExecute()
        {
            var buildDef = this.BuildDefinition.Get(ActivityContext);
            var localItem = this.LocalItem.Get(ActivityContext);
            var serverItem = this.ServerItem.Get(ActivityContext); 

            switch (this.Action)
            {
                case BuildWorkspaceAction.Add:
                    Build.AddWorkspaceMapping(buildDef, localItem, serverItem); 
                    break;
                case BuildWorkspaceAction.Remove:
                    if (string.IsNullOrWhiteSpace(localItem))
                    {
                        Build.RemoveServerMapping(buildDef, serverItem);
                    }
                    else
                    {
                        Build.RemoveLocalMapping(buildDef, localItem); 
                    }

                    break;
                case BuildWorkspaceAction.Clear:
                    Build.ClearWorkspaceMappings(buildDef); 
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }
    }
}
