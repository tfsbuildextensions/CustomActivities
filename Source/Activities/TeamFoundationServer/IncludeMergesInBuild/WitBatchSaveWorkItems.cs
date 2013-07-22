//-----------------------------------------------------------------------
// <copyright file="WitBatchSaveWorkItems.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
	using Microsoft.TeamFoundation.Build.Client;
	using Microsoft.TeamFoundation.Common;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Save a batch of work item changes.
	/// </summary>
	[BuildActivity(HostEnvironmentOption.All)]
	public sealed class WitBatchSaveWorkItems : CodeActivity<IList<BatchSaveError>>
	{
		/// <summary>
		/// The updated work items to save
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), RequiredArgument]
		public InArgument<IEnumerable<WorkItem>> WorkItems { get; set; }

		protected override IList<BatchSaveError> Execute(CodeActivityContext context)
		{
			var workItems = WorkItems.Get(context);
			var errors = new List<BatchSaveError>();

			TFCommonUtil.CheckForNull(workItems, "WorkItems");
			if (workItems.Count() > 0)
			{
				errors.AddRange(workItems.First().Store.BatchSave(workItems.ToArray(), SaveFlags.None));
			}
			return errors;
		}
	}
}
