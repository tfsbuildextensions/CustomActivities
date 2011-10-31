//-----------------------------------------------------------------------
// <copyright file="NewStorageKey.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.StorageServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Create storage keys to use with an Azure storage service account.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class NewStorageKey : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the Azure key type to regenerate.
        /// Can be either Primary or Secondary.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> KeyType { get; set; }

        /// <summary>
        /// Gets or sets the storage service keys.
        /// </summary>
        public OutArgument<StorageServiceKeys> StorageKeys { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain storage service keys.
        /// </summary>
        protected override void AzureExecute()
        {
            var regenerateKeys = new RegenerateKeys();
            regenerateKeys.KeyType = this.KeyType.Get(this.ActivityContext);

            try
            {
                StorageService storageService = this.RetryCall(s => this.Channel.RegenerateStorageServiceKeys(s, this.ServiceName.Get(this.ActivityContext), regenerateKeys));              
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