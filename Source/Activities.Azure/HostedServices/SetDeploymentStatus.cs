//-----------------------------------------------------------------------
// <copyright file="SetDeploymentStatus.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Change the service status for a deployed service.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class SetDeploymentStatus : BaseAzureAsynchronousActivity
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
        /// Gets or sets the status of the Azure service.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceStatus { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and set the deployment status.
        /// </summary>
        /// <returns>The asynchronous operation identifier.</returns>
        protected override string AzureExecute()
        {
            var updateDeploymentStatus = new UpdateDeploymentStatusInput()
            {
                Status = this.ServiceStatus.Get(this.ActivityContext)
            };

            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    this.RetryCall(s => this.Channel.UpdateDeploymentStatusBySlot(s, this.ServiceName.Get(this.ActivityContext), this.Slot.Get(this.ActivityContext), updateDeploymentStatus));
                    return RetrieveOperationId();
                }
                catch (EndpointNotFoundException ex)
                {
                    LogBuildMessage(ex.Message);
                    return null;
                }
            }
        }
    }
}