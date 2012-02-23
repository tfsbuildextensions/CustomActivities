//-----------------------------------------------------------------------
// <copyright file="UnlockEnvironment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.LabManagement
{
    using System;
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Provides an activity that unlocks an environment, providing it is the build that locked it (unless explicitly overridden)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class UnlockEnvironment : BaseCodeActivity
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
        /// Defines the Build Number
        /// </summary>
        [RequiredArgument]
        public InArgument<string> BuildNumber { get; set; }

        /// <summary>
        /// Defines the flag used to indicate that the environment should be forcibly unlocked
        /// </summary>
        [RequiredArgument]
        public InArgument<bool> ForceUnlock { get; set; }

        /// <summary>
        /// Defines the returned information indicating whether or not the lock-file is created, false
        /// indicates that there was a problem creating the lock file
        /// </summary>
        public OutArgument<bool> Success { get; set; }

        /// <summary>
        /// Execute the Update Version Number build step.
        /// </summary>
        protected override void InternalExecute()
        {
            //-- Get the input parameters
            string lockingUncShare = this.ActivityContext.GetValue(this.LockingUNCShare);
            string environmentName = this.ActivityContext.GetValue(this.EnvironmentName);
            string buildNumber = this.ActivityContext.GetValue(this.BuildNumber);
            bool forceUnlock = this.ActivityContext.GetValue(this.ForceUnlock);

            //-- Calculate the full path to the target file...
            string strTargetFile = Path.Combine(lockingUncShare, environmentName);

            //-- If the File Already doesn't Exists, the environment is unlocked...      
            if (!File.Exists(strTargetFile))
            {
                this.ActivityContext.SetValue(this.Success, true);
            }

            //-- Create a file with our build number inside it...
            using (StreamReader reader = new StreamReader(strTargetFile))
            {
                string strFileContents = reader.ReadToEnd();

                //-- Was the environment locked by our build number?
                if (!strFileContents.Equals(buildNumber, StringComparison.OrdinalIgnoreCase) && !forceUnlock)
                {
                    //-- No, the environment was not locked by this build...
                    this.ActivityContext.SetValue(this.Success, false);
                }
            }

            //-- If we made it here, we should delete the lock file...
            File.Delete(strTargetFile);
            this.ActivityContext.SetValue(this.Success, true);
        }
    }
}
