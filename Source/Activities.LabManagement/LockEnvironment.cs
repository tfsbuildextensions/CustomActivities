//-----------------------------------------------------------------------
// <copyright file="LockEnvironment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.LabManagement
{
	using System;
	using System.Activities;
	using System.IO;
	using System.Threading;
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

			//-- If the File Already Exists, we fail...
			if (File.Exists(targetFile))
			{
				context.SetValue(this.Success, false);
			}

			//-- Create a file with our build number inside it...
			using (StreamWriter writer = File.CreateText(targetFile))
			{
				writer.Write(buildNumber);
			}

			//-- Give the environment time to settle...
			Thread.Sleep(2500);

			//-- Check to make sure that we are the ones that ended up locking the environment...
			//-- Create a file with our build number inside it...
			using (StreamReader reader = new StreamReader(targetFile))
			{
				string strFileContents = reader.ReadToEnd();

				//-- Was the environment locked by our build number?
				if (!strFileContents.Equals(buildNumber, StringComparison.OrdinalIgnoreCase))
				{
					//-- No, the environment was not locked by this build...
					context.SetValue(this.Success, false);
				}
			}

			//-- If we made it here, the file was created...
			context.SetValue(this.Success, true);
		}
	}
}
