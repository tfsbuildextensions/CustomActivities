
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class BuildAgentTags : CodeActivity
    {
        private TagAction action = TagAction.Add;

        [RequiredArgument]
        public TagAction Action { get { return this.action; } set { this.action = value; } }

        [RequiredArgument]
        [Browsable(true)]
        public InArgument<IBuildAgent> BuildAgent { get; set; }

        [RequiredArgument]
        [Browsable(true)]
        public InArgument<String> Tag { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IBuildAgent buildAgent = context.GetValue(this.BuildAgent);
            String tag = context.GetValue(this.Tag);
            switch (action)
            {
                case TagAction.Add:
                    buildAgent.Tags.Add(tag);
                    buildAgent.Save();
                    break;
                case TagAction.Remove:
                    buildAgent.Tags.Remove(tag);
                    buildAgent.Save();
                    break;
                default:
                    break;
            }
        }
    }

    public enum TagAction
    {
        Add, Remove
    }
}
