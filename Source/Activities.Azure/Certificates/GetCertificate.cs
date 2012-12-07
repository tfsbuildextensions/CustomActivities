//-----------------------------------------------------------------------
// <copyright file="GetCertificate.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Certificates
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the public data for a certificate.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetCertificate : BaseAzureActivity
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
        /// Gets or sets the certificate.
        /// </summary>
        public OutArgument<Certificate> Certificate { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a certificate.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                Certificate cert = this.RetryCall(s => this.Channel.GetCertificate(
                    s,
                    this.ServiceName.Get(this.ActivityContext),
                    this.ThumbprintAlgorithm.Get(this.ActivityContext),
                    this.CertificateThumbprintId.Get(this.ActivityContext)));
                this.Certificate.Set(this.ActivityContext, cert);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                this.Certificate.Set(this.ActivityContext, null);
            }
        }
    }
}