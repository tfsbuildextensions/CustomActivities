//-----------------------------------------------------------------------
// <copyright file="CheckIfEnvironmentIsLocked.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /*
     *  This Activity Represents a Work in progress and is subject to change without notice until the
     *  corresponding process template has been published.
     */

    /// <summary>
    /// Provides an activity that can be used to determine if an Environment is locked (via the file-lock)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class CheckIfEnvironmentIsLocked : CodeActivity
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
        /// <param name="context">Contains the workflow context</param>
        protected override void Execute(CodeActivityContext context)
        {
            //-- Get the input parameters
            string lockingUncShare = context.GetValue(this.LockingUNCShare);
            string environmentName = context.GetValue(this.EnvironmentName);

            //-- If the file does not exist, we are not locked...
            if (!File.Exists(Path.Combine(lockingUncShare, environmentName)))
            {
                context.SetValue(this.EnvironmentIsLocked, false);
            }

            //-- If we made it here, the file exists, and we can be considered locked
            context.SetValue(this.EnvironmentIsLocked, true);
        }
    }
}
