//-----------------------------------------------------------------------
// <copyright file="ItemInfo.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils
{
	using Microsoft.TeamFoundation.VersionControl.Client;
	using Microsoft.TeamFoundation.VersionControl.Client.Fakes;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Represents an item in Source Control. Used for testing only.
	/// </summary>
	internal sealed class ItemInfo
	{
		/// <summary>
		/// Source control path for the item
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Change type for this item
		/// </summary>
		public ChangeType ChangeType { get; set; }

		/// <summary>
		/// VersionId of this item
		/// </summary>
		public int VersionId { get; set; }

		/// <summary>
		/// Merge sources for this item (if any)
		/// </summary>
		public List<int> MergeSourcesIds { get; set; }

		/// <summary>
		/// Convert this item into a Change object value
		/// </summary>
		/// <returns>Microsoft.TeamFoundation.VersionControl.Client.Change</returns>
		internal Change ToChange()
		{
			return new ShimChange()
			{
				ItemGet = () => new ShimItem()
				{
					ServerItemGet = () => Path
				},
				ChangeTypeGet = () => ChangeType,
			};
		}
	}
}
