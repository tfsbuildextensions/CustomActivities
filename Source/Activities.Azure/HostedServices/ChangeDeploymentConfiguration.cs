//-----------------------------------------------------------------------
// <copyright file="ChangeDeploymentConfiguration.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.HostedServices
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.Azure.Extended;

    /// <summary>
    /// Possible actions for the ChangeDeploymentConfiguration activity.
    /// </summary>
    public enum ChangeDeploymentConfigurationAction
    {
        /// <summary>
        /// Changes the deployment defined by the unique deployment name.
        /// </summary>
        ChangeByDeploymentName = 0,

        /// <summary>
        /// Changes the deployment defined by the service name and slot.
        /// </summary>
        ChangeBySlot
    }

    /// <summary>
    /// Initiate a change to the deployment configuration.
    /// <b>Valid Action values are:</b>
    /// <para><i>ChangeByDeploymentName</i></para>
    /// <para><i>ChangeBySlot</i></para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class ChangeDeploymentConfiguration : BaseAzureAsynchronousActivity
    {
        /// <summary>
        /// The action to perform
        /// </summary>
        private ChangeDeploymentConfigurationAction action = ChangeDeploymentConfigurationAction.ChangeByDeploymentName;

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public ChangeDeploymentConfigurationAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Gets or sets the Azure deployment slot identifier.
        /// </summary>
        public InArgument<string> Slot { get; set; }

        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the Azure deployment name.
        /// </summary>
        public InArgument<string> DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the configuration file path.
        /// This parameter should specifiy a .cscfg file on disk.
        /// </summary>
        public InArgument<string> ConfigurationFilePath { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and change the configuration of a deployment.
        /// </summary>
        /// <returns>The asynchronous operation identifier.</returns>
        protected override string AzureExecute()
        {
            var changeInput = this.CreateConfigurationInput();

            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    if (this.action == ChangeDeploymentConfigurationAction.ChangeByDeploymentName)
                    {
                        this.RetryCall(s => this.Channel.ChangeConfiguration(s, this.ServiceName.Get(this.ActivityContext), this.DeploymentName.Get(this.ActivityContext), changeInput));
                    }
                    else
                    {
                        this.RetryCall(s => this.Channel.ChangeConfigurationBySlot(s, this.ServiceName.Get(this.ActivityContext), this.Slot.Get(this.ActivityContext), changeInput));
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

        private ChangeConfigurationInput CreateConfigurationInput()
        {
            return new ChangeConfigurationInput
            {
                Configuration = Utility.GetConfiguration(this.ConfigurationFilePath.Get(this.ActivityContext)),                 
                TreatWarningsAsError = false
            };
        }
    }
}
