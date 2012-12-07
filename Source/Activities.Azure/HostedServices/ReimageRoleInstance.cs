//-----------------------------------------------------------------------
// <copyright file="ReimageRoleInstance.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ComponentModel;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Possible actions for the ReimageRoleInstance activity.
    /// </summary>
    public enum ReimageRoleInstanceAction
    {
        /// <summary>
        /// Reimage the deployment defined by the unique deployment name.
        /// </summary>
        ReimageByInstanceName = 0,

        /// <summary>
        /// Reimage the deployment defined by the service name and slot.
        /// </summary>
        ReimageBySlot
    }

    /// <summary>
    /// Request a reboot of a role instance that is running in a deployment.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class ReimageRoleInstance : BaseAzureAsynchronousActivity
    {
        /// <summary>
        /// The action to perform
        /// </summary>
        private ReimageRoleInstanceAction action = ReimageRoleInstanceAction.ReimageByInstanceName;

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public ReimageRoleInstanceAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

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
        /// Gets or sets the Azure deployment name.
        /// </summary>
        public InArgument<string> DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the Azure role instance name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> InstanceName { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and reimage a deployment.
        /// </summary>
        /// <returns>The asynchronous operation identifier.</returns>
        protected override string AzureExecute()
        {
            try
            {
                if (this.Action == ReimageRoleInstanceAction.ReimageByInstanceName)
                {
                    this.RetryCall(s => this.Channel.ReimageDeploymentRoleInstance(s, this.ServiceName.Get(this.ActivityContext), this.DeploymentName.Get(this.ActivityContext), this.InstanceName.Get(this.ActivityContext)));
                }
                else
                {
                    this.RetryCall(s => this.Channel.ReimageDeploymentRoleInstanceBySlot(s, this.ServiceName.Get(this.ActivityContext), this.Slot.Get(this.ActivityContext), this.InstanceName.Get(this.ActivityContext)));
                }

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
