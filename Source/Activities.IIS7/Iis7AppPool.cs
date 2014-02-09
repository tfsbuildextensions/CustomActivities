//-----------------------------------------------------------------------
// <copyright file="Iis7AppPool.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Web
{
    using System;
    using System.Activities;
    using System.Globalization;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.Web.Administration;
    
    /// <summary>
    /// IIS7AppPoolAction
    /// </summary>
    public enum IIS7AppPoolAction
    {
        /// <summary>
        /// CheckExists
        /// </summary>
        CheckExists,

        /// <summary>
        /// Create
        /// </summary>
        Create,

        /// <summary>
        /// Delete
        /// </summary>
        Delete,

        /// <summary>
        /// Modify
        /// </summary>
        Modify,

        /// <summary>
        /// Recycle
        /// </summary>
        Recycle,

        /// <summary>
        /// SetIdentity
        /// </summary>
        SetIdentity,

        /// <summary>
        /// SetPiplelineMode
        /// </summary>
        SetPipelineMode,

        /// <summary>
        /// Stop
        /// </summary>
        Stop,

        /// <summary>
        /// Start
        /// </summary>
        Start
    }

    /// <summary>
    /// Iis7AppPool
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Iis7AppPool : BaseRemoteCodeActivity
    {
        private ServerManager iisServerManager;
        private bool autoStart = true;
        private ManagedPipelineMode managedPM = ManagedPipelineMode.Integrated;
        private ProcessModelIdentityType processModelType = ProcessModelIdentityType.LocalService;
        private ApplicationPool pool;
        private IIS7AppPoolAction action = IIS7AppPoolAction.Create;

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public IIS7AppPoolAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Sets the private memory (kb) a process can use before the process is recycled. Default is 0. Set > 0 to use. Set to -1 to restore the Application Pool Default.
        /// </summary>
        public long PeriodicRestartPrivateMemory { get; set; }

        /// <summary>
        /// Sets the maximum number of requests to queue before rejecting additional requests. Default is 0. Set > 0 to use. Set to -1 to restore the Application Pool Default.
        /// </summary>
        public long QueueLength { get; set; }

        /// <summary>
        /// Sets a TimeSpan value in minutes for the period of time a process should remain idle. Set > 0 to use. Set to -1 to restore the Application Pool Default.
        /// </summary>
        public long IdleTimeout { get; set; }

        /// <summary>
        /// Sets the maximum number of worker processes allowed for the AppPool. Set to -1 to restore the Application Pool Default.
        /// </summary>
        public long MaxProcesses { get; set; }

        /// <summary>
        /// Sets a TimeSpan value in minutes for the period of time that should elapse before a worker process is recycled. Default is 29 hours. Set > 0 to use. Set to -1 to restore the Application Pool Default for Modify or -1 to Disable Recycling.PeriodicRestartTime for Create
        /// </summary>
        public long PeriodicRestartTime { get; set; }

        /// <summary>
        /// Sets the times that the application pool should recycle. Format is 'hh:mm,hh:mm,hh:mm'. Set to "-1" to clear the RecycleTimes
        /// </summary>
        public string RecycleTimes { get; set; }

        /// <summary>
        /// Sets the fixed number of requests to recycle the application pool. Set to -1 to restore the Application Pool Default.
        /// </summary>
        public int RecycleRequests { get; set; }

        /// <summary>
        /// Sets the RecycleInterval in minutes for the application pool. Set to -1 to restore the Application Pool Default.
        /// </summary>
        public int RecycleInterval { get; set; }

        /// <summary>
        /// Set whether the application pool should start automatically. Default is true.
        /// </summary>
        public bool AutoStart
        {
            get { return this.autoStart; }
            set { this.autoStart = value; }
        }

        /// <summary>
        /// Sets whether 32-bit applications are enabled on 64-bit processors. Default is false.
        /// </summary>
        public bool Enable32BitAppOnWin64 { get; set; }

        /// <summary>
        /// Sets the ProcessModelIdentityType. Default is LocalService
        /// </summary>
        public string IdentityType
        {
            get { return this.processModelType.ToString(); }
            set { this.processModelType = (ProcessModelIdentityType)Enum.Parse(typeof(ProcessModelIdentityType), value); }
        }

        /// <summary>
        /// Sets the user name associated with the security identity under which the application pool runs.
        /// </summary>
        public string PoolIdentity { get; set; }

        /// <summary>
        /// Sets the password associated with the PoolIdentity property.
        /// </summary>
        public string IdentityPassword { get; set; }

        /// <summary>
        /// Sets the version number of the .NET Framework used by the application pool. Default is "v2.0".
        /// </summary>
        public string ManagedRuntimeVersion { get; set; }

        /// <summary>
        /// Sets the ManagedPipelineMode. Default is ManagedPipelineMode.Integrated.
        /// </summary>
        public string PipelineMode
        {
            get { return this.managedPM.ToString(); }
            set { this.managedPM = (ManagedPipelineMode)Enum.Parse(typeof(ManagedPipelineMode), value); }
        }

        /// <summary>
        /// Sets the name of the AppPool
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Name { get; set; }

        /// <summary>
        /// Set to true to force the creation of an apppool, even if it exists.
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Gets whether the Application Pool exists
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
                    case IIS7AppPoolAction.Create:
                        this.Create();
                        break;
                    case IIS7AppPoolAction.Modify:
                        this.Modify();
                        break;
                    case IIS7AppPoolAction.Delete:
                        this.Delete();
                        break;
                    case IIS7AppPoolAction.CheckExists:
                        this.CheckExists();
                        break;
                    case IIS7AppPoolAction.SetIdentity:
                        this.SetIdentity();
                        break;
                    case IIS7AppPoolAction.SetPipelineMode:
                        this.SetPipelineMode();
                        break;
                    case IIS7AppPoolAction.Start:
                    case IIS7AppPoolAction.Stop:
                    case IIS7AppPoolAction.Recycle:
                        this.ControlAppPool();
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
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking whether Application Pool: {0} exists on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
            this.Exists.Set(this.ActivityContext, this.AppPoolExists());
        }

        private void SetPipelineMode()
        {
            if (!this.AppPoolExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} was not found on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Modifying Application Pool: {0} on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ManagedPipelineMode to: {0}", this.PipelineMode), BuildMessageImportance.Low);
            this.pool.ManagedPipelineMode = this.managedPM;
            this.iisServerManager.CommitChanges();
        }

        private void SetIdentity()
        {
            if (!this.AppPoolExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} was not found on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                return;
            }

            if (this.IdentityType == "SpecificUser" && (string.IsNullOrEmpty(this.PoolIdentity) || string.IsNullOrEmpty(this.IdentityPassword)))
            {
                this.LogBuildError("PoolIdentity and PoolPassword must be specified if the IdentityType is SpecificUser");
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Modifying Application Pool: {0} on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ProcessModelIdentityType to: {0}", this.IdentityType), BuildMessageImportance.Low);
            this.pool.ProcessModel.IdentityType = this.processModelType;

            if (this.IdentityType == "SpecificUser")
            {
                this.pool.ProcessModel.UserName = this.PoolIdentity;
                this.pool.ProcessModel.Password = this.IdentityPassword;
            }

            this.iisServerManager.CommitChanges();
        }

        private void Delete()
        {
            if (!this.AppPoolExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} was not found on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting Application Pool: {0} on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
            this.iisServerManager.ApplicationPools.Remove(this.pool);
            this.iisServerManager.CommitChanges();
        }

        private void ControlAppPool()
        {
            if (!this.AppPoolExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} was not found on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "{0} Application Pool: {1} on: {2}", this.Action, this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));

            switch (this.Action)
            {
                case IIS7AppPoolAction.Start:
                    this.pool.Start();
                    break;
                case IIS7AppPoolAction.Stop:
                    if (this.pool.State != ObjectState.Stopped && this.pool.State != ObjectState.Stopping)
                    {
                        this.pool.Stop();
                    }

                    break;
                case IIS7AppPoolAction.Recycle:
                    this.pool.Start();
                    this.pool.Recycle();
                    break;
            }
        }

        private void Create()
        {
            if (this.AppPoolExists())
            {
                if (!this.Force)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} already exists on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                    return;
                }

                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting Application Pool: {0} on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                this.iisServerManager.ApplicationPools.Remove(this.pool);
                this.iisServerManager.CommitChanges();
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Creating Application Pool: {0} on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));

            if (this.IdentityType == "SpecificUser" && (string.IsNullOrEmpty(this.PoolIdentity) || string.IsNullOrEmpty(this.IdentityPassword)))
            {
                this.LogBuildError("PoolIdentity and PoolPassword must be specified if the IdentityType is SpecificUser");
                return;
            }

            this.pool = this.iisServerManager.ApplicationPools.Add(this.Name.Get(this.ActivityContext));
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ManagedPipelineMode to: {0}", this.PipelineMode), BuildMessageImportance.Low);
            this.pool.ManagedPipelineMode = this.managedPM;
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ProcessModelIdentityType to: {0}", this.IdentityType), BuildMessageImportance.Low);
            this.pool.ProcessModel.IdentityType = this.processModelType;
            if (this.IdentityType == "SpecificUser")
            {
                this.pool.ProcessModel.UserName = this.PoolIdentity;
                this.pool.ProcessModel.Password = this.IdentityPassword;
            }

            this.SetCommonInfo();
            this.iisServerManager.CommitChanges();
        }

        private void Modify()
        {
            if (!this.AppPoolExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The Application Pool: {0} was not found on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Modifying Application Pool: {0} on: {1}", this.Name.Get(this.ActivityContext), this.MachineName.Get(this.ActivityContext)));
            this.SetCommonInfo();
            this.iisServerManager.CommitChanges();
        }

        private void SetCommonInfo()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting AutoStart to: {0}", this.AutoStart), BuildMessageImportance.Low);
            this.pool.AutoStart = this.AutoStart;
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Enable32BitAppOnWin64 to: {0}", this.Enable32BitAppOnWin64), BuildMessageImportance.Low);
            this.pool.Enable32BitAppOnWin64 = this.Enable32BitAppOnWin64;

            if (!string.IsNullOrEmpty(this.ManagedRuntimeVersion))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ManagedRuntimeVersion to: {0}", this.ManagedRuntimeVersion), BuildMessageImportance.Low);
                this.pool.ManagedRuntimeVersion = this.ManagedRuntimeVersion.ToUpperInvariant() == "NO MANAGED CODE" ? string.Empty : this.ManagedRuntimeVersion;
            }

            if (this.QueueLength > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting QueueLength to: {0}", this.QueueLength), BuildMessageImportance.Low);
                this.pool.QueueLength = this.QueueLength;
            }
            else if (this.QueueLength == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting QueueLength to: {0}", this.iisServerManager.ApplicationPoolDefaults.QueueLength));
                this.pool.QueueLength = this.iisServerManager.ApplicationPoolDefaults.QueueLength;
            }

            if (this.IdleTimeout > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting IdleTimeout to: {0} minutes", this.IdleTimeout), BuildMessageImportance.Low);
                this.pool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes(this.IdleTimeout);
            }
            else if (this.IdleTimeout == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting IdleTimeout to: {0}", this.iisServerManager.ApplicationPoolDefaults.ProcessModel.IdleTimeout), BuildMessageImportance.Low);
                this.pool.ProcessModel.IdleTimeout = this.iisServerManager.ApplicationPoolDefaults.ProcessModel.IdleTimeout;
            }

            if (this.PeriodicRestartPrivateMemory > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.PrivateMemory to: {0}", this.PeriodicRestartPrivateMemory), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.PrivateMemory = this.PeriodicRestartPrivateMemory;
            }
            else if (this.PeriodicRestartPrivateMemory == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.PrivateMemory to: {0}", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.PrivateMemory), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.PrivateMemory = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.PrivateMemory;
            }

            if (this.PeriodicRestartTime > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestartTime to: {0} minutes", this.PeriodicRestartTime), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Time = TimeSpan.FromMinutes(this.PeriodicRestartTime);
            }
            else if (this.PeriodicRestartTime == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestartTime to: {0} minutes", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Time = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time;
            }

            if (this.MaxProcesses > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ProcessModel.MaxProcesses to: {0}", this.MaxProcesses), BuildMessageImportance.Low);
                this.pool.ProcessModel.MaxProcesses = this.MaxProcesses;
            }
            else if (this.MaxProcesses == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting ProcessModel.MaxProcesses to: {0}", this.MaxProcesses), BuildMessageImportance.Low);
                this.pool.ProcessModel.MaxProcesses = this.iisServerManager.ApplicationPoolDefaults.ProcessModel.MaxProcesses;
            }

            if (!string.IsNullOrEmpty(this.RecycleTimes))
            {
                string[] times = this.RecycleTimes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string time in times)
                {
                    double hours = Convert.ToDouble(time.Split(new[] { ':' })[0], CultureInfo.CurrentCulture);
                    double minutes = Convert.ToDouble(time.Split(new[] { ':' })[1], CultureInfo.CurrentCulture);
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.Schedule to: {0}:{1}", hours, minutes));
                    this.pool.Recycling.PeriodicRestart.Schedule.Add(TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes)));
                }
            }
            else if (this.RecycleTimes == "-1")
            {
                this.LogBuildMessage("Clearing the Recycling.PeriodicRestart.Schedule", BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Schedule.Clear();
            }

            if (this.RecycleRequests > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.RecycleRequests to: {0}", this.RecycleRequests), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Requests = this.RecycleRequests;
            }
            else if (this.RecycleRequests == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.RecycleRequests to: {0}", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Requests), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Requests = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Requests;
            }

            if (this.RecycleInterval > 0)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.Time to: {0}", this.RecycleInterval), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Time = TimeSpan.FromMinutes(this.RecycleInterval);
            }
            else if (this.RecycleInterval == -1)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.Time to: {0}", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time), BuildMessageImportance.Low);
                this.pool.Recycling.PeriodicRestart.Time = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time;
            }
        }

        private bool AppPoolExists()
        {
            this.pool = this.iisServerManager.ApplicationPools[this.Name.Get(this.ActivityContext)];
            return this.pool != null;
        }
    }
}