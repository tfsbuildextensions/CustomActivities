//-----------------------------------------------------------------------
// <copyright file="GetEnvironmentLockedByBuildNumber.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Provides an activity that allows the user to get the build number that currently has the environment locked
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetEnvironmentLockedByBuildNumber : BaseCodeActivity
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
        /// Returns the locking build number of the environment
        /// </summary>
        public OutArgument<string> BuildNumber { get; set; }

        /// <summary>
        /// Execute the Update Version Number build step.
        /// </summary>
        protected override void InternalExecute()
        {
            //-- Get the input parameters
            string lockingUncShare = this.ActivityContext.GetValue(this.LockingUNCShare);
            string environmentName = this.ActivityContext.GetValue(this.EnvironmentName);

            //-- Calculate the full path to the target file...
            string strTargetFile = Path.Combine(lockingUncShare, environmentName);

            //-- If the File Doesn't Exist, there is no lock, so return null...
            if (!File.Exists(strTargetFile))
            {
                this.ActivityContext.SetValue(this.BuildNumber, null);
            }

            //-- Create a file with our build number inside it...
            using (StreamReader reader = new StreamReader(strTargetFile))
            {
                //-- Read the contents of the file...
                string strFileContents = reader.ReadToEnd();

                //-- If we made it here, the file was created...
                this.ActivityContext.SetValue(this.BuildNumber, strFileContents);
            }
        }
    }
}
