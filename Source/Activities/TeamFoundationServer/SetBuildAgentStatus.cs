//-----------------------------------------------------------------------
// <copyright file="SetBuildAgentStatus.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Set the status of a build agent.
    /// </summary>
    [System.ComponentModel.Description("Activity to set the status of a build agent.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SetBuildAgentStatus : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the build server.
        /// </summary>
        public InArgument<IBuildServer> BuildServer { get; set; }

        /// <summary>
        /// Gets or sets the build agent name.
        /// </summary>
        public InArgument<string> BuildAgentName { get; set; }

        /// <summary>
        /// Gets or sets the full version control label.
        /// </summary>
        public InArgument<AgentStatus> BuildAgentStatus { get; set; }

        /// <summary>
        /// Get the label details.
        /// </summary>
        protected override void InternalExecute()
        {
            var buildServer = this.BuildServer.Get(this.ActivityContext);
            var agentName = this.BuildAgentName.Get(this.ActivityContext);

            // Create a spec for the agent we want to find
            var agentSpec = buildServer.CreateBuildAgentSpec();
            agentSpec.Name = agentName;

            // Query for the single build agent
            var result = buildServer.QueryBuildAgents(agentSpec);

            // Set the status of the agent
            result.Agents[0].Status = this.BuildAgentStatus.Get(this.ActivityContext);
            result.Agents[0].Save();
        }
    }
}
