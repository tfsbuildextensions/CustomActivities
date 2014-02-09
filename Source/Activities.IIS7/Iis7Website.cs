//-----------------------------------------------------------------------
// <copyright file="Iis7Website.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Web
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.Web.Administration;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// IIS7WebsiteAction
    /// </summary>
    public enum IIS7WebsiteAction
    {
        /// <summary>
        /// AddApplication
        /// </summary>
        AddApplication,

        /// <summary>
        /// AddVirtualDirectory
        /// </summary>
        AddVirtualDirectory,

        /// <summary>
        /// CheckExists
        /// </summary>
        CheckExists,

        /// <summary>
        /// CheckVirtualDirectoryExists
        /// </summary>
        CheckVirtualDirectoryExists,

        /// <summary>
        /// Create
        /// </summary>
        Create,

        /// <summary>
        /// Delete
        /// </summary>
        Delete,
        
        /// <summary>
        /// DeleteVirtualDirectory
        /// </summary>
        DeleteVirtualDirectory,

        /// <summary>
        /// ModifyPath
        /// </summary>
        ModifyPath,

        /// <summary>
        /// Start
        /// </summary>
        Start,

        /// <summary>
        /// Stop
        /// </summary>
        Stop
    }

    /// <summary>
    /// Iis7Website
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Iis7Website : BaseRemoteCodeActivity
    {
        private ServerManager iisServerManager;
        private Site website;
        private IIS7WebsiteAction action = IIS7WebsiteAction.Create;

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public IIS7WebsiteAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Sets the name of the Website
        /// </summary>
        [RequiredArgument]
        public string Name { get; set; }

        /// <summary>
        /// Sets the Application Name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Sets the ApplicationPhysicalPath
        /// </summary>
        public string ApplicationPhysicalPath { get; set; }

        /// <summary>
        /// Sets the ApplicationPath
        /// </summary>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// Sets the VirtualDirectoryName
        /// </summary>
        public string VirtualDirectoryName { get; set; }

        /// <summary>
        /// Sets the VirtualDirectoryApplicationPath
        /// </summary>
        public string VirtualDirectoryApplicationPath { get; set; }

        /// <summary>
        /// Sets the ApplicationPath
        /// </summary>
        public string VirtualDirectoryPhysicalPath { get; set; }

        /// <summary>
        /// Sets the path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Sets the app pool.
        /// </summary>
        public string AppPool { get; set; }

        /// <summary>
        /// Sets the Enabled Protocols for the website
        /// </summary>
        public string EnabledProtocols { get; set; }

        /// <summary>
        /// Sets the port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Set to true to force the creation of a website, even if it exists.
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Gets the site id. [Output]
        /// </summary>
        public OutArgument<long> SiteId { get; set; }

        /// <summary>
        /// Gets the site physical path. [Output]
        /// </summary>
        public OutArgument<string> PhysicalPath { get; set; }

        /// <summary>
        /// Gets whether the website exists
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
                switch (this.Action)
                {
                    case IIS7WebsiteAction.AddApplication:
                        this.AddApplication();
                        break;
                    case IIS7WebsiteAction.AddVirtualDirectory:
                        this.AddVirtualDirectory();
                        break;
                    case IIS7WebsiteAction.Create:
                        this.Create();
                        break;
                    case IIS7WebsiteAction.ModifyPath:
                        this.ModifyPath();
                        break;
                    case IIS7WebsiteAction.Delete:
                        this.Delete();
                        break;
                    case IIS7WebsiteAction.CheckExists:
                        this.CheckExists();
                        break;
                    case IIS7WebsiteAction.CheckVirtualDirectoryExists:
                        this.CheckVirtualDirectoryExists();
                        break;
                    case IIS7WebsiteAction.DeleteVirtualDirectory:
                        this.DeleteVirtualDirectory();
                        break;
                    case IIS7WebsiteAction.Stop:
                    case IIS7WebsiteAction.Start:
                        this.ControlWebsite();
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

        private void CheckVirtualDirectoryExists()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            if (!string.IsNullOrEmpty(this.VirtualDirectoryName))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking whether VirtualDirectory: {0} exists on: {1}", this.VirtualDirectoryName, this.VirtualDirectoryApplicationPath));
                if (this.website.Applications[this.ApplicationPath].VirtualDirectories.Any(v => v.Path.Equals(this.VirtualDirectoryApplicationPath, StringComparison.OrdinalIgnoreCase)))
                {
                    this.Exists.Set(this.ActivityContext, true);
                }
            }
        }

        private void DeleteVirtualDirectory()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            if (!string.IsNullOrEmpty(this.VirtualDirectoryName))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Removing VirtualDirectory: {0} from: {1}", this.VirtualDirectoryName, this.VirtualDirectoryApplicationPath));
                this.website.Applications[this.ApplicationPath].VirtualDirectories.Remove(this.website.Applications[this.ApplicationPath].VirtualDirectories[this.VirtualDirectoryName]);
                this.iisServerManager.CommitChanges();
            }
        }

        private void CheckExists()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking whether website: {0} exists on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
            this.Exists.Set(this.ActivityContext, this.SiteExists());
        }

        private void AddApplication()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            if (!string.IsNullOrEmpty(this.ApplicationName))
            {
                this.ProcessApplications();
                this.iisServerManager.CommitChanges();
            }
        }

        private void ProcessApplications()
        {
            string physicalPath = this.ApplicationPhysicalPath;
            if (!Directory.Exists(physicalPath))
            {
                Directory.CreateDirectory(physicalPath);
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding Application: {0}", this.ApplicationName));
            this.website.Applications.Add(this.ApplicationName, physicalPath);

            // Set Application Pool if given
            if (!string.IsNullOrEmpty(this.AppPool))
            {
                ApplicationPool pool = this.iisServerManager.ApplicationPools[this.AppPool];
                if (pool == null)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} specified for: {1} was not found", this.AppPool, this.ApplicationName));
                    return;
                }

                this.website.Applications[this.ApplicationName].ApplicationPoolName = this.AppPool;
            }
        }

        private void AddVirtualDirectory()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            if (!string.IsNullOrEmpty(this.VirtualDirectoryName))
            {
                this.ProcessVirtualDirectories();
                this.iisServerManager.CommitChanges();
            }
        }

        private void Delete()
        {
            if (!this.SiteExists())
            {
                this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting website: {0} on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
            this.iisServerManager.Sites.Remove(this.website);
            this.iisServerManager.CommitChanges();
        }

        private void ControlWebsite()
        {
            if (!this.SiteExists())
            {
                this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            switch (this.Action)
            {
                case IIS7WebsiteAction.Start:
                    this.website.Start();
                    break;
                case IIS7WebsiteAction.Stop:
                    this.website.Stop();
                    break;
            }
        }

        private void Create()
        {
            if (this.SiteExists())
            {
                if (!this.Force)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} already exists on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                    return;
                }

                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting website: {0} on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                this.iisServerManager.Sites.Remove(this.website);
                this.iisServerManager.CommitChanges();
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Creating website: {0} on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
            if (!Directory.Exists(this.Path))
            {
                Directory.CreateDirectory(this.Path);
            }

            this.website = this.iisServerManager.Sites.Add(this.Name, this.Path, this.Port);
            if (!string.IsNullOrEmpty(this.AppPool))
            {
                this.website.ApplicationDefaults.ApplicationPoolName = this.AppPool;
            }

            if (!string.IsNullOrEmpty(this.ApplicationName))
            {
                this.ProcessApplications();
            }

            if (!string.IsNullOrEmpty(this.VirtualDirectoryName))
            {
                this.ProcessVirtualDirectories();
            }

            if (!string.IsNullOrEmpty(this.EnabledProtocols))
            {
                this.website.ApplicationDefaults.EnabledProtocols = this.EnabledProtocols;
            }

            this.iisServerManager.CommitChanges();
            this.SiteId.Set(this.ActivityContext, this.website.Id);
        }

        private void ProcessVirtualDirectories()
        {
            string physicalPath = this.VirtualDirectoryPhysicalPath;
            if (!Directory.Exists(physicalPath))
            {
                Directory.CreateDirectory(physicalPath);
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding VirtualDirectory: {0} to: {1}", this.VirtualDirectoryName, this.VirtualDirectoryApplicationPath));
            this.website.Applications[this.ApplicationPath].VirtualDirectories.Add(this.VirtualDirectoryName, physicalPath);
        }

        private void ModifyPath()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Modifying website: {0} on: {1}", this.Name, this.MachineName.Get(this.ActivityContext)));
            if (!Directory.Exists(this.Path))
            {
                Directory.CreateDirectory(this.Path);
            }

            Application app = this.website.Applications["/"];
            if (app != null)
            {
                VirtualDirectory vdir = app.VirtualDirectories["/"];
                if (vdir != null)
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting physical path: {0} on: {1}", this.Path, vdir.Path));
                    vdir.PhysicalPath = this.Path;
                }
            }

            this.iisServerManager.CommitChanges();
            this.SiteId.Set(this.ActivityContext, this.website.Id);
        }

        private bool SiteExists()
        {
            this.website = this.iisServerManager.Sites[this.Name];
            return this.website != null;
        }
    }
}