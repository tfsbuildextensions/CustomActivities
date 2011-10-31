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
    public sealed class GetLastGoodBuild : CodeActivity<IBuildDetail>
    {
        // Define an activity input argument of type string
        public InArgument<String> BuildDefinition { get; set; }
        public InArgument<String> TeamProject { get; set; }
        public InArgument<String> TeamFoundationServer { get; set; }
        public InArgument<IBuildDetail> ParentBuild { get; set; }

        private TfsTeamProjectCollection mtfs;
        private IBuildController bc;
        private IBuildServer bs;
        private CustLogMessage logger;
        private IBuildDetail build;
        private bool isErrorCaught;

        private string sBuildDefinition;
        private string sTeamProject;
        private string sTeamFoundationServer;        
        
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">WF context</param>
        protected override IBuildDetail Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            sBuildDefinition = context.GetValue(this.BuildDefinition);
            sTeamProject = context.GetValue(this.TeamProject);
            sTeamFoundationServer = context.GetValue(this.TeamFoundationServer);
            build = context.GetValue(this.ParentBuild);

            ConnectToTFS();
            build = GetGoodBuild() ?? build;            
            context.SetValue(Result, build);
            return build;
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

        private IBuildDetail GetGoodBuild()
        {
            IBuildDefinition buildDefinition = bs.GetBuildDefinition(sTeamProject, sBuildDefinition);
            IBuildDetail[] builds = bs.QueryBuilds(buildDefinition);
            IBuildDetail latestBuild = null;
            
            foreach (IBuildDetail build in builds)
                if (build.Status == BuildStatus.Succeeded && Directory.Exists(build.DropLocation))
                    if (latestBuild == null || build.StartTime > latestBuild.StartTime)
                        latestBuild = build;

            return latestBuild;
        }
    }
}
