//-----------------------------------------------------------------------
// <copyright file="WorkItemInfo.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.IncludeMergesInBuild.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents work item information. Used for testing only.
	/// </summary>
	internal sealed class WorkItemInfo
	{
		/// <summary>
		/// Id of the Work Item
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// Parent Id of the work item
		/// </summary>
		public int ParentId { get; set; }

		/// <summary>
		/// History field of the work item
		/// </summary>
		public string History { get; set; }

		/// <summary>
		/// IntegratedIn field of the work item
		/// </summary>
		public string IntegratedIn { get; set; }
	}
}
