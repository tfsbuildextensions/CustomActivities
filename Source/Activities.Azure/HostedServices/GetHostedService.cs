//-----------------------------------------------------------------------
// <copyright file="GetHostedService.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.Azure.Model;

    /// <summary>
    /// Get the context for an Azure service within a subscription.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetHostedService : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the service context.
        /// </summary>
        public OutArgument<HostedServiceContext> ServiceContext { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain service information.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                HostedService hostedService = this.RetryCall(s => this.Channel.GetHostedService(s, this.ServiceName.Get(this.ActivityContext)));

                if (string.IsNullOrEmpty(hostedService.ServiceName))
                {
                    hostedService.ServiceName = this.ServiceName.Get(this.ActivityContext);
                }

                var ctx = new HostedServiceContext(hostedService);
                ctx.SubscriptionId = this.SubscriptionId.Get(this.ActivityContext);
                ctx.ServiceName = this.ServiceName.Get(this.ActivityContext);
                ctx.Certificate = this.Certificate;

                this.ServiceContext.Set(this.ActivityContext, ctx);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.ServiceContext.Set(this.ActivityContext, null);
            }
        }
    }
}
