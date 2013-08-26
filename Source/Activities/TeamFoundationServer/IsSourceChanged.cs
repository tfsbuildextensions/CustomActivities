//-----------------------------------------------------------------------
// <copyright file="IsSourceChanged.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Checks build definition workspace to determine if the source control has changed since a given datetime (i.e. when the last good build ran).  Useful for not launching builds if source hasn't changed.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class IsSourceChanged : BaseCodeActivity<bool>
    {
        private IBuildServer bs;
        private VersionControlServer vcs;
        private DateTime since;
        private TfsTeamProjectCollection mtfs;
        private List<string> exclusions;
        private string buildDefinition;
        private string teamProject;
        private string teamFoundationServer;
        private string serverPath;

        /// <summary>
        /// Specifies the TeamFoundationServer. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// Specifies the TeamProject. Defaults to that of the current build
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// Specifies the BuildDefinition
        /// </summary>
        public InArgument<string> BuildDefinition { get; set; }

        /// <summary>
        /// Specifies the ServerPath to query
        /// </summary>
        public InArgument<string> ServerPath { get; set; }

        /// <summary>
        /// Since
        /// </summary>
        public InArgument<DateTime> Since { get; set; }

        /// <summary>
        /// Exclusions
        /// </summary>
        public InArgument<List<string>> Exclusions { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>bool</returns>
        protected override bool InternalExecute()
        {
            this.buildDefinition = this.BuildDefinition.Get(this.ActivityContext);
            this.serverPath = (this.ServerPath.Get(this.ActivityContext) == null) ? string.Empty : this.ServerPath.Get(this.ActivityContext);
            this.teamProject = this.TeamProject.Get(this.ActivityContext);
            this.teamFoundationServer = this.TeamFoundationServer.Get(this.ActivityContext);
            this.since = this.Since.Get(this.ActivityContext);
            this.exclusions = this.Exclusions.Get(this.ActivityContext);

            this.ConnectToTFS();

            bool containsChanges = string.IsNullOrEmpty(this.serverPath) ? this.CheckWorkspaces() : this.CheckServerPath();

            this.Result.Set(this.ActivityContext, containsChanges);

            return containsChanges;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTFS()
        {
            this.mtfs = new TfsTeamProjectCollection(new Uri(this.teamFoundationServer));
            this.bs = (IBuildServer)this.mtfs.GetService(typeof(IBuildServer));
            this.vcs = (VersionControlServer)this.mtfs.GetService(typeof(VersionControlServer));
        }

        private bool CheckWorkspaces()
        {
            IBuildDefinition builddef = this.bs.GetBuildDefinition(this.teamProject, this.buildDefinition);
            IWorkspaceTemplate workspaceTemplate = builddef.Workspace;
            bool containsChanges = false;

            foreach (IWorkspaceMapping mapping in workspaceTemplate.Mappings)
            {
                if (mapping.MappingType == WorkspaceMappingType.Map)
                {
                    if (this.exclusions != null && this.exclusions.Contains(mapping.ServerItem))
                    {
                        continue;
                    }

                    ItemSet itemSet = this.vcs.GetItems(mapping.ServerItem, RecursionType.Full);

                    foreach (Item item in itemSet.Items)
                    {
                        if (!workspaceTemplate.Mappings.Exists(m => item.ServerItem.Contains(m.ServerItem) && m.MappingType == WorkspaceMappingType.Cloak && m.ServerItem.Contains(mapping.ServerItem)))
                        {
                            if (item.CheckinDate >= this.since)
                            {
                                containsChanges = true;
                                foreach (string exclusion in this.exclusions)
                                {
                                    if (item.ServerItem.Contains(exclusion))
                                    {
                                        containsChanges = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return containsChanges;
        }

        private bool CheckServerPath()
        {
            bool containsChanges = false;

            ItemSet itemSet = this.vcs.GetItems(this.serverPath, RecursionType.Full);

            foreach (Item item in itemSet.Items)
            {
                if (item.CheckinDate >= this.since)
                {
                    containsChanges = true;
                    if (this.exclusions != null)
                    {
                        foreach (string e in this.exclusions)
                        {
                            if (item.ServerItem.Contains(e))
                            {
                                containsChanges = false;
                            }
                        }
                    }
                }
            }

            return containsChanges;
        }
    }
}
