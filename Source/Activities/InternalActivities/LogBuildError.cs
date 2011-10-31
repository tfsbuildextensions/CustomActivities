//-----------------------------------------------------------------------
// <copyright file="LogBuildError.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Internal
{
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;

    /// <summary>
    /// Logs a message as a build error
    /// Also can fail the build if the FailBuildOnError flag is set
    /// </summary>
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    internal class LogBuildError : BaseCodeActivity
    {
        [RequiredArgument]
        [Description("The Error log message")]
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// Logs a message as a build error
        /// Also can fail the build if the FailBuildOnError flag is set
        /// </summary>
        protected override void InternalExecute()
        {
            LogBuildError(this.Message.Get(ActivityContext));
        }
    }
}
