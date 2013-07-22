//-----------------------------------------------------------------------
// <copyright file="UpdateWorkItem.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Updates the work item - specifically, the Microsoft.VSTS.Build.IntegrationBuild field
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class UpdateWorkItem : CodeActivity<bool>
    {
        /// <summary>
        /// The name of the field to update in the work item
        /// </summary>
        public const string IntegrationBuildFieldRef = "Microsoft.VSTS.Build.IntegrationBuild";

        /// <summary>
        /// The work item to update
        /// </summary>
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// The build number to use
        /// </summary>
        public InArgument<string> BuildNumber { get; set; }

        /// <summary>
        /// A warning message (for when work items could not be updated)
        /// </summary>
        public OutArgument<string> WarningMessage { get; set; }

        /// <summary>
        /// A cache of already-updated work items
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InArgument<IDictionary<string, bool>> WorkItemTypeFieldCache { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            var workItem = this.WorkItem.Get(context);
            var typeDictionary = this.WorkItemTypeFieldCache.Get(context);
            bool hasIntegrationBuildField;
            lock (typeDictionary)
            {
                if (!typeDictionary.TryGetValue(workItem.Type.Name, out hasIntegrationBuildField))
                {
                    hasIntegrationBuildField = workItem.Type.FieldDefinitions.Contains(IntegrationBuildFieldRef);
                    typeDictionary.Add(workItem.Type.Name, hasIntegrationBuildField);
                }
            }

            if (hasIntegrationBuildField)
            {
                workItem.PartialOpen();
                workItem[IntegrationBuildFieldRef] = this.BuildNumber.Get(context);
                workItem.History = "The Fixed In field was updated as part of associating work items with the build.";
                if (!workItem.IsValid())
                {
                    this.WarningMessage.Set(context, CombineInvalidFields(workItem));
                    return false;
                }

                return true;
            }

            this.WarningMessage.Set(context, string.Format("TF42093: The work item {0} could not be updated with build information. The field {1} is not available on this work item.", workItem.Id, IntegrationBuildFieldRef));
            return false;
        }

        private static string CombineInvalidFields(WorkItem workItem)
        {
            var list = workItem.Validate().Cast<Field>().ToList();
            if (!list.Any())
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i != 0)
                {
                    builder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                }

                builder.Append(string.Format("Field: '{0}' Value: '{1}'", list[i].Name, list[i].Value));
            }

            return string.Format("TF42097: A work item could not be saved due to a field error. The following fields have incorrect values:{0}", builder);
        }
    }
}
