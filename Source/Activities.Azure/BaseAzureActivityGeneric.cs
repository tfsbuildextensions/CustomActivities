//-----------------------------------------------------------------------
// <copyright file="BaseAzureActivityGeneric.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    /// <summary>
    /// Provide the base integration to the Azure Service Management API for all activities.
    /// </summary>
    /// <typeparam name="T">Return type of the activity.</typeparam>
    public abstract class BaseAzureActivity<T> : BaseCodeActivity<T>
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
        /// Facade over Azure service management interface instance.
        /// </summary>
        public IServiceManagement Channel
        {
            get
            {
                return (this.RemoteChannel != null) ? this.RemoteChannel.Channel : null;
            }
        }

        /// <summary>
        /// Facade to the Azure client certificate loaded from the local store.
        /// </summary>
        public X509Certificate2 ManagementCertificate
        {
            get
            {
                return (this.RemoteChannel != null) ? this.RemoteChannel.ManagementCertificate : null;
            }
        }

        /// <summary>
        /// Gets or sets the Azure REST API Channel.
        /// </summary>
        internal ChannelManager RemoteChannel { get; set; }

        /// <summary>
        /// Prevent inheritance of the method.  Bind required parameters.
        /// </summary>
        /// <returns>A value of the generic type.</returns>
        protected sealed override T InternalExecute()
        {
            // Initialize the channel to Azure.
            try
            {
                this.RemoteChannel = new ChannelManager().InitializeChannel(this.CertificateThumbprintId.Get(this.ActivityContext));
            }
            catch (System.ServiceModel.CommunicationException)
            {
                this.LogBuildError("Unable to connect to the Azure Service.");
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Unable to obtain certificate from the CURRENT_USER\\MY store"));
            }

            // Execute the activity body
            return this.AzureExecute();
        }

        /// <summary>
        /// AzureExecute method which Azure-specific activities should implement
        /// </summary>
        /// <returns>A value of generic return type.</returns>
        protected abstract T AzureExecute();

        /// <summary>
        /// Execute a call to the Azure service, with retry logic for common failures.
        /// </summary>
        /// <param name="call">The API call to execute.</param>
        protected void RetryCall(Action<string> call)
        {
            this.RemoteChannel.RetryCall(this.SubscriptionId.Get(this.ActivityContext), call);
        }

        /// <summary>
        /// Execute a call to the Azure service, with retry logic for common failures.
        /// </summary>
        /// <typeparam name="TResult">Expected return type of the service call.</typeparam>
        /// <param name="call">The API call to execute.</param>
        /// <returns>The result of the API call.</returns>
        protected TResult RetryCall<TResult>(Func<string, TResult> call)
        {
            return this.RemoteChannel.RetryCall(this.SubscriptionId.Get(this.ActivityContext), call);
        }
    }
}