//-----------------------------------------------------------------------
// <copyright file="ScheduledActionExtension.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
	using System;
	using System.Activities;
	using System.Activities.Hosting;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal sealed class ScheduleActionExtension : IWorkflowInstanceExtension
	{
		private WorkflowInstanceProxy Owner { get; set; }
		private TimeSpan waitTimeout = TimeSpan.FromMinutes(5.0);

		internal static object ShutdownEvent = new object();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void OnBookmarkResumed(IAsyncResult result)
		{
			try
			{
				Owner.EndResumeBookmark(result);
			}
			catch
			{
			}
		}

		public void ScheduleAction<T>(string bookmarkName, T item)
		{
			Owner.BeginResumeBookmark(new Bookmark(bookmarkName), item, waitTimeout, new AsyncCallback(OnBookmarkResumed), null);
		}

		public void ScheduleAction<T1, T2>(string bookmarkName, T1 item1, T2 item2)
		{
			Owner.BeginResumeBookmark(new Bookmark(bookmarkName), new Tuple<T1, T2>(item1, item2), waitTimeout, new AsyncCallback(OnBookmarkResumed), null);
		}

		public void Shutdown(string bookmarkName)
		{
			Owner.BeginResumeBookmark(new Bookmark(bookmarkName), ShutdownEvent, TimeSpan.MaxValue, new AsyncCallback(OnBookmarkResumed), null);
		}

		IEnumerable<object> IWorkflowInstanceExtension.GetAdditionalExtensions()
		{
			return null;
		}

		void IWorkflowInstanceExtension.SetInstance(WorkflowInstanceProxy instance)
		{
			Owner = instance;
		}
	}
}
