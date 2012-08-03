//-----------------------------------------------------------------------
// <copyright file="LogBuildMessage.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Internal
{
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using Microsoft.VisualBasic.Activities;

    [ActivityTracking(ActivityTrackingOption.None)]
    internal sealed class LogBuildMessage : BaseCodeActivity
    {
        private InArgument<BuildMessageImportance> importance;
        private bool importanceSet;

        /// <summary>
        /// Initializes a new instance of the LogBuildMessage class
        /// </summary>
        public LogBuildMessage()
        {
            this.importance = new InArgument<BuildMessageImportance>(new VisualBasicValue<BuildMessageImportance>
            {
                ExpressionText = "Microsoft.TeamFoundation.Build.Client.BuildMessageImportance.Normal"
            });
        }

        [RequiredArgument]
        [Description("The Message")]
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// The importance of the message to be logged
        /// </summary>
        public InArgument<BuildMessageImportance> Importance
        {
            get
            {
                return this.importance;
            }

            set
            {
                this.importance = value;
                this.importanceSet = true;
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeImportance()
        {
            return this.importanceSet;
        }

        protected override void InternalExecute()
        {
            this.LogBuildMessage(this.Message.Get(this.ActivityContext), this.Importance.Get(this.ActivityContext));
        }
    }
}
