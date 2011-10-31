//-----------------------------------------------------------------------
// <copyright file="GetStorageKeys.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.StorageServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the storage keys for a service.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetStorageKeys : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the storage service keys.
        /// </summary>
        public OutArgument<StorageServiceKeys> StorageKeys { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain storage service keys.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                StorageService storageService = this.RetryCall(s => this.Channel.GetStorageKeys(s, this.ServiceName.Get(this.ActivityContext)));              
                this.StorageKeys.Set(this.ActivityContext, storageService.StorageServiceKeys);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.StorageKeys.Set(this.ActivityContext, null);
            }
        }
    }
}