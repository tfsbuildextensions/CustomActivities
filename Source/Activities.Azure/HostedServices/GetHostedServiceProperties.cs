//-----------------------------------------------------------------------
// <copyright file="GetHostedServiceProperties.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the properties of a defined Azure service.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetHostedServiceProperties : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the service properties.
        /// </summary>
        public OutArgument<HostedServiceProperties> ServiceProperties { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain service properties.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                HostedService hostedService = this.RetryCall(s => this.Channel.GetHostedService(s, this.ServiceName.Get(this.ActivityContext)));
                this.ServiceProperties.Set(this.ActivityContext, hostedService.HostedServiceProperties);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.ServiceProperties.Set(this.ActivityContext, null);
            }
        }
    }
}