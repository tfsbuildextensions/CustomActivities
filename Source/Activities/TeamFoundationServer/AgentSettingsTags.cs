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

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class AgentSettingsTags : CodeActivity
    {
        private TagAction action = TagAction.Add;

        [RequiredArgument]
        public TagAction Action
        {
            get { return this.action; } 
            set { this.action = value; }
        }

        [RequiredArgument]
        [Browsable(true)]
        public InArgument<AgentSettings> AgentSettings { get; set; }

        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> Tag { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            AgentSettings agentSettings = context.GetValue(this.AgentSettings);
            string tag = context.GetValue(this.Tag);
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