//-----------------------------------------------------------------------
// <copyright file="IsSourceChanged.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
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
        private List<string> exclusions;

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
            this.since = this.Since.Get(this.ActivityContext);
            this.exclusions = this.Exclusions.Get(this.ActivityContext);
            this.ConnectToTfs();
            bool containsChanges = this.ServerPath.Expression == null ? this.CheckWorkspaces() : this.CheckServerPath();
            this.ActivityContext.SetValue(Result, containsChanges);
            return containsChanges;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "TODO: Need to resolve this.")]
        private void ConnectToTfs()
        {
            TfsTeamProjectCollection tpc = this.TeamFoundationServer.Expression == null ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.TeamFoundationServer.Get(this.ActivityContext)));
            this.bs = (IBuildServer)tpc.GetService(typeof(IBuildServer));
            this.vcs = (VersionControlServer)tpc.GetService(typeof(VersionControlServer));
        }

        private bool CheckWorkspaces()
        {
            if (this.TeamProject.Expression == null)
            {
                var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                this.TeamProject.Set(this.ActivityContext, buildDetail.TeamProject);
            }

            IBuildDefinition buildDefinition = this.bs.GetBuildDefinition(this.TeamProject.Get(this.ActivityContext), this.BuildDefinition.Get(this.ActivityContext));
            IWorkspaceTemplate workspaceTemplate = buildDefinition.Workspace;
            bool containsChanges = false;

            foreach (IWorkspaceMapping mapping in workspaceTemplate.Mappings)
            {
                if (mapping.MappingType == WorkspaceMappingType.Map)
                {
                    if (this.exclusions != null && this.exclusions.Contains(mapping.ServerItem))
                    {
                        continue;
                    }

                    string serverPath = mapping.ServerItem;
                    ItemSet itemSet = this.vcs.GetItems(serverPath, RecursionType.Full);

                    foreach (Item item in itemSet.Items.Where(item => !workspaceTemplate.Mappings.Exists(m => item.ServerItem.Contains(m.ServerItem) && m.MappingType == WorkspaceMappingType.Cloak && m.ServerItem.Contains(mapping.ServerItem))).Where(item => item.CheckinDate >= this.since))
                    {
                        containsChanges = true;
                        foreach (string exclusion in this.exclusions.Where(exclusion => item.ServerItem.Contains(exclusion)))
                        {
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

            ItemSet itemSet = this.vcs.GetItems(this.ServerPath.Get(this.ActivityContext), RecursionType.Full);

            foreach (Item item in itemSet.Items.Where(item => item.CheckinDate >= this.since))
            {
                containsChanges = true;
                if (this.exclusions != null)
                {
                    foreach (string e in this.exclusions.Where(e => item.ServerItem.Contains(e)))
                    {
                        containsChanges = false;
                    }
                }
            }

            return containsChanges;
        }
    }
}
