//-----------------------------------------------------------------------
// <copyright file="GetDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.Azure.Model;

    /// <summary>
    /// Get the deployment context for a currently running service.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetDeployment : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure deployment slot identifier.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Slot { get; set; }

        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the deployment context.
        /// </summary>
        public OutArgument<DeploymentInfoContext> DeploymentContext { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain deployment information.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                Deployment deployment = this.RetryCall(s => this.Channel.GetDeploymentBySlot(s, this.ServiceName.Get(this.ActivityContext), this.Slot.Get(this.ActivityContext)));

                if (string.IsNullOrEmpty(deployment.DeploymentSlot))
                {
                    deployment.DeploymentSlot = this.Slot.Get(this.ActivityContext);
                }

                var ctx = new DeploymentInfoContext(deployment);
                ctx.SubscriptionId = this.SubscriptionId.Get(this.ActivityContext);
                ctx.ServiceName = this.ServiceName.Get(this.ActivityContext);
                ctx.Certificate = this.ManagementCertificate;

                this.DeploymentContext.Set(this.ActivityContext, ctx);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.DeploymentContext.Set(this.ActivityContext, null);
            }
        }
    }
}
