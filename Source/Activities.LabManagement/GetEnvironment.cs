//-----------------------------------------------------------------------
// <copyright file="GetEnvironment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Lab.Client;

    /*
     *  This Activity Represents a Work in progress and is subject to change without notice until the
     *  corresponding process template has been published.
     */

    /// <summary>
    /// An activity that lists TFS Lab Management Lab Environments based on tag filters.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetEnvironment : BaseCodeActivity
    {
        /// <summary>
        /// Specifies the name of the environment to select. The environment is located in the team project executing the build. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string> EnvironmentName { get; set; }

        /// <summary>
        /// Defines the returned names of matching lab environments.
        /// </summary>
        public OutArgument<LabEnvironment> Environment { get; set; }

        /// <summary>
        /// Execute the GetEnvironment build activity.
        /// </summary>
        protected override void InternalExecute()
        {
            this.FindEnvironment();
        }

        private void FindEnvironment()
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
                    this.LogBuildMessage(string.Format("Found lab environment {0}", matchingName));
                    this.ActivityContext.SetValue(this.Environment, environment);
                    break;
                }
            }
        }
    }
}