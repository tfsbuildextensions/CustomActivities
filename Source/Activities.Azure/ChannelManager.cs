//-----------------------------------------------------------------------
// <copyright file="ChannelManager.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    /// <summary>
    /// Provide common handling for Azure API channels.
    /// </summary>
    internal class ChannelManager
    {
        /// <summary>
        /// Reference to the Azure client certificate loaded from the local store.
        /// </summary>
        internal X509Certificate2 ManagementCertificate { get; set; }

        /// <summary>
        /// Azure service management interface instance.
        /// </summary>
        internal IServiceManagement Channel { get; set; }

        /// <summary>
        /// WCF binding to the Azure service.
        /// </summary>
        protected Binding ServiceBinding { get; set; }

        /// <summary>
        /// WCF endpoint connection location.
        /// </summary>
        protected string ServiceEndpoint { get; set; }

        /// <summary>
        /// Initialize Azure Service Management channel.
        /// </summary>
        /// <param name="certificateThumbprintId">The thumbprint of the certificate for the channel.</param>
        /// <returns>An instance of the channel manager.</returns>
        public ChannelManager InitializeChannel(string certificateThumbprintId)
        {
            // Find the certficate from the local store
            this.ManagementCertificate = this.FindCertificate(certificateThumbprintId);

            // Setup the WCF channel to the Azure Management Service
            if (this.Channel == null)
            {
                this.Channel = this.CreateChannel();
                if (this.Channel == null)
                {
                    throw new CommunicationException();
                }
            }

            return this;
        }

        #region Azure Service Management API wrappers

        /// <summary>
        /// Execute a call to the Azure service, with retry logic for common failures.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier for which to interact with.</param>
        /// <param name="call">The API call to execute.</param>
        public void RetryCall(string subscriptionId, Action<string> call)
        {
            try
            {
                try
                {
                    call(subscriptionId);
                }
                catch (MessageSecurityException ex)
                {
                    var webException = ex.InnerException as WebException;

                    if (webException == null)
                    {
                        throw;
                    }

                    var webResponse = webException.Response as HttpWebResponse;

                    if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        this.Channel = this.CreateChannel();
                        if (subscriptionId.Equals(subscriptionId.ToUpper(CultureInfo.InvariantCulture)))
                        {
                            call(subscriptionId.ToLower(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            call(subscriptionId.ToUpper(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (MessageSecurityException ex)
            {
                var webException = ex.InnerException as WebException;

                if (webException == null)
                {
                    throw;
                }

                var webResponse = webException.Response as HttpWebResponse;

                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.Channel = this.CreateChannel();
                    if (subscriptionId.Equals(subscriptionId.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        call(subscriptionId.ToLower(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        call(subscriptionId.ToUpper(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Execute a call to the Azure service, with retry logic for common failures.
        /// </summary>
        /// <typeparam name="TResult">Expected return type of the service call.</typeparam>
        /// <param name="subscriptionId">The subscription identifier for which to interact with.</param>
        /// <param name="call">The API call to execute.</param>
        /// <returns>The result of the API call.</returns>
        public TResult RetryCall<TResult>(string subscriptionId, Func<string, TResult> call)
        {
            try
            {
                try
                {
                    return call(subscriptionId);
                }
                catch (MessageSecurityException ex)
                {
                    var webException = ex.InnerException as WebException;

                    if (webException == null)
                    {
                        throw;
                    }

                    var webResponse = webException.Response as HttpWebResponse;

                    if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        if (subscriptionId.Equals(subscriptionId.ToUpper(CultureInfo.InvariantCulture)))
                        {
                            return call(subscriptionId.ToLower(CultureInfo.InvariantCulture));
                        }
                        
                        return call(subscriptionId.ToUpper(CultureInfo.InvariantCulture));
                    }

                    throw;
                }
            }
            catch (MessageSecurityException ex)
            {
                var webException = ex.InnerException as WebException;

                if (webException == null)
                {
                    throw;
                }

                var webResponse = webException.Response as HttpWebResponse;

                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    if (subscriptionId.Equals(subscriptionId.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        return call(subscriptionId.ToLower(CultureInfo.InvariantCulture));
                    }

                    return call(subscriptionId.ToUpper(CultureInfo.InvariantCulture));
                }

                throw;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Connect to the Azure Management Service.
        /// </summary>
        /// <returns>An instance of the WCF service interface.</returns>
        protected IServiceManagement CreateChannel()
        {
            if (this.ServiceBinding == null)
            {
                this.ServiceBinding = ConfigurationConstants.WebHttpBinding();
            }

            if (string.IsNullOrEmpty(this.ServiceEndpoint))
            {
                this.ServiceEndpoint = ConfigurationConstants.ServiceEndpoint;
            }

            return ServiceManagementHelper.CreateServiceManagementChannel(this.ServiceBinding, new Uri(this.ServiceEndpoint), this.ManagementCertificate);
        }

        /// <summary>
        /// Find the deployment tools certificate by its thumbprint identifier.
        /// </summary>
        /// <param name="certificateThumbprintId">The thumbprint of the certificate for the channel.</param>
        /// <returns>A valid X.509 certificate or null.</returns>
        protected X509Certificate2 FindCertificate(string certificateThumbprintId)
        {
            // Bind the certificate from the local store
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certCollection = store.Certificates;
                X509Certificate2Collection validCerts = certCollection.Find(X509FindType.FindByThumbprint, certificateThumbprintId, false);
                if (validCerts.Count > 0)
                {
                    return validCerts[0];
                }
                else
                {
                    throw new System.Security.Cryptography.CryptographicException();
                }
            }
            finally
            {
                store.Close();
            }
        }

        #endregion
    }
}
