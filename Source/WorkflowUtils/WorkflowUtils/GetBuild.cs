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
    public sealed class GetBuild : CodeActivity<IQueuedBuild>
    {
        // Define an activity input argument of type string
        public InArgument<String> TeamFoundationServer { get; set; }
        public InArgument<int> BuildId { get; set; }

        private TfsTeamProjectCollection mtfs;
        private IBuildController bc;
        private IBuildServer bs;
        private int buildId;

        private string sTeamFoundationServer;
        
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">WF context</param>
        protected override IQueuedBuild Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            sTeamFoundationServer = context.GetValue(this.TeamFoundationServer);
            buildId = (context.GetValue(this.BuildId) == null) ? 0 : context.GetValue(this.BuildId);

            ConnectToTFS();
            IQueuedBuild build = RetrieveBuild();
            context.SetValue(Result, build);
            return build;
        }

        private void ConnectToTFS()
        {
            try
            {
                mtfs = new TfsTeamProjectCollection(new Uri(sTeamFoundationServer));
                bs = (IBuildServer)mtfs.GetService(typeof(IBuildServer));
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem connecting to this TFS server: " + sTeamFoundationServer);
            }
        }

        private IQueuedBuild RetrieveBuild()
        {
            IQueuedBuild buildToRetrieve = bs.GetQueuedBuild(buildId, QueryOptions.All);
            if (buildToRetrieve == null)
                throw new Exception("Queued build: " + buildId + " does not exist.");
            return buildToRetrieve;
        }
    }
}
