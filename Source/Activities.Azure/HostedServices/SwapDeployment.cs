//-----------------------------------------------------------------------
// <copyright file="SwapDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Execute a swap operation between the staging and production slots.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class SwapDeployment : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the Azure staging deployment name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> StagingDeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the Azure production deployment name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ProductionDeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the operation id of the Azure API command.
        /// </summary>
        public OutArgument<string> OperationId { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and execute a VIP swap.
        /// </summary>
        protected override void AzureExecute()
        {
            var swapDeploymentInput = new SwapDeploymentInput()
            {
                SourceDeployment = this.StagingDeploymentName.Get(this.ActivityContext),
                Production = this.ProductionDeploymentName.Get(this.ActivityContext)
            };

            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    this.RetryCall(s => this.Channel.SwapDeployment(s, this.ServiceName.Get(this.ActivityContext), swapDeploymentInput));
                    this.OperationId.Set(this.ActivityContext, RetrieveOperationId());
                }
                catch (EndpointNotFoundException ex)
                {
                    LogBuildMessage(ex.Message);
                    this.OperationId.Set(this.ActivityContext, null);
                }
            }
        }
    }
}
