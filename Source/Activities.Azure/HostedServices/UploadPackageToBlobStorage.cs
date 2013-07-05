//-----------------------------------------------------------------------
// <copyright file="UploadPackageToBlobStorage.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.IO;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary>
    /// Upload a packaged service to blob storage as a precursor to deployment.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class UploadPackageToBlobStorage : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure storage service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> StorageServiceName { get; set; }
        
        /// <summary>
        /// Gets or sets the storage service keys.
        /// </summary>
        [RequiredArgument]
        public InArgument<StorageServiceKeys> StorageKeys { get; set; }

        /// <summary>
        /// Gets or sets the package location.
        /// This parameter should have the path or URI to a .cspkg in blob storage whose storage account is part of the same subscription/project.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> LocalPackagePath { get; set; }

        /// <summary>
        /// Gets or sets the operation id of the Azure API command.
        /// </summary>
        public OutArgument<string> PackageUrl { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and upload a package to blob storage.
        /// </summary>
        protected override void AzureExecute()
        {
            string storageKey = this.StorageKeys.Get(this.ActivityContext).Primary;
            string storageName = this.StorageServiceName.Get(this.ActivityContext);
            string filePath = this.LocalPackagePath.Get(this.ActivityContext);

            var baseAddress = string.Format(CultureInfo.InvariantCulture, ConfigurationConstants.BlobEndpointTemplate, storageName);
            var credentials = new StorageCredentialsAccountAndKey(storageName, storageKey);
            var client = new CloudBlobClient(baseAddress, credentials);

            const string ContainerName = "mydeployments";
            string blobName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
                Path.GetFileName(filePath));

            CloudBlobContainer container = client.GetContainerReference(ContainerName);
            container.CreateIfNotExist();
            CloudBlob blob = container.GetBlobReference(blobName);

            UploadBlobStream(blob, filePath);

            this.PackageUrl.Set(this.ActivityContext, string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", client.BaseUri, ContainerName, client.DefaultDelimiter, blobName));
        }

        private static void UploadBlobStream(CloudBlob blob, string sourceFile)
        {
            using (FileStream readStream = File.OpenRead(sourceFile))
            {
                byte[] buffer = new byte[1024 * 128];

                using (BlobStream blobStream = blob.OpenWrite())
                {
                    blobStream.BlockSize = 1024 * 128;

                    while (true)
                    {
                        int bytesCount = readStream.Read(buffer, 0, buffer.Length);

                        if (bytesCount <= 0)
                        {
                            break;
                        }

                        blobStream.Write(buffer, 0, bytesCount);
                    }
                }
            }
        }
    }
}