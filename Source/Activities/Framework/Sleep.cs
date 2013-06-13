//-----------------------------------------------------------------------
// <copyright file="Sleep.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Framework
{
    using System.Activities;
    using System.Threading;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Sleep a thread for the specified number of milliseconds
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class Sleep : BaseCodeActivity
    {
        /// <summary>
        /// Sepecifies the number of milliseconds to sleep for
        /// </summary>
        [RequiredArgument]
        public InArgument<int> NumberOfMilliseconds { get; set; }

        /// <summary>
        /// InternalExecute method which activities should implement
        /// </summary>
        protected override void InternalExecute()
        {
            int numberOfMillisecs = this.NumberOfMilliseconds.Get(this.ActivityContext);
            this.LogBuildMessage(string.Format("Sleeping for {0} milliseconds", numberOfMillisecs));
            Thread.Sleep(numberOfMillisecs);
        }
    }
}
