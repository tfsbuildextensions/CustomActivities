//-----------------------------------------------------------------------
// <copyright file="RemoveCertificate.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Certificates
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Deletes a certificate from the subscription's certificate store.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class RemoveCertificate : BaseAzureAsynchronousActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the Azure account certificate algorithm.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ThumbprintAlgorithm { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a certificate.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                this.RetryCall(s => this.Channel.DeleteCertificate(
                    s,
                    this.ServiceName.Get(this.ActivityContext),
                    this.ThumbprintAlgorithm.Get(this.ActivityContext), 
                    this.CertificateThumbprintId.Get(this.ActivityContext)));
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