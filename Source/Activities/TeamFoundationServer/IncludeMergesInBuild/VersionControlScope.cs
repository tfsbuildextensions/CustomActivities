//-----------------------------------------------------------------------
// <copyright file="VersionControlScope.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild
{
	using Microsoft.TeamFoundation.Build.Workflow;
	using Microsoft.TeamFoundation.VersionControl.Client;
	using System;
	using System.Activities;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal sealed class VersionControlScope : IDisposable
	{
		public ScheduleActionExtension ActionInvoker { get; private set; }

		public List<Tuple<string, GettingEventHandler>> GettingHandlers { get; private set; }

		public List<Tuple<string, PendingChangeEventHandler>> NewPendingChangeHandlers { get; private set; }

		public List<Tuple<string, ExceptionEventHandler>> NonFatalErrorHandlers { get; private set; }

		public VersionControlServer Server { get; private set; }

		public VersionControlScope(VersionControlServer server)
			: this(server, null)
		{
		}

		public VersionControlScope(VersionControlServer server, ActivityContext context)
		{
			Server = server;
			if (context != null)
			{
				ActionInvoker = context.GetExtension<ScheduleActionExtension>();
				if (ActionInvoker == null)
				{
					throw new ServiceMissingException(typeof(ScheduleActionExtension));
				}
			}
			GettingHandlers = new List<Tuple<string, GettingEventHandler>>();
			NonFatalErrorHandlers = new List<Tuple<string, ExceptionEventHandler>>();
			NewPendingChangeHandlers = new List<Tuple<string, PendingChangeEventHandler>>();
		}

		public void RegisterGettingBookmark(string bookmarkName)
		{
			if (ActionInvoker == null)
			{
				throw new InvalidOperationException();
			}
			GettingEventHandler handler = delegate(object sender, GettingEventArgs e)
			{
				ActionInvoker.ScheduleAction<object, GettingEventArgs>(bookmarkName, sender, e);
			};
			GettingHandlers.Add(new Tuple<string, GettingEventHandler>(bookmarkName, handler));
			Server.Getting += handler;
		}

		public void RegisterNewPendingChangeBookmark(string bookmarkName)
		{
			if (ActionInvoker == null)
			{
				throw new InvalidOperationException();
			}
			PendingChangeEventHandler handler = delegate(object sender, PendingChangeEventArgs e)
			{
				ActionInvoker.ScheduleAction<object, PendingChangeEventArgs>(bookmarkName, sender, e);
			};
			NewPendingChangeHandlers.Add(new Tuple<string, PendingChangeEventHandler>(bookmarkName, handler));
			Server.NewPendingChange += handler;
		}

		public void RegisterNonFatalErrorBookmark(string bookmarkName)
		{
			if (ActionInvoker == null)
			{
				throw new InvalidOperationException();
			}
			ExceptionEventHandler handler = delegate(object sender, ExceptionEventArgs e)
			{
				ActionInvoker.ScheduleAction<object, ExceptionEventArgs>(bookmarkName, sender, e);
			};
			NonFatalErrorHandlers.Add(new Tuple<string, ExceptionEventHandler>(bookmarkName, handler));
			Server.NonFatalError += handler;
		}

		void IDisposable.Dispose()
		{
			Server.Canceled = false;
			foreach (Tuple<string, GettingEventHandler> tuple in GettingHandlers)
			{
				Server.Getting -= tuple.Item2;
				ActionInvoker.Shutdown(tuple.Item1);
			}
			foreach (Tuple<string, ExceptionEventHandler> tuple2 in NonFatalErrorHandlers)
			{
				Server.NonFatalError -= tuple2.Item2;
				ActionInvoker.Shutdown(tuple2.Item1);
			}
			foreach (Tuple<string, PendingChangeEventHandler> tuple3 in NewPendingChangeHandlers)
			{
				Server.NewPendingChange -= tuple3.Item2;
				ActionInvoker.Shutdown(tuple3.Item1);
			}

			ActionInvoker = null;
			Server = null;
			GettingHandlers.Clear();
			NonFatalErrorHandlers.Clear();
			NewPendingChangeHandlers.Clear();
		}
	}
}
