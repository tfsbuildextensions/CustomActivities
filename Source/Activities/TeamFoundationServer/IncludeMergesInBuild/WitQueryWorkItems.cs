//-----------------------------------------------------------------------
// <copyright file="WitQueryWorkItems.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
	using Microsoft.TeamFoundation.Build.Client;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Run a query to get work items
	/// </summary>
	[BuildActivity(HostEnvironmentOption.All)]
	public sealed class WitQueryWorkItems : AsyncCodeActivity<WorkItemCollection>
	{
		/// <summary>
		/// Fields to select for the matching work items
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[RequiredArgument]
		public InArgument<IEnumerable<string>> Fields { get; set; }

		/// <summary>
		/// The list of work item id's to get
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[RequiredArgument]
		public InArgument<IEnumerable<int>> Ids { get; set; }

		/// <summary>
		/// Include parent work items
		/// </summary>
		public InArgument<bool> IncludeParentWorkItems { get; set; }

		/// <summary>
		/// Page size for the results
		/// </summary>
		public InArgument<int> PageSize { get; set; }

		/// <summary>
		/// The links from child to parent for the matching work items
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public OutArgument<IEnumerable<WorkItemLinkInfo>> ParentChildLinks { get; set; }

		/// <summary>
		/// The work item store
		/// </summary>
		[RequiredArgument]
		public InArgument<WorkItemStore> WorkItemStore { get; set; }

		public WitQueryWorkItems()
		{
			PageSize = 50;
			IncludeParentWorkItems = false;
		}

		protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
		{
			var func = new Func<WorkItemStore, IEnumerable<int>, IEnumerable<string>, int, bool, Tuple<WorkItemCollection, List<WorkItemLinkInfo>>>(RunCommand);
			context.UserState = func;
			return func.BeginInvoke(WorkItemStore.Get(context), Ids.Get(context), Fields.Get(context), PageSize.Get(context), IncludeParentWorkItems.Get(context), callback, state);
		}

		protected override WorkItemCollection EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
		{
			var tuple = ((Func<WorkItemStore, IEnumerable<int>, IEnumerable<string>, int, bool, Tuple<WorkItemCollection, List<WorkItemLinkInfo>>>)context.UserState).EndInvoke(result);
			WorkItemCollection items = tuple.Item1;
			ParentChildLinks.Set(context, tuple.Item2);
			return items;
		}

		private Tuple<WorkItemCollection, List<WorkItemLinkInfo>> RunCommand(WorkItemStore workItemStore, IEnumerable<int> ids, IEnumerable<string> fields, int pageSize, bool includeParentWorkItems)
		{
			var source = new List<WorkItemLinkInfo>();
			if (includeParentWorkItems && ids.Count() > 0)
			{
				var idArray = new[] { string.Join(", ", ids.Select(x => x.ToString())) };
				var query = string.Format("SELECT [System.Id] FROM WorkItemLinks WHERE ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') And ([Target].[System.Id] IN ({0})) ORDER BY [System.Id] mode(Recursive,ReturnMatchingChildren)",
					idArray);
				source.AddRange(new Query(workItemStore, query).RunLinkQuery());
				ids = source.Select(wi => wi.TargetId).Distinct();
			}

			var args = new[] { string.Join(", ", fields.Select(x => string.Format("[{0}]", x))) };
			var wiql = string.Format("SELECT {0} FROM WorkItems", args);
			var items = workItemStore.Query(ids.ToArray(), wiql);
			items.PageSize = Math.Max(50, Math.Min(pageSize, 200));
			return new Tuple<WorkItemCollection, List<WorkItemLinkInfo>>(items, source);
		}
	}
}
