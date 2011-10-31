//-----------------------------------------------------------------------
// <copyright file="ServiceClientFactory.cs" company="Shingl, inc.">
//   Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>Stuart Schaefer</author>
// <email>stuart@shingl.com</email>
// <date>2010-11-18</date>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
    /// Responsible for creating client proxies for service interfaces.
    /// </summary>
    /// <remarks>
    /// Ideally our implementation would use ClientBase to gain the benefit of channel creation caching and 
    /// channel reuse, but caching is disabled for non-configuration file based clients.
    /// </remarks>
    /// <typeparam name="TChannel">Interface contract type.</typeparam>
    public abstract class ServiceClientFactory<TChannel> : IDisposable
        where TChannel : IClientChannel
    {
        /// <summary>
        /// The WCF channel proxy factory that we will use to create channel clients.
        /// </summary>
        private ChannelFactory<TChannel> channelFactory;

        /// <summary>
        /// Initializes a new instance of the ServiceClientFactory class.
        /// </summary>
        /// <param name="baseEndpointUri">Base address for the remote service.</param>
        protected ServiceClientFactory(Uri baseEndpointUri)
        {
            // This should ideally be a fully blown constructor, leaving as little as possible
            // to channel creation time.  Then it can cache the full metadata for the service.
            // http://readcommit.blogspot.com/2009/11/wcf-middle-tier-client-clientbase-proxy.html
            EndpointAddress remoteAddress = new System.ServiceModel.EndpointAddress(baseEndpointUri);
            if (remoteAddress.Uri.Scheme.Equals("http"))
            {
                channelFactory = new System.ServiceModel.ChannelFactory<TChannel>(
                    new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None),
                    remoteAddress);
            }
            else if (remoteAddress.Uri.Scheme.Equals("https"))
            {
                channelFactory = new System.ServiceModel.ChannelFactory<TChannel>(
                    new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport),
                    remoteAddress);
            }
            else
            {
                throw new ArgumentException("Invalid Endpoint Address.");
            }
        }

        /// <summary>
        /// Factory method for creating a WCF client of the service.
        /// </summary>
        /// <returns>Client proxy for the interface type.</returns>
        public virtual TChannel CreateClient()
        {
            return channelFactory.CreateChannel();
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "The method cannot be sealed because it cannot be an override.")]
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        /// <param name="disposing">The managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (channelFactory != null)
            {
                channelFactory.Close();
            }
        }
    }
}