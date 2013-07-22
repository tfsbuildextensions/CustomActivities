//-----------------------------------------------------------------------
// <copyright file="WitGetWorkItemsForArtifactsAsyncState.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
	using System;
	using System.Collections.Generic;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;

	/// <summary>
	/// Data for the async WitGetWorkItemsForArtifacts task.
	/// </summary>
	public sealed class WitGetWorkItemsForArtifactsAsyncState
	{
		/// <summary>
		/// The async action to execute
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Func<WitGetWorkItemsForArtifactsAsyncState, IList<int>> Action { get; set; }

		/// <summary>
		/// List of artifact Uri's
		/// </summary>
		public IEnumerable<Uri> ArtifactUris { get; set; }

		/// <summary>
		/// The date to use in the lookup
		/// </summary>
		public DateTime AsOfDate { get; set; }

		/// <summary>
		/// Flag to indicate a cancellation of the async operation
		/// </summary>
		public bool Canceled { get; set; }

		/// <summary>
		/// The work item store
		/// </summary>
		public WorkItemStore WorkItemStore { get; set; }
	}
}
