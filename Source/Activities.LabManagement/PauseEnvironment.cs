//-----------------------------------------------------------------------
// <copyright file="PauseEnvironment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Lab.Client;

    /// <summary>
    /// This activity will set a TFS Lab Management Lab Environment in a Paused state.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class PauseEnvironment : BaseCodeActivity
    {
        /// <summary>
        /// Specifies the name of the environment to shutdown. The environment is located in the team project executing the build. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string> EnvironmentName { get; set; }

        /// <summary>
        /// Execute the ShutdownEnvironment build activity.
        /// </summary>
        protected override void InternalExecute()
        {
            this.Pause();
        }

        private void Pause()
        {
            var tpc = this.ActivityContext.GetExtension<TfsTeamProjectCollection>();
            var labService = tpc.GetService<LabService>();
            var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            var environments = labService.QueryLabEnvironments(
                                    new LabEnvironmentQuerySpec() { Project = buildDetail.TeamProject });

            var matchingName = this.ActivityContext.GetValue(this.EnvironmentName);
            foreach (var environment in environments)
            {
                if (environment.Name.ToUpper() == matchingName.ToUpper())
                {
                    this.LogBuildMessage(string.Format("Pausing lab environment {0}", matchingName));
                    environment.Pause();
                    break;
                }
            }
        }
    }
}