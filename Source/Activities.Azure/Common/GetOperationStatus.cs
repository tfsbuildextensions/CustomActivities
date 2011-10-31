//-----------------------------------------------------------------------
// <copyright file="GetOperationStatus.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Common
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the status of an asynchronous operation.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetOperationStatus : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the operation id of the Azure API command.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> OperationId { get; set; }

        /// <summary>
        /// Gets or sets the operation state of the Azure API command.
        /// </summary>
        public OutArgument<string> OperationState { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain information about a pending operation.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                Operation operation = this.RetryCall(s => this.Channel.GetOperationStatus(s, this.OperationId.Get(this.ActivityContext)));
                this.OperationState.Set(this.ActivityContext, operation.Status);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.OperationState.Set(this.ActivityContext, null);
            }
        }
    }
}