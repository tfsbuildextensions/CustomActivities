//-----------------------------------------------------------------------
// <copyright file="GetStorageProperties.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.StorageAccounts
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the properties of a storage service account.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetStorageProperties : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the storage service properties.
        /// </summary>
        public OutArgument<StorageServiceProperties> ServiceProperties { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain storage service properties.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                StorageService storageService = this.RetryCall(s => this.Channel.GetStorageService(s, this.ServiceName.Get(this.ActivityContext)));
                this.ServiceProperties.Set(this.ActivityContext, storageService.StorageServiceProperties);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.ServiceProperties.Set(this.ActivityContext, null);
            }
        }
    }
}