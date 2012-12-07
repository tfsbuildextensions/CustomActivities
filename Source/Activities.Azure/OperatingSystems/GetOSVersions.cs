//-----------------------------------------------------------------------
// <copyright file="GetOSVersions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.OperatingSystems
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get a list of the versions of the guest operating system that are currently available in Windows Azure.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetOSVersions : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the operating system list.
        /// </summary>
        public OutArgument<OperatingSystemList> OperatingSystems { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a list of operating systems.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                OperatingSystemList operatingSystems = this.RetryCall(s => this.Channel.ListOperatingSystems(s));
                this.OperatingSystems.Set(this.ActivityContext, operatingSystems);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                this.OperatingSystems.Set(this.ActivityContext, null);
            }
        }
    }
}