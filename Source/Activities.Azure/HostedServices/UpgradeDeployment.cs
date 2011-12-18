//-----------------------------------------------------------------------
// <copyright file="UpgradeDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System;
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.Azure.Helpers;

    /// <summary>
    /// UpgradeMode enumeration
    /// </summary>
    public enum UpgradeMode
    {
        /// <summary>
        /// The Windows Azure platform will automatically apply the update to each Upgrade Domain in sequence.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// WalkUpgradeDomain must be called to apply the update
        /// </summary>
        Manual
    }

    /// <summary>
    /// Upgrade an existing deployment for a package which has already been uploaded.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class UpgradeDeployment : BaseAzureAsynchronousActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the role to upgrade (OPTIONAL).
        /// </summary>
        public InArgument<string> RoleName { get; set; }

        /// <summary>
        /// Gets or sets the package location.
        /// This parameter should have the path or URI to a .cspkg in blob storage whose storage account is part of the same subscription/project.
        /// </summary>
        public InArgument<string> PackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the configuration file path.
        /// This parameter should specifiy a .cscfg file on disk.
        /// </summary>
        public InArgument<string> ConfigurationFilePath { get; set; }

        /// <summary>
        /// Gets or sets the label name for the new deployment.
        /// </summary>
        public InArgument<string> DeploymentLabel { get; set; }

        /// <summary>
        /// Gets or sets the Azure deployment name.
        /// </summary>
        public InArgument<string> DeploymentName { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and upgrade an existing deployment.
        /// </summary>
        protected override void AzureExecute()
        {
            var deploymentInput = this.CreateUpgradeInput();

            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    this.RetryCall(s => this.Channel.UpgradeDeployment(s, this.ServiceName.Get(this.ActivityContext), this.DeploymentName.Get(this.ActivityContext), deploymentInput));
                    this.OperationId.Set(this.ActivityContext, RetrieveOperationId());
                }
                catch (EndpointNotFoundException ex)
                {
                    LogBuildMessage(ex.Message);
                    this.OperationId.Set(this.ActivityContext, null);
                }
            }
        }

        private UpgradeDeploymentInput CreateUpgradeInput()
        {
            string deploymentName = this.DeploymentName.Get(this.ActivityContext);
            if (string.IsNullOrEmpty(deploymentName))
            {
                deploymentName = Guid.NewGuid().ToString();
            }

            string package = this.PackageUrl.Get(this.ActivityContext); 
            Uri packageUrl;
            if (package.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                package.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                packageUrl = new Uri(package);
            }
            else
            {
                throw new InvalidOperationException("You must upload the blob to Azure before creating a new deployment.");
            }

            return new UpgradeDeploymentInput
            {
                PackageUrl = packageUrl,
                Configuration = Utility.GetConfiguration(this.ConfigurationFilePath.Get(this.ActivityContext)),
                Label = ServiceManagementHelper.EncodeToBase64String(this.DeploymentLabel.Get(this.ActivityContext)),
                RoleToUpgrade = this.RoleName.Get(this.ActivityContext),
                Mode = "Auto",
                TreatWarningsAsError = this.TreatWarningsAsErrors.Get(this.ActivityContext)
            };
        }
    }
}
