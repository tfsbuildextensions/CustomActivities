//-----------------------------------------------------------------------
// <copyright file="QueryBuildAgentStatus.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the status of a build agent.
    /// </summary>
    [System.ComponentModel.Description("Activity to get the status of a build agent.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class QueryBuildAgentStatus : BaseCodeActivity
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
        public OutArgument<AgentStatus> BuildAgentStatus { get; set; }

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

            // Return the status
            this.BuildAgentStatus.Set(this.ActivityContext, result.Agents[0].Status);
        }
    }
}
