//-----------------------------------------------------------------------
// <copyright file="AddCertificate.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Certificates
{
    using System.Activities;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Add a certificate to a subscription.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class AddCertificate : BaseAzureAsynchronousActivity
    {
        /// <summary>
        /// Gets or sets the Azure service name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the certificate to upload
        /// </summary>
        public InArgument<X509Certificate2> Certificate { get; set; }

        /// <summary>
        /// Gets or sets the Azure account certificate algorithm.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Password { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a certificate.
        /// </summary>
        /// <returns>The asynchronous operation identifier.</returns>
        protected override string AzureExecute()
        {
            CertificateFile file = this.CreateFileFromCertificate();

            try
            {
                this.RetryCall(s => this.Channel.AddCertificates(s, this.ServiceName.Get(this.ActivityContext), file));
                return BaseAzureAsynchronousActivity.RetrieveOperationId();
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Create an Azure certificate structure from the provided cert.
        /// </summary>
        /// <returns>An Azure certificate structure.</returns>
        private CertificateFile CreateFileFromCertificate()
        {
            var cert = this.Certificate.Get(this.ActivityContext);
            byte[] certData = null;

            try
            {
                certData = cert.HasPrivateKey ? cert.Export(X509ContentType.Pfx) : cert.Export(X509ContentType.Pkcs12);
            }
            catch (CryptographicException)
            {
                certData = cert.RawData;
            }

            return new CertificateFile
            {
                Data = System.Convert.ToBase64String(certData),
                Password = this.Password.Get(this.ActivityContext),
                CertificateFormat = "pfx"
            };
        }
    }
}