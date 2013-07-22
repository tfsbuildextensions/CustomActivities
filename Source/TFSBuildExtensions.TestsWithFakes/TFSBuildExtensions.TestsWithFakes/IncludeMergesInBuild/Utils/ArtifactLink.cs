//-----------------------------------------------------------------------
// <copyright file="AtrifactLink.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Represents an artifact link. Used for testing only.
	/// </summary>
	internal sealed class ArtifactLink
	{
		/// <summary>
		/// Uri of the link
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		/// List of work item id's associated with this link
		/// </summary>
		public List<int> AssociatedWorkItems { get; set; }

		public ArtifactLink()
		{
			AssociatedWorkItems = new List<int>();
		}
	}
}
