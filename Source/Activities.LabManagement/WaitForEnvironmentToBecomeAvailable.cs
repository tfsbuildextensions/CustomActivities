//-----------------------------------------------------------------------
// <copyright file="WaitForEnvironmentToBecomeAvailable.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Lab.Client;

    /// <summary>
    /// Provides an activity that can be used to wait until an environment because available to be
    /// used
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class WaitForEnvironmentToBecomeAvailable : BaseCodeActivity
    {
        /// <summary>
        /// Defines the UNC Share where the flags exist
        /// </summary>
        [RequiredArgument]
        public InArgument<string> LockingUNCShare { get; set; }

        /// <summary>
        /// Defines the List of Environments (by Name) to watch
        /// </summary>
        [RequiredArgument]
        public InArgument<string[]> EnvironmentList { get; set; }

        /// <summary>
        /// Defines the maximum wait time for this activity to wait until there is an available environment, 0 indicates
        /// that there is no timeout.
        /// </summary>
        [RequiredArgument]
        public InArgument<int> MaximumWaitTimeSeconds { get; set; }

        /// <summary>
        /// Defines the returned information indicating whether or not the lock-file is created, false
        /// indicates that there was a problem creating the lock file
        /// </summary>
        public OutArgument<bool> EnvironmentIsAvailable { get; set; }

        /// <summary>
        /// Execute the Update Version Number build step.
        /// </summary>
        protected override void InternalExecute()
        {
            //-- Get the input parameters
            string lockingUNCShare = this.ActivityContext.GetValue(this.LockingUNCShare);
            string[] environmentNames = this.ActivityContext.GetValue(this.EnvironmentList);
            int maximumWaitTime = this.ActivityContext.GetValue(this.MaximumWaitTimeSeconds);

            //-- Calculate the end time...
            DateTime endTime = DateTime.Now.AddSeconds(maximumWaitTime);

            //-- Get the details about the Project Collection, Build, and LabService...
            TfsTeamProjectCollection tpc = this.ActivityContext.GetExtension<TfsTeamProjectCollection>();
            LabService labService = tpc.GetService<LabService>();
            IBuildDetail buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();

            //-- Get a list of environments that could be used...
            ICollection<LabEnvironment> lstEnvironments = labService.QueryLabEnvironments(new LabEnvironmentQuerySpec { Project = buildDetail.TeamProject });

            //-- Loop until we have reached the maximum wait time...
            while (maximumWaitTime == 0 || (DateTime.Now < endTime))
            {
                //-- First, lets loop through the environments to see if any have become available...
                foreach (string name in environmentNames)
                {
                    //-- Is the environment locked?
                    if (!File.Exists(Path.Combine(lockingUNCShare, name)))
                    {
                        //-- Locate the environment in the list of potential environments...
                        foreach (LabEnvironment environment in lstEnvironments)
                        {
                            if (environment.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                            {
                                //-- Get the environment In-Use information...
                                LabEnvironmentInUseMarker marker = environment.GetInUseMarker();
                                if (marker == null)
                                {
                                    //-- The Environment is not in use...
                                    this.ActivityContext.SetValue(this.EnvironmentIsAvailable, true);
                                    return;
                                }

                                break;
                            }
                        }
                    }
                }

                //-- Wait for 5 seconds and try again...
                Thread.Sleep(5000);
            }

            //-- If we reached here, none of the available environments became available...
            this.ActivityContext.SetValue(this.EnvironmentIsAvailable, false);
        }
    }
}
