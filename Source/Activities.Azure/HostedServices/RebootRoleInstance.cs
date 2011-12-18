//-----------------------------------------------------------------------
// <copyright file="RebootRoleInstance.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System.Activities;
    using System.ComponentModel;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.Azure.Model;

    /// <summary>
    /// Possible actions for the RebootRoleInstance activity.
    /// </summary>
    public enum RebootRoleInstanceAction
    {
        /// <summary>
        /// Reboots the deployment defined by the unique deployment name.
        /// </summary>
        RebootByInstanceName = 0,

        /// <summary>
        /// Reboots the deployment defined by the service name and slot.
        /// </summary>
        RebootBySlot
    }

    /// <summary>
    /// Request a reboot of a role instance that is running in a deployment.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class RebootRoleInstance : BaseAzureAsynchronousActivity
    {
        /// <summary>
        /// The action to perform
        /// </summary>
        private RebootRoleInstanceAction action = RebootRoleInstanceAction.RebootByInstanceName;

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public RebootRoleInstanceAction Action
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
        /// Connect to an Azure subscription and reboot a deployment.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                if (this.Action == RebootRoleInstanceAction.RebootByInstanceName)
                {
                    this.RetryCall(s => this.Channel.RebootDeploymentRoleInstance(s, this.ServiceName.Get(this.ActivityContext), this.DeploymentName.Get(this.ActivityContext), this.InstanceName.Get(this.ActivityContext)));
                }
                else
                {
                    this.RetryCall(s => this.Channel.RebootDeploymentRoleInstanceBySlot(s, this.ServiceName.Get(this.ActivityContext), this.Slot.Get(this.ActivityContext), this.InstanceName.Get(this.ActivityContext)));
                }

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
