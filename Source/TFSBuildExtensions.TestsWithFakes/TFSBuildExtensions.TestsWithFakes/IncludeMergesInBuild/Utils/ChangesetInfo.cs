//-----------------------------------------------------------------------
// <copyright file="ChangesetInfo.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils
{
	using Microsoft.TeamFoundation.VersionControl.Client;
	using Microsoft.TeamFoundation.VersionControl.Client.Fakes;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents a changeset. Used for testing only.
	/// </summary>
	internal sealed class ChangesetInfo
	{
		/// <summary>
		/// Id of the changeset
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// List of associated work items for this changeset
		/// </summary>
		public List<WorkItemInfo> AssociatedWorkItems  { get; set; }

		/// <summary>
		/// List of changes in this changeset
		/// </summary>
		public List<ItemInfo> Changes { get; set; }

		/// <summary>
		/// Convert to a changeset object.
		/// </summary>
		/// <returns>Microsoft.TeamFoundation.VersionControl.Client.Changeset</returns>
		public Changeset ToChangeset()
		{
			return new ShimChangeset()
			{
				ChangesGet = () => Changes.ConvertAll(c => c.ToChange()).ToArray(),
				ChangesetIdGet = () => Id
			};
		}
	}
}
