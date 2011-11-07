//-----------------------------------------------------------------------
// <copyright file="ListEnvironments.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Lab.Client;

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ListEnvironments : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string[]> Tags { get; set; }

        public OutArgument<string[]> LabEnvironments { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var tpc = context.GetExtension<TfsTeamProjectCollection>();
            var labService = tpc.GetService<LabService>();
            var buildDetail = context.GetExtension<IBuildDetail>();
            var environments = labService.QueryLabEnvironments(
                                    new LabEnvironmentQuerySpec() { Project = buildDetail.TeamProject });

            var tags = context.GetValue(this.Tags);

            var matchingEnvironments = new List<string>();
            foreach (var environment in environments)
            {
                foreach (var tag in tags)
                {
                    var tagParts = tag.Split('=');
                    if (tagParts.Length == 2)
                    {
                        string environmentTag = null;
                        if (environment.CustomProperties.TryGetValue(tagParts[0], out environmentTag) && 
                            environmentTag.Equals(tagParts[1]))
                        {
                            matchingEnvironments.Add(environment.Name);
                        }                        
                    }
                }
            }

            context.SetValue(this.LabEnvironments, matchingEnvironments.ToArray());
        }
    }
}
