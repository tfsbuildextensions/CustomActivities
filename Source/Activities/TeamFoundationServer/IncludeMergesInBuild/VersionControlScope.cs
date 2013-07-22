//-----------------------------------------------------------------------
// <copyright file="VersionControlScope.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Workflow;
    using Microsoft.TeamFoundation.VersionControl.Client;

    internal sealed class VersionControlScope : IDisposable
    {
        public VersionControlScope(VersionControlServer server) : this(server, null)
        {
        }

        public VersionControlScope(VersionControlServer server, ActivityContext context)
        {
            this.Server = server;
            if (context != null)
            {
                this.ActionInvoker = context.GetExtension<ScheduleActionExtension>();
                if (this.ActionInvoker == null)
                {
                    throw new ServiceMissingException(typeof(ScheduleActionExtension));
                }
            }

            this.GettingHandlers = new List<Tuple<string, GettingEventHandler>>();
            this.NonFatalErrorHandlers = new List<Tuple<string, ExceptionEventHandler>>();
            this.NewPendingChangeHandlers = new List<Tuple<string, PendingChangeEventHandler>>();
        }

        public ScheduleActionExtension ActionInvoker { get; private set; }

        public List<Tuple<string, GettingEventHandler>> GettingHandlers { get; private set; }

        public List<Tuple<string, PendingChangeEventHandler>> NewPendingChangeHandlers { get; private set; }

        public List<Tuple<string, ExceptionEventHandler>> NonFatalErrorHandlers { get; private set; }

        public VersionControlServer Server { get; private set; }

        public void RegisterGettingBookmark(string bookmarkName)
        {
            if (this.ActionInvoker == null)
            {
                throw new InvalidOperationException();
            }

            GettingEventHandler handler = delegate(object sender, GettingEventArgs e)
            {
                this.ActionInvoker.ScheduleAction(bookmarkName, sender, e);
            };
            this.GettingHandlers.Add(new Tuple<string, GettingEventHandler>(bookmarkName, handler));
            this.Server.Getting += handler;
        }

        public void RegisterNewPendingChangeBookmark(string bookmarkName)
        {
            if (this.ActionInvoker == null)
            {
                throw new InvalidOperationException();
            }

            PendingChangeEventHandler handler = delegate(object sender, PendingChangeEventArgs e)
            {
                this.ActionInvoker.ScheduleAction(bookmarkName, sender, e);
            };
            this.NewPendingChangeHandlers.Add(new Tuple<string, PendingChangeEventHandler>(bookmarkName, handler));
            this.Server.NewPendingChange += handler;
        }

        public void RegisterNonFatalErrorBookmark(string bookmarkName)
        {
            if (this.ActionInvoker == null)
            {
                throw new InvalidOperationException();
            }

            ExceptionEventHandler handler = delegate(object sender, ExceptionEventArgs e)
            {
                this.ActionInvoker.ScheduleAction(bookmarkName, sender, e);
            };
            this.NonFatalErrorHandlers.Add(new Tuple<string, ExceptionEventHandler>(bookmarkName, handler));
            this.Server.NonFatalError += handler;
        }

        void IDisposable.Dispose()
        {
            this.Server.Canceled = false;
            foreach (Tuple<string, GettingEventHandler> tuple in this.GettingHandlers)
            {
                this.Server.Getting -= tuple.Item2;
                this.ActionInvoker.Shutdown(tuple.Item1);
            }

            foreach (Tuple<string, ExceptionEventHandler> tuple2 in this.NonFatalErrorHandlers)
            {
                this.Server.NonFatalError -= tuple2.Item2;
                this.ActionInvoker.Shutdown(tuple2.Item1);
            }

            foreach (Tuple<string, PendingChangeEventHandler> tuple3 in this.NewPendingChangeHandlers)
            {
                this.Server.NewPendingChange -= tuple3.Item2;
                this.ActionInvoker.Shutdown(tuple3.Item1);
            }

            this.ActionInvoker = null;
            this.Server = null;
            this.GettingHandlers.Clear();
            this.NonFatalErrorHandlers.Clear();
            this.NewPendingChangeHandlers.Clear();
        }
    }
}
