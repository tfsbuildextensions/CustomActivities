//-----------------------------------------------------------------------
// <copyright file="BaseAzureActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    /// <summary>
    /// Provide the base integration to the Azure Service Management API for all activities.
    /// </summary>
    public abstract class BaseAzureActivity : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the Azure subscription ID.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the Azure account certificate.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> CertificateThumbprintId { get; set; }

        /// <summary>
        /// Reference to the Azure client certificate loaded from the local store.
        /// </summary>
        protected X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// WCF binding to the Azure service.
        /// </summary>
        protected Binding ServiceBinding { get; set; }

        /// <summary>
        /// WCF endpoint connection location.
        /// </summary>
        protected string ServiceEndpoint { get; set; }

        /// <summary>
        /// Azure service management interface instance.
        /// </summary>
        protected IServiceManagement Channel { get; set; }

        /// <summary>
        /// Get the Azure operation identifier from the server response headers.
        /// </summary>
        /// <returns>The operation identifier.</returns>
        protected static string RetrieveOperationId()
        {
            var operationId = string.Empty;

            if (WebOperationContext.Current.IncomingResponse != null)
            {
                operationId = WebOperationContext.Current.IncomingResponse.Headers[Constants.OperationTrackingIdHeader];
            }

            return operationId;
        }

        /// <summary>
        /// Prevent inheritance of the method.  Bind required parameters.
        /// </summary>
        protected sealed override void InternalExecute()
        {
            // Find the certficate from the local store
            this.Certificate = this.FindCertificate();

            // Setup the WCF channel to the Azure Management Service
            if (this.Channel == null)
            {
                this.Channel = this.CreateChannel();
                if (this.Channel == null)
                {
                    this.LogBuildError("Unable to connect to the Azure Service.");
                    return;
                }
            }

            // Execute the activity body
            this.AzureExecute();
        }

        /// <summary>
        /// Find the deployment tools certificate by its thumbprint identifier.
        /// </summary>
        /// <returns>A valid X.509 certificate or null.</returns>
        protected X509Certificate2 FindCertificate()
        {
            // Bind the certificate from the local store
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certCollection = store.Certificates;
                X509Certificate2Collection validCerts = certCollection.Find(X509FindType.FindByThumbprint, this.CertificateThumbprintId.Get(this.ActivityContext), false);
                if (validCerts.Count > 0)
                {
                    return validCerts[0];
                }
                else
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Unable to obtain certificate from the CURRENT_USER\\MY store"));
                }
            }
            finally
            {
                store.Close();
            }

            return null;
        }

        /// <summary>
        /// AzureExecute method which Azure-specific activities should implement
        /// </summary>
        protected abstract void AzureExecute();

        #region Azure Service Management API wrappers

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

            return ServiceManagementHelper.CreateServiceManagementChannel(this.ServiceBinding, new Uri(this.ServiceEndpoint), this.Certificate);
        }

        /// <summary>
        /// Execute a call to the Azure service, with retry logic for common failures.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier for which to interact with.</param>
        /// <param name="call">The API call to execute.</param>
        protected void RetryCall(string subscriptionId, Action<string> call)
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
        /// <param name="call">The API call to execute.</param>
        protected void RetryCall(Action<string> call)
        {
            this.RetryCall(this.SubscriptionId.Get(this.ActivityContext), call);
        }

        /// <summary>
        /// Execute a call to the Azure service, with retry logic for common failures.
        /// </summary>
        /// <typeparam name="TResult">Expected return type of the service call.</typeparam>
        /// <param name="subscriptionId">The subscription identifier for which to interact with.</param>
        /// <param name="call">The API call to execute.</param>
        /// <returns>The result of the API call.</returns>
        protected TResult RetryCall<TResult>(string subscriptionId, Func<string, TResult> call)
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
                        else
                        {
                            return call(subscriptionId.ToUpper(CultureInfo.InvariantCulture));
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
                    if (subscriptionId.Equals(subscriptionId.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        return call(subscriptionId.ToLower(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        return call(subscriptionId.ToUpper(CultureInfo.InvariantCulture));
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
        /// <param name="call">The API call to execute.</param>
        /// <returns>The result of the API call.</returns>
        protected TResult RetryCall<TResult>(Func<string, TResult> call)
        {
            return this.RetryCall(this.SubscriptionId.Get(this.ActivityContext), call);
        }

        #endregion
    }
}