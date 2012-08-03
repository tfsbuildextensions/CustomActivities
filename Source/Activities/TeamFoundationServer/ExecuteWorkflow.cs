//-----------------------------------------------------------------------
// <copyright file="ExecuteWorkflow.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Activity based on CodeActivity
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ExecuteWorkflow : BaseCodeActivity<IQueuedBuild>
    {
        private IBuildServer bs;
        private IQueuedBuild qb;

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the BuildDefinition to query
        /// </summary>
        public InArgument<string> BuildDefinition { get; set; }

        /// <summary>
        /// Specifies the TeamProject. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// Specifies the MSBuildArguments
        /// </summary>
        public InArgument<string> MSBuildArguments { get; set; }

        /// <summary>
        /// Specifies whether to OverrideExistingMSBuildArguments
        /// </summary>
        public InArgument<bool> OverrideExistingMSBuildArguments { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>IQueuedBuild</returns>
        protected override IQueuedBuild InternalExecute()
        {
            this.ConnectToTfs();
            this.LaunchBuild();
            this.ActivityContext.SetValue(this.Result, this.qb);
            return this.qb;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
        }

        private void LaunchBuild()
        {
            try
            {
                if (this.TeamProject.Expression == null)
                {
                    var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                    this.TeamProject.Set(this.ActivityContext, buildDetail.TeamProject);
                }

                IBuildDefinition buildDefinition = this.bs.GetBuildDefinition(this.TeamProject.Get(this.ActivityContext), this.BuildDefinition.Get(this.ActivityContext));
                IBuildRequest buildRequest = buildDefinition.CreateBuildRequest();

                if (this.MSBuildArguments.Expression != null)
                {
                    IDictionary<string, object> processParameters = WorkflowHelpers.DeserializeProcessParameters(buildRequest.ProcessParameters);
                    IDictionary<string, object> definitionProcessParameters = WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);

                    if (this.OverrideExistingMSBuildArguments.Get(this.ActivityContext) || !definitionProcessParameters.Keys.Contains(ProcessParameterMetadata.StandardParameterNames.MSBuildArguments))
                    {
                        processParameters[ProcessParameterMetadata.StandardParameterNames.MSBuildArguments] = this.MSBuildArguments.Get(this.ActivityContext);
                    }
                    else
                    {
                        processParameters[ProcessParameterMetadata.StandardParameterNames.MSBuildArguments] = this.MSBuildArguments.Get(this.ActivityContext) + " " + definitionProcessParameters[ProcessParameterMetadata.StandardParameterNames.MSBuildArguments];
                    }
                    
                    buildRequest.ProcessParameters = WorkflowHelpers.SerializeProcessParameters(processParameters);
                }

                this.qb = this.bs.QueueBuild(buildRequest);
            }
            catch (Exception)
            {
                this.qb = null;
                this.LogBuildError("There is a problem with the build definition referenced");
            }
        }
    }
}
