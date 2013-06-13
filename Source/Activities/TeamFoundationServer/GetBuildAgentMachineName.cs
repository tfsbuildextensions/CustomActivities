//-----------------------------------------------------------------------
// <copyright file="GetBuildAgentMachineName.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get BuildAgent MachineName
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetBuildAgentMachineName : CodeActivity
    {
        /// <summary>
        /// BuildAgent
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<IBuildAgent> BuildAgent { get; set; }

        /// <summary>
        /// AgentMachineName
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public OutArgument<string> AgentMachineName { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IBuildAgent buildAgent = context.GetValue(this.BuildAgent);
            context.SetValue(this.AgentMachineName, buildAgent.Url.Host);
        }
    }
}