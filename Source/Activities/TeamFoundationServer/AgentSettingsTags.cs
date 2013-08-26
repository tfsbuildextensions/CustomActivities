//-----------------------------------------------------------------------
// <copyright file="AgentSettingsTags.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;

    /// <summary>
    /// AgentSettingsTags
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class AgentSettingsTags : BaseCodeActivity
    {
        private TagAction action = TagAction.Add;

        /// <summary>
        /// Action
        /// </summary>
        [RequiredArgument]
        public TagAction Action
        {
            get { return this.action; } 
            set { this.action = value; }
        }

        /// <summary>
        /// AgentSettings
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<AgentSettings> AgentSettings { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> Tag { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        protected override void InternalExecute()
        {
            AgentSettings agentSettings = this.AgentSettings.Get(this.ActivityContext);
            string tag = this.Tag.Get(this.ActivityContext);
            switch (this.action)
            {
                case TagAction.Add:
                    agentSettings.Tags.Add(tag);
                    break;
                case TagAction.Remove:
                    agentSettings.Tags.Remove(tag);
                    break;
            }
        }
    }
}