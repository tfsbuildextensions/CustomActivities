//-----------------------------------------------------------------------
// <copyright file="GetBuildDefinition.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Workflow activity that gets an <see cref="Microsoft.TeamFoundation.Build.Client.IBuildDefinition"/>
    /// based on the Build Server and build definition name provided.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetBuildDefinition : BaseCodeActivity<IBuildDefinition>
    {
        /// <summary>
        /// The name of the team project where the build definition exists.
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> TeamProjectName { get; set; }

        /// <summary>
        /// The name of the build definition to return.
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> BuildDefinitionName { get; set; }

        /// <summary>
        /// The <see cref="Microsoft.TeamFoundation.Build.Client.IBuildServer"/>
        /// object for the Team Foundation Server and Team
        /// Project Collection to use that contains the build definition.
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<IBuildServer> BuildServer { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity.
        /// </summary>
        /// <returns>The <see cref="Microsoft.TeamFoundation.Build.Client.IBuildDefinition"/>
        /// that is specified.</returns>
        protected override IBuildDefinition InternalExecute()
        {
            IBuildServer buildServer = this.BuildServer.Get(this.ActivityContext);
            string teamProjectName = this.TeamProjectName.Get(this.ActivityContext);
            string buildDefinitionName = this.BuildDefinitionName.Get(this.ActivityContext);
            
            return buildServer.GetBuildDefinition(teamProjectName, buildDefinitionName);
        }
    }
}
