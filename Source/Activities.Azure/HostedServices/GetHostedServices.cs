//-----------------------------------------------------------------------
// <copyright file="GetHostedServices.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get a list of hosted services for a subscription.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetHostedServices : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the deployment context.
        /// </summary>
        public OutArgument<HostedServiceList> ServiceList { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain its service list.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                HostedServiceList hostedServices = this.RetryCall(s => this.Channel.ListHostedServices(s));
                this.ServiceList.Set(this.ActivityContext, hostedServices);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.ServiceList.Set(this.ActivityContext, null);
            }
        }
    }
}
