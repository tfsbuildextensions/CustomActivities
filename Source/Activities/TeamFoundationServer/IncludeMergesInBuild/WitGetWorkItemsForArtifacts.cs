//-----------------------------------------------------------------------
// <copyright file="WitGetWorkItemsForArtifacts.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
	using Microsoft.TeamFoundation.Build.Client;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Gets a list of work items for the list of artifacts passed in (usually these are changesets).
	/// </summary>
	[BuildActivity(HostEnvironmentOption.All)]
	public sealed class WitGetWorkItemsForArtifacts : AsyncCodeActivity<IList<int>>
	{
		/// <summary>
		/// Artifacts to find associated work items for
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[RequiredArgument]
		public InArgument<IEnumerable<Uri>> ArtifactUris { get; set; }

		/// <summary>
		/// The date to use for finding associations
		/// </summary>
		public InArgument<DateTime> AsOfDate { get; set; }

		/// <summary>
		/// The work item store
		/// </summary>
		[RequiredArgument]
		public InArgument<WorkItemStore> WorkItemStore { get; set; }

		protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
		{
			var curState = new WitGetWorkItemsForArtifactsAsyncState();
			curState.Action = new Func<WitGetWorkItemsForArtifactsAsyncState, IList<int>>(RunCommand);
			curState.ArtifactUris = ArtifactUris.Get(context);
			curState.AsOfDate = AsOfDate.Get(context);
			curState.WorkItemStore = WorkItemStore.Get(context);
			var arg = curState;
			context.UserState = arg;
			return arg.Action.BeginInvoke(arg, callback, state);
		}

		protected override void Cancel(AsyncCodeActivityContext context)
		{
			((WitGetWorkItemsForArtifactsAsyncState)context.UserState).Canceled = true;
		}

		protected override IList<int> EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
		{
			return ((WitGetWorkItemsForArtifactsAsyncState)context.UserState).Action.EndInvoke(result);
		}

		private IList<int> RunCommand(WitGetWorkItemsForArtifactsAsyncState asyncState)
		{
			var source = new HashSet<int>();
			var artifactUriList = asyncState.ArtifactUris.Select(x => x.AbsoluteUri).ToArray<string>();
			foreach (var pair in asyncState.WorkItemStore.GetWorkItemIdsForArtifactUris(artifactUriList, asyncState.AsOfDate))
			{
				if (asyncState.Canceled)
				{
					break;
				}
				if (pair.Value != null)
				{
					source.UnionWith(pair.Value);
				}
			}
			return source.ToArray();
		}

	}
}
