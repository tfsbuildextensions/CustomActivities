//-----------------------------------------------------------------------
// <copyright file="GetParentId.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetParentId : CodeActivity<int>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InArgument<IEnumerable<WorkItemLinkInfo>> ParentChildLinks { get; set; }

        public InArgument<WorkItem> WorkItem { get; set; }

        protected override int Execute(CodeActivityContext context)
        {
            var workItem = this.WorkItem.Get(context);
            var source = this.ParentChildLinks.Get(context);

            var sourceId = 0;
            if (workItem != null && source != null)
            {
                var parentLink = source.FirstOrDefault(w => w.TargetId == workItem.Id);
                sourceId = parentLink.SourceId;
            }

            return sourceId;
        }
    }
}
