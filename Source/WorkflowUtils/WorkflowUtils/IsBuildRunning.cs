using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.IO;

using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Build.Client;
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
    public sealed class IsBuildRunning : CodeActivity<bool>
    {
        public InArgument<String> BuildDefinition { get; set; }
        public InArgument<String> TeamProject { get; set; }
        public InArgument<String> TeamFoundationServer { get; set; }
        public InArgument<int> OptionalBuildId { get; set; }

        private TfsTeamProjectCollection mtfs;
        private IBuildController bc;
        private IBuildServer bs;
        private CustLogMessage logger;
        private int buildId = 0;
        private bool isErrorCaught;

        private string sBuildDefinition;
        private string sTeamProject;
        private string sTeamFoundationServer;
        
        
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">WF context</param>
        protected override bool Execute(CodeActivityContext context)
        {
            sBuildDefinition = context.GetValue(this.BuildDefinition);
            sTeamProject = context.GetValue(this.TeamProject);
            sTeamFoundationServer = context.GetValue(this.TeamFoundationServer);
            buildId = context.GetValue(this.OptionalBuildId);

            ConnectToTFS();
            bool isBuildRunning = false;

            if (buildId == 0)
                isBuildRunning = CheckDefinition();
            else
                isBuildRunning = CheckBuildId();

            context.SetValue(Result, isBuildRunning);
            return isBuildRunning;
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
                isErrorCaught = true;
            }
        }

        private bool CheckDefinition()
        {
            IQueuedBuildSpec buildSpec = bs.CreateBuildQueueSpec(sTeamProject, sBuildDefinition);
            IQueuedBuildQueryResult builds = bs.QueryQueuedBuilds(buildSpec);
            
            bool isBuildRunning = false;
            
            foreach (IQueuedBuild build in builds.QueuedBuilds)
                if (IsQueuedBuildRunning(build))
                        isBuildRunning = true;

            return isBuildRunning;
        }

        private bool CheckBuildId()
        {
            IQueuedBuild queuedBuild = bs.GetQueuedBuild(buildId, QueryOptions.All);
            return IsQueuedBuildRunning(queuedBuild);
        }

        private bool IsQueuedBuildRunning(IQueuedBuild build)
        {
            bool isQueuedBuildRunning = false;
            DateTime timeStampIfBuildIsRunning = new DateTime();
            if (build != null)
                if (build.Status == QueueStatus.Queued ||
                        build.Status == QueueStatus.InProgress ||
                        ((build.Build != null) && (build.Build.Status == BuildStatus.InProgress && build.Build.FinishTime == timeStampIfBuildIsRunning || build.Build.Status == BuildStatus.NotStarted)))
                    isQueuedBuildRunning = true;
            return isQueuedBuildRunning;
        }
    }
}
