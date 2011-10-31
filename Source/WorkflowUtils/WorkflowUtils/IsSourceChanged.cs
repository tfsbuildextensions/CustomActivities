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
    public sealed class IsSourceChanged : CodeActivity<bool>
    {
        public InArgument<String> BuildDefinition { get; set; }
        public InArgument<String> ServerPath { get; set; }
        public InArgument<String> TeamProject { get; set; }
        public InArgument<String> TeamFoundationServer { get; set; }
        public InArgument<DateTime> Since { get; set; }
        public InArgument<List<String>> Exclusions { get; set; }        

        private TeamFoundationServer mtfs;
        //private IBuildController bc;
        private IBuildServer bs;
        private VersionControlServer vcs;
        private CustLogMessage logger;
        private bool isErrorCaught;

        private string sBuildDefinition;
        private string sServerPath;
        private string sTeamProject;
        private string sTeamFoundationServer;
        private DateTime since;
        private List<String> exclusions;
        
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">WF context</param>
        protected override bool Execute(CodeActivityContext context)
        {
            sBuildDefinition = context.GetValue(this.BuildDefinition);
            sServerPath = (context.GetValue(this.ServerPath) == null) ? "" : context.GetValue(this.ServerPath);
            sTeamProject = context.GetValue(this.TeamProject);
            sTeamFoundationServer = context.GetValue(this.TeamFoundationServer);
            since = context.GetValue(this.Since);
            exclusions = context.GetValue(this.Exclusions);

            ConnectToTFS();
            bool containsChanges = true;

            if (sServerPath == "")
                containsChanges = CheckWorkspaces();
            else
                containsChanges = CheckServerPath();

            context.SetValue(Result, containsChanges);
            return containsChanges;
        }

        private void ConnectToTFS()
        {
            try
            {
                mtfs = new TeamFoundationServer(sTeamFoundationServer);
                bs = (IBuildServer)mtfs.GetService(typeof(IBuildServer));
                vcs = (VersionControlServer)mtfs.GetService(typeof(VersionControlServer));
            }
            catch (Exception ex)
            {
                isErrorCaught = true;
            }
        }

        private bool CheckWorkspaces()
        {
            IBuildDefinition buildDefinition = bs.GetBuildDefinition(sTeamProject, sBuildDefinition);
            IWorkspaceTemplate workspaceTemplate = buildDefinition.Workspace;
            bool containsChanges = false;

            foreach (IWorkspaceMapping mapping in workspaceTemplate.Mappings)
            {
                if (mapping.MappingType == WorkspaceMappingType.Map)
                {
                    if (exclusions != null && exclusions.Contains(mapping.ServerItem))
                    {
                        continue;
                    }
                    else
                    {
                        String serverPath = mapping.ServerItem;
                        ItemSet itemSet = vcs.GetItems(serverPath, RecursionType.Full);

                        foreach (Item item in itemSet.Items)
                            if (!workspaceTemplate.Mappings.Exists(m => item.ServerItem.Contains(m.ServerItem) && m.MappingType == WorkspaceMappingType.Cloak && m.ServerItem.Contains(mapping.ServerItem)))
                                if (item.CheckinDate >= since)
                                {
                                    containsChanges = true;
                                    foreach (String exclusion in exclusions)
                                        if (item.ServerItem.Contains(exclusion))
                                            containsChanges = false;                                        
                                }
                    }
                }
            }

            return containsChanges;
        }

        private bool CheckServerPath()
        {
            bool containsChanges = false;

            ItemSet itemSet = vcs.GetItems(sServerPath, RecursionType.Full);

            foreach (Item item in itemSet.Items)
                if (item.CheckinDate >= since)
                {
                    containsChanges = true;
                    if (exclusions != null)
                        foreach (String e in exclusions)
                            if (item.ServerItem.Contains(e))
                                containsChanges = false;
                }

            return containsChanges;
        }
    }
}
