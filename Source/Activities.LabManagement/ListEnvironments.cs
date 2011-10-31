//-----------------------------------------------------------------------
// <copyright file="ListEnvironments.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Lab.Client;

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ListEnvironments : CodeActivity
    {
        // TODO: get build info        
        [RequiredArgument]
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        public OutArgument<List<LabEnvironment>> LabEnvironments { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            // string text = context.GetValue(this.Text)
            var tpc = context.GetExtension<TfsTeamProjectCollection>();
            var buildDefinition = context.GetExtension<IBuildDetail>().BuildDefinition;

            string serverUri = @"http://server:8080/tfs/CollectionName"; // Specify the tfs uri
            TfsTeamProjectCollection teamFoundationServer = new TfsTeamProjectCollection(TfsTeamProjectCollection.GetFullyQualifiedUriForName(serverUri));
            LabService labService = teamFoundationServer.GetService<LabService>();

            LabEnvironmentQuerySpec spec = new LabEnvironmentQuerySpec();
            spec.Project = "Project1"; // Specify the project name.

            ICollection<LabEnvironment> environments = labService.QueryLabEnvironments(spec);
            List<LabEnvironment> filteredEnvironments = new List<LabEnvironment>();
            foreach (LabEnvironment le in environments)
            {
                string os = null;
                if (le.CustomProperties.TryGetValue("OS", out os) && os.Equals("OS Name"))
                {
                    filteredEnvironments.Add(le);
                }
            }

            context.SetValue<List<LabEnvironment>>(this.LabEnvironments, filteredEnvironments);
        }
    }
}
