//-----------------------------------------------------------------------
// <copyright file="ListLabSystems.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// An activity that lists TFS Lab Management Lab Systems based on tag filters.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ListLabSystems : CodeActivity
    {
        /// <summary>
        /// Specifies the filter criteria to match environments. The tags are to be specified as name-value
        /// pairs (name=value) and only environments matching all tags will be returned. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string[]> Tags { get; set; }

        /// <summary>
        /// Defines the returned names of matching lab systems.
        /// </summary>
        public OutArgument<string[]> LabSystems { get; set; }

        /// <summary>
        /// Execute the ListEnvironment build activity.
        /// </summary>
        /// <param name="context">Contains the workflow context</param>
        protected override void Execute(CodeActivityContext context)
        {
            var tpc = context.GetExtension<TfsTeamProjectCollection>();
            var labService = tpc.GetService<LabService>();
            var buildDetail = context.GetExtension<IBuildDetail>();
            var environments = labService.QueryLabEnvironments(
                                    new LabEnvironmentQuerySpec() { Project = buildDetail.TeamProject });

            var filterTags = context.GetValue(this.Tags);

            var matchingLabSystems = new List<string>();
            foreach (var environment in environments)
            {
                foreach (var labSystem in environment.LabSystems)
                {
                    foreach (var filterTag in filterTags)
                    {
                        var tagParts = filterTag.Split('=');
                        if (tagParts.Length == 2)
                        {
                            string labTag = null;
                            if (labSystem.CustomProperties.TryGetValue(tagParts[0], out labTag) && labTag.Equals(tagParts[1]))
                            {
                                matchingLabSystems.Add(environment.Name);
                            }
                        }
                    }
                }
            }

            context.SetValue(this.LabSystems, matchingLabSystems.ToArray());
        }
    }
}