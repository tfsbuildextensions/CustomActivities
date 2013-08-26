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
    public sealed class GetBuildAgentMachineName : BaseCodeActivity
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
        protected override void InternalExecute()
        {
            IBuildAgent buildAgent = this.BuildAgent.Get(this.ActivityContext);
            this.AgentMachineName.Set(this.ActivityContext, buildAgent.Url.Host);
        }
    }
}