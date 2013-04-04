namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class AgentSettingsTags : CodeActivity
    {
        private TagAction action = TagAction.Add;

        [RequiredArgument]
        public TagAction Action { get { return this.action; } set { this.action = value; } }

        [RequiredArgument]
        [Browsable(true)]
        public InArgument<AgentSettings> AgentSettings { get; set; }

        [RequiredArgument]
        [Browsable(true)]
        public InArgument<String> Tag { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            AgentSettings agentSettings = context.GetValue(this.AgentSettings);
            String tag = context.GetValue(this.Tag);
            switch (action)
            {
                case TagAction.Add:
                    agentSettings.Tags.Add(tag);
                    break;
                case TagAction.Remove:
                    agentSettings.Tags.Remove(tag);
                    break;
                default:
                    break;
            }
        }
    }
}