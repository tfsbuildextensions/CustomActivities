//-----------------------------------------------------------------------
// <copyright file="FakeUtils.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils
{
	using Microsoft.TeamFoundation.VersionControl.Client;
	using Microsoft.TeamFoundation.VersionControl.Client.Fakes;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using Microsoft.TeamFoundation.WorkItemTracking.Client.Fakes;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal class FakeUtils
	{
		internal static VersionControlServer GetFakeVCServer(List<ChangesetInfo> changeSetHierarchy)
		{
			var server = new ShimVersionControlServer()
			{
				QueryMergesStringVersionSpecStringVersionSpecVersionSpecVersionSpecRecursionType = (sourcePath, sourceVersion, targetPath, targetVersion, versionFrom, versionTo, recursion) =>
					{
						var mergeIds = from set in changeSetHierarchy
									   from c in set.Changes
									   where c.ChangeType.HasFlag(ChangeType.Merge) && c.Path == targetPath &&
										   c.VersionId == ((ChangesetVersionSpec)versionFrom).ChangesetId
									   select c.MergeSourcesIds;
						var flatList = new List<int>();
						foreach (var list in mergeIds)
						{
							flatList = flatList.Union(list).ToList();
						}
						return flatList.ConvertAll(i => (ChangesetMerge)new ShimChangesetMerge() { SourceVersionGet = () => i, TargetVersionGet = () => ((ChangesetVersionSpec)versionFrom).ChangesetId }).ToArray();
					},
					GetChangesetInt32 = (i) => changeSetHierarchy.SingleOrDefault(c => c.Id == i).ToChangeset()
			};
			return server;
		}

		internal static ChangesetInfo CreateChangeset(int id, List<int> mergeSources = null)
		{
			var change = new ItemInfo()
			{
				Path = "Test.cs",
				VersionId = id
			};
			if (mergeSources != null)
			{
				change.MergeSourcesIds = mergeSources;
				change.ChangeType = ChangeType.Merge | ChangeType.Edit;
			}
			else
			{
				change.ChangeType = ChangeType.Edit;
			}

			var targetChangeset = new ChangesetInfo()
			{
				Id = id,
				AssociatedWorkItems = new List<WorkItemInfo>()
					{
						new WorkItemInfo() { Id = id, History = "" }
					},
				Changes = new List<ItemInfo>() { change }
			};
			return targetChangeset;
		}

		internal static WorkItemStore GetFakeWorkItemStore(List<int> savedIds)
		{
			return new ShimWorkItemStore()
			{
				BatchSaveWorkItemArraySaveFlags = (arr, f) =>
					{
						var list = new List<BatchSaveError>();
						foreach(var wi in arr.Where(w => w.Id < 0))
						{
							list.Add(new ShimBatchSaveError() { WorkItemGet = () => wi, ExceptionGet = () => new ApplicationException("Error") });
						}
						if (list.Count == 0)
						{
							savedIds.AddRange(arr.Where(w => w.Id > 0).Select(w => w.Id));
						}
						return list.ToArray();
					}
			};
		}

		internal static WorkItemStore GetFakeWorkItemStore(List<ArtifactLink> artifactLinks)
		{
			return new ShimWorkItemStore()
			{
				GetWorkItemIdsForArtifactUrisStringArrayDateTime = (uris, d) =>
					{
						var list = from a in artifactLinks
								   where uris.Contains(a.Uri.AbsoluteUri)
								   select new
								   {
									   Uri = a.Uri,
									   Links = a.AssociatedWorkItems
								   };
						var dict = new Dictionary<string, int[]>();
						foreach (var i in list)
						{
							dict.Add(i.Uri.AbsolutePath, i.Links.ToArray());
						}
						return dict;
					}
			};
		}

		internal static WorkItemStore GetFakeWorkItemStore(List<WorkItemInfo> workItems)
		{
			ShimQuery.ConstructorWorkItemStoreString = (q, store, wiql) =>
			{
				var fakeQ = new ShimQuery(q) 
				{
					WorkItemStoreGet = () => store,
					QueryStringGet = () => wiql,
					RunLinkQuery = () =>
					{
						// extract the targetIds from the wiql
						var start = q.QueryString.IndexOf("IN (") + 4;
						var end = q.QueryString.IndexOf(")", start);
						var idArgs = q.QueryString.Substring(start, end - start);
						var ids = idArgs.Split(new[] { ", " }, StringSplitOptions.None).ToList().ConvertAll(s => int.Parse(s));

						var children = from w in workItems
									   where ids.Contains(w.Id)
									   select new WorkItemLinkInfo()
									   {
										   LinkTypeId = w.ParentId == 0 ? 0 : 2,
										   SourceId = w.ParentId,
										   TargetId = w.Id
									   };
						var parents = from w in children
									  where w.SourceId != 0
									  select new WorkItemLinkInfo()
									  {
										  LinkTypeId = 0,
										  SourceId = 0,
										  TargetId = w.SourceId
									  };
						return children.Union(parents).ToArray();
					}
				};
			};

			return new ShimWorkItemStore()
			{
				QueryInt32ArrayString = (ids, query) =>
					{
						var col = new ShimWorkItemCollection() { CountGet = () => workItems.Where(w => ids.Contains(w.Id)).Count() };
						col.Bind(workItems.Where(w => ids.Contains(w.Id)).ToList().ConvertAll(w => (WorkItem)new ShimWorkItem() { IdGet = () => w.Id }));
						return col;
					}
			};
		}
	}
}
