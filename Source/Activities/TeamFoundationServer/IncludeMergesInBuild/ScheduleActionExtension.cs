//-----------------------------------------------------------------------
// <copyright file="ScheduleActionExtension.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;

    internal sealed class ScheduleActionExtension : IWorkflowInstanceExtension
    {
        private static readonly object ShutdownEvent = new object();
        private readonly TimeSpan waitTimeout = TimeSpan.FromMinutes(5.0);

        private WorkflowInstanceProxy Owner { get; set; }

        public void ScheduleAction<T>(string bookmarkName, T item)
        {
            this.Owner.BeginResumeBookmark(new Bookmark(bookmarkName), item, this.waitTimeout, this.OnBookmarkResumed, null);
        }

        public void ScheduleAction<T1, T2>(string bookmarkName, T1 item1, T2 item2)
        {
            this.Owner.BeginResumeBookmark(new Bookmark(bookmarkName), new Tuple<T1, T2>(item1, item2), this.waitTimeout, this.OnBookmarkResumed, null);
        }

        public void Shutdown(string bookmarkName)
        {
            this.Owner.BeginResumeBookmark(new Bookmark(bookmarkName), ShutdownEvent, TimeSpan.MaxValue, this.OnBookmarkResumed, null);
        }

        IEnumerable<object> IWorkflowInstanceExtension.GetAdditionalExtensions()
        {
            return null;
        }

        void IWorkflowInstanceExtension.SetInstance(WorkflowInstanceProxy instance)
        {
            this.Owner = instance;
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnBookmarkResumed(IAsyncResult result)
        {
            try
            {
                this.Owner.EndResumeBookmark(result);
            }
            catch
            {
            }
        }
    }
}
