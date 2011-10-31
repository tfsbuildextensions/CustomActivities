//-----------------------------------------------------------------------
// <copyright file="Iis7Application.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Web
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.Web.Administration;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// IIS7ApplicationAction
    /// </summary>
    public enum IIS7ApplicationAction
    {
        /// <summary>
        /// Delete
        /// </summary>
        Delete,

        /// <summary>
        /// CheckExists
        /// </summary>
        CheckExists    
    }

    /// <summary>
    /// Iis7Application
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Iis7Application : BaseRemoteCodeActivity
    {
        private ServerManager iisServerManager;
        private Site website;
        private IIS7ApplicationAction action = IIS7ApplicationAction.Delete;

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public IIS7ApplicationAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Sets the name of the Website
        /// </summary>
        [RequiredArgument]
        public string Website { get; set; }

        /// <summary>
        /// ITaskItem of Applications
        /// </summary>
        public InArgument<IEnumerable<string>> Applications { get; set; }

        /// <summary>
        /// Gets whether the application exists
        /// </summary>
        public OutArgument<bool> Exists { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            try
            {
                this.iisServerManager = System.Environment.MachineName != this.MachineName.Get(this.ActivityContext) ? ServerManager.OpenRemote(this.MachineName.Get(this.ActivityContext)) : new ServerManager();
                if (!this.SiteExists())
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Website, this.MachineName.Get(this.ActivityContext)));
                    return;
                }

                switch (this.action)
                {
                    case IIS7ApplicationAction.Delete:
                        this.Delete();
                        break;
                    case IIS7ApplicationAction.CheckExists:
                        this.CheckExists();
                        break;
                    default:
                        throw new ArgumentException("Action not supported");
                }
            }
            finally
            {
                if (this.iisServerManager != null)
                {
                    this.iisServerManager.Dispose();
                }
            }
        }

        private void CheckExists()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking whether application: {0} exists in {1} on: {2}", this.Applications.Get(this.ActivityContext).First(), this.Website, this.MachineName.Get(this.ActivityContext)));
            this.Exists.Set(this.ActivityContext, this.ApplicationExists(this.Applications.Get(this.ActivityContext).First()));
        }

        private bool ApplicationExists(string name)
        {
            return this.website.Applications[name] != null;
        }

        private void Delete()
        {
            if (this.Applications != null)
            {
                foreach (var app in this.Applications.Get(this.ActivityContext).Where(app => this.website.Applications[app] != null))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting Application: {0}", app));
                    this.website.Applications.Remove(this.website.Applications[app]);
                }

                this.iisServerManager.CommitChanges();
            }
        }

        private bool SiteExists()
        {
            this.website = this.iisServerManager.Sites[this.Website];
            return this.website != null;
        }
    }
}