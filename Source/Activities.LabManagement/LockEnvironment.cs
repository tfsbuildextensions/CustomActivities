//-----------------------------------------------------------------------
// <copyright file="LockEnvironment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System;
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /*
     *  This Activity Represents a Work in progress and is subject to change without notice until the
     *  corresponding process template has been published.
     */

    /// <summary>
    /// Provides an activity that locked the environment and writes the build number into the lock file
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class LockEnvironment : CodeActivity
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
        /// Defines the returned information indicating whether or not the lock-file is created, false
        /// indicates that there was a problem creating the lock file
        /// </summary>
        public OutArgument<bool> Success { get; set; }

        /// <summary>
        /// Execute the Update Version Number build step.
        /// </summary>
        /// <param name="context">Contains the workflow context</param>
        protected override void Execute(CodeActivityContext context)
        {
            //-- Get the input parameters
            string lockingUncShare = context.GetValue(this.LockingUNCShare);
            string environmentName = context.GetValue(this.EnvironmentName);
            string buildNumber = context.GetValue(this.BuildNumber);

            //-- Calculate the full path to the target file...
            string targetFile = Path.Combine(lockingUncShare, environmentName);

            //-- Calculate a TempFile name that will be unique to this build...
            string tempFileName = targetFile + "_" + buildNumber + ".Lock";

            //-- If the File Already Exists, we fail...
            if (File.Exists(targetFile))
            {
                //-- Already Locked, lets see who has it locked...
                using (StreamReader reader = new StreamReader(targetFile))
                {
                    string strFileContents = reader.ReadToEnd();

                    //-- Was the environment locked by our build number?
                    if (strFileContents.Equals(buildNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        //-- Yes, indicate that we have locked it...
                        context.SetValue(this.Success, true);
                    }
                }

                //-- Lock file exists, but this build is not the one that locked it...
                context.SetValue(this.Success, false);
            }

            try
            {
                //-- Create a file with our build number inside it...
                using (StreamWriter writer = File.CreateText(tempFileName))
                {
                    writer.Write(buildNumber);
                    writer.Flush();
                }

                //-- If the File does not exist, rename the .lock file...
                if (!File.Exists(targetFile))
                {
                    File.Move(tempFileName, targetFile);

                    //-- We have successfully Locked the environment...
                    context.SetValue(this.Success, true);
                }
                else
                {
                    //-- Lock File Now Exists, so we cannot lock this environment...
                    context.SetValue(this.Success, false);
                }
            }
            catch (Exception)
            {
                //-- We could not lock the build, or there was some other issue...
                context.SetValue(this.Success, false);
            }
            finally
            {
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
        }
    }
}
