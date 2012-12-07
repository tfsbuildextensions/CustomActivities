//-----------------------------------------------------------------------
// <copyright file="RemoveDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Remove a pre-existing deployment from a slot.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class RemoveDeployment : BaseAzureAsynchronousActivity
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
        /// Connect to an Azure subscription and remove the named deployment.
        /// </summary>
        /// <returns>The asynchronous operation identifier.</returns>
        protected override string AzureExecute()
        {
            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    this.RetryCall(s => this.Channel.DeleteDeploymentBySlot(s, this.ServiceName.Get(this.ActivityContext), this.Slot.Get(this.ActivityContext)));
                    return BaseAzureAsynchronousActivity.RetrieveOperationId();
                }
                catch (EndpointNotFoundException ex)
                {
                    this.LogBuildMessage(ex.Message);
                    return null;
                }
            }
        }
    }
}
