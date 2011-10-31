using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.IO;

using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WorkflowUtils
{
    /// <summary>
    /// Activity based on CodeActivity
    /// </summary>
    [BuildActivity (HostEnvironmentOption.All)]
    [BuildExtension (HostEnvironmentOption.All)]
    public sealed class ExecuteWorkflow : CodeActivity<IQueuedBuild>
    {
        public InArgument<String> BuildDefinition { get; set; }
        public InArgument<String> TeamProject { get; set; }
        public InArgument<String> TeamFoundationServer { get; set; }
        public InArgument<String> MSBuildArguments { get; set; }
        public InArgument<bool> OverrideExistingMSBuildArguments { get; set; }

        private TfsTeamProjectCollection mtfs;
        private IBuildController bc;
        private IBuildServer bs;
        private IQueuedBuild qb;

        private string sBuildDefinition;
        private string sTeamProject;
        private string sTeamFoundationServer;
        private string sMSBuildArguments = "";
        private bool bOverrideExistingMSBuildArguments = true;
        
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">WF context</param>
        protected override IQueuedBuild Execute(CodeActivityContext context)
        {
            sBuildDefinition = context.GetValue(this.BuildDefinition);
            sTeamProject = context.GetValue(this.TeamProject);
            sTeamFoundationServer = context.GetValue(this.TeamFoundationServer);
            sMSBuildArguments = context.GetValue(this.MSBuildArguments);
            bOverrideExistingMSBuildArguments = context.GetValue(this.OverrideExistingMSBuildArguments);

            ConnectToTFS();
            LaunchBuild();
            context.SetValue(Result, qb);
            return qb;
        }

        private void ConnectToTFS()
        {
            try
            {
                mtfs = new TfsTeamProjectCollection(new Uri(sTeamFoundationServer));
                bs = (IBuildServer)mtfs.GetService(typeof(IBuildServer));
                bc = (IBuildController)mtfs.GetService(typeof(IBuildController));
            }
            catch (Exception ex)
            {
                qb = null;
            }
        }

        private void LaunchBuild()
        {
            try
            {
                IBuildDefinition buildDefinition = bs.GetBuildDefinition(sTeamProject, sBuildDefinition);
                IBuildRequest buildRequest = buildDefinition.CreateBuildRequest();

                if (sMSBuildArguments != null && sMSBuildArguments != "")
                {
                    IDictionary<String, Object> processParameters = WorkflowHelpers.DeserializeProcessParameters(buildRequest.ProcessParameters);
                    IDictionary<String, Object> definitionProcessParameters = WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);

                    if (bOverrideExistingMSBuildArguments || !definitionProcessParameters.Keys.Contains(ProcessParameterMetadata.StandardParameterNames.MSBuildArguments))
                        processParameters[ProcessParameterMetadata.StandardParameterNames.MSBuildArguments] = sMSBuildArguments;
                    else
                        processParameters[ProcessParameterMetadata.StandardParameterNames.MSBuildArguments] = sMSBuildArguments + " " + definitionProcessParameters[ProcessParameterMetadata.StandardParameterNames.MSBuildArguments];
                    
                    buildRequest.ProcessParameters = WorkflowHelpers.SerializeProcessParameters(processParameters);
                }

                qb = bs.QueueBuild(buildRequest);
            }
            catch (Exception ex)
            {
                qb = null;
                throw new Exception("There is a problem with the build definition referenced");
            }
        }
    }
}
