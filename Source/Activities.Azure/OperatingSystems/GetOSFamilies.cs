//-----------------------------------------------------------------------
// <copyright file="GetOSFamilies.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.OperatingSystems
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get a list of the versions of the guest operating system families that are currently available in Windows Azure.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetOSFamilies : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the operating system family list.
        /// </summary>
        public OutArgument<OperatingSystemFamilyList> OperatingSystemFamilies { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a list of operating systems.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                OperatingSystemFamilyList families = this.RetryCall(s => this.Channel.ListOperatingSystemFamilies(s));
                this.OperatingSystemFamilies.Set(this.ActivityContext, families);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                this.OperatingSystemFamilies.Set(this.ActivityContext, null);
            }
        }
    }
}