//-----------------------------------------------------------------------
// <copyright file="GetLastGoodBuild.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Activity based on CodeActivity
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "TODO: Need to resolve this."), BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class GetLastGoodBuild : CodeActivity<IBuildDetail>
    {
        private TfsTeamProjectCollection mtfs;
        private IBuildServer bs;
        private IBuildDetail build;
        private string buildDefinition;
        private string teamProject;
        private string teamFoundationServer;
        private string buildQuality;

        /// <summary>
        /// The BuildDefinition to check
        /// </summary>
        public InArgument<string> BuildDefinition { get; set; }

        /// <summary>
        /// The TeamProject to connect to
        /// </summary>
        public InArgument<string> TeamProject { get; set; }

        /// <summary>
        /// The TeamFoundationServer to connect to.
        /// If parameter is empty then the server to which the build server is connected is used
        /// </summary>
        public InArgument<string> TeamFoundationServer { get; set; }

        /// <summary>
        /// The ParentBuild
        /// </summary>
        public InArgument<IBuildDetail> ParentBuild { get; set; }

        /// <summary>
        /// The build quality. If set only the last build with this
        /// build quality value will be returned (the comparison
        /// is case insensitive)
        /// </summary>
        public InArgument<string> BuildQuality { get; set; }

        private CodeActivityContext ActivityContext { get; set; }

        /// <summary>
        /// Executes the workflow
        /// </summary>
        /// <param name="context">The CodeActivityContext</param>
        /// <returns>IBuildDetail</returns>
        protected override IBuildDetail Execute(CodeActivityContext context)
        {
            this.ActivityContext = context;

            this.buildDefinition = context.GetValue(this.BuildDefinition);
            this.teamProject = context.GetValue(this.TeamProject);
            this.teamFoundationServer = context.GetValue(this.TeamFoundationServer);
            this.build = context.GetValue(this.ParentBuild);
            this.buildQuality = context.GetValue(this.BuildQuality);

            this.ConnectToTFS();
            this.build = this.GetGoodBuild() ?? this.build;
            context.SetValue(this.Result, this.build);
            return this.build;
        }

        private void ConnectToTFS()
        {
            this.mtfs = string.IsNullOrEmpty(this.teamFoundationServer) ? this.ActivityContext.GetExtension<TfsTeamProjectCollection>() : new TfsTeamProjectCollection(new Uri(this.teamFoundationServer));
            this.bs = (IBuildServer)this.mtfs.GetService(typeof(IBuildServer));
        }

        private IBuildDetail GetGoodBuild()
        {
            IBuildDefinition buildDef = this.bs.GetBuildDefinition(this.teamProject, this.buildDefinition);
            IBuildDetail[] builds = this.bs.QueryBuilds(buildDef);
            IBuildDetail latestBuild = null;

            foreach (IBuildDetail b in builds)
            {
                if (b.Status == BuildStatus.Succeeded)
                {
                    if (latestBuild == null || b.StartTime > latestBuild.StartTime)
                    {
                        if (string.IsNullOrEmpty(this.buildQuality) || string.Compare(b.Quality, this.buildQuality, StringComparison.OrdinalIgnoreCase) == 0) 
                        {
                            latestBuild = b;
                        }
                    }
                }
            }

            return latestBuild;
        }
    }
}
