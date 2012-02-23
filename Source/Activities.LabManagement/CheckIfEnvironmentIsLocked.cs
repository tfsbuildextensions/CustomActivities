//-----------------------------------------------------------------------
// <copyright file="CheckIfEnvironmentIsLocked.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Provides an activity that can be used to determine if an Environment is locked (via the file-lock)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class CheckIfEnvironmentIsLocked : BaseCodeActivity
    {
        /// <summary>
        /// Defines the UNC Share where the flags exist
        /// </summary>
        [RequiredArgument]
        public InArgument<string> LockingUNCShare { get; set; }

        /// <summary>
        /// Defines the Environment Name
        /// </summary>
        [RequiredArgument]
        public InArgument<string> EnvironmentName { get; set; }

        /// <summary>
        /// Defines the returned information indicating whether or not the lock-file is created, false
        /// indicates that there was a problem creating the lock file
        /// </summary>
        public OutArgument<bool> EnvironmentIsLocked { get; set; }

        /// <summary>
        /// Execute the Update Version Number build step.
        /// </summary>
        protected override void InternalExecute()
        {
            //-- Get the input parameters
            string lockingUncShare = this.ActivityContext.GetValue(this.LockingUNCShare);
            string environmentName = this.ActivityContext.GetValue(this.EnvironmentName);

            //-- If the file does not exist, we are not locked...
            if (!File.Exists(Path.Combine(lockingUncShare, environmentName)))
            {
                this.ActivityContext.SetValue(this.EnvironmentIsLocked, false);
            }

            //-- If we made it here, the file exists, and we can be considered locked
            this.ActivityContext.SetValue(this.EnvironmentIsLocked, true);
        }
    }
}
