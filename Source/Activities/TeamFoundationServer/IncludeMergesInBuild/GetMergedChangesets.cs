//-----------------------------------------------------------------------
// <copyright file="GetMergedChangesets.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Gets merged changesets for the list of changesets passed in.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetMergedChangesets : AsyncCodeActivity<IEnumerable<Changeset>>
    {
        /// <summary>
        /// Version Control server object
        /// </summary>
        [RequiredArgument]
        public InArgument<VersionControlServer> VersionControlServer { get; set; }

        /// <summary>
        /// List of changesets to find merges for
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [RequiredArgument]
        public InArgument<IEnumerable<Changeset>> AssociatedChangesets { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var func = new Func<VersionControlScope, IEnumerable<Changeset>, IEnumerable<Changeset>>(this.RunCommand);
            context.UserState = func;
            return func.BeginInvoke(new VersionControlScope(this.VersionControlServer.Get(context)), this.AssociatedChangesets.Get(context), callback, state);
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddDefaultExtensionProvider(() => new ScheduleActionExtension());
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            var server = this.VersionControlServer.Get(context);
            if (server != null)
            {
                server.Canceled = true;
            }
        }

        protected override IEnumerable<Changeset> EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            IEnumerable<Changeset> enumerable = null;
            try
            {
                enumerable = ((Func<VersionControlScope, IEnumerable<Changeset>, IEnumerable<Changeset>>)context.UserState).EndInvoke(result);
            }
            catch (OperationCanceledException)
            {
            }

            return enumerable;
        }

        private IEnumerable<Changeset> RunCommand(VersionControlScope versionControlScope, IEnumerable<Changeset> associatedChangesets)
        {
            using (versionControlScope)
            {
                var merges = this.GetMergesForChangesets(versionControlScope.Server, associatedChangesets);
                return merges.Except(associatedChangesets);
            }
        }

        private IEnumerable<Changeset> GetMergesForChangesets(VersionControlServer versioncontrolServer, IEnumerable<Changeset> changesets, int recursionLevel = 0)
        {
			recursionLevel++;
            var list = new List<Changeset>();

            if (changesets == null || !changesets.Any() || recursionLevel == 10)
            {
                return list;
            }

            // at this stage, the changeset objects do not include changes
            // get them to "refresh" the changes
            var fullChangesets = changesets.Select(c => !c.Changes.Any() ? versioncontrolServer.GetChangeset(c.ChangesetId) : c).ToList();

            var mergeChangesets = fullChangesets.Where(s => s.Changes.Any(c => c.ChangeType.HasFlag(ChangeType.Merge)));
            foreach (var changeset in mergeChangesets)
            {
                changeset.Changes.Where(c => c.ChangeType.HasFlag(ChangeType.Merge)).ToList().ForEach(change =>
                {
                    var merges = versioncontrolServer.QueryMerges(null, null, change.Item.ServerItem, new ChangesetVersionSpec(changeset.ChangesetId), new ChangesetVersionSpec(changeset.ChangesetId), null, RecursionType.Full).Where(c => c.TargetVersion == changeset.ChangesetId).ToList();

                    var mergeSets = new List<Changeset>();
                    foreach (var merge in merges)
                    {
                        if (!list.Exists(c => c.ChangesetId == merge.SourceVersion))
                        {
                            var mergeSource = versioncontrolServer.GetChangeset(merge.SourceVersion);
                            list.Add(mergeSource);
                            mergeSets.Add(mergeSource);
                        }
                    }

                    list.AddRange(GetMergesForChangesets(versioncontrolServer, mergeSets, recursionLevel));
                });
            }

            return list.Except(changesets);
        }
    }
}
