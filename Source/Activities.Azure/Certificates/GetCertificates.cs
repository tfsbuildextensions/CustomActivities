//-----------------------------------------------------------------------
// <copyright file="GetCertificates.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Certificates
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get a list of all certificates associated with a hosted service.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetCertificates : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the certificate list.
        /// </summary>
        public OutArgument<CertificateList> Certificates { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a list certificates.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                CertificateList certs = this.RetryCall(s => this.Channel.ListCertificates(s, this.ServiceName.Get(this.ActivityContext)));
                this.Certificates.Set(this.ActivityContext, certs);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                this.Certificates.Set(this.ActivityContext, null);
            }
        }
    }
}