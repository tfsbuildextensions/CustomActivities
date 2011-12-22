//-----------------------------------------------------------------------
// <copyright file="BaseAzureAsynchronousActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using System.Activities;
    using System.ServiceModel.Web;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    /// <summary>
    /// Provide support for Azure asynchronous activities that return an operation identifier for later polling.
    /// </summary>
    public abstract class BaseAzureAsynchronousActivity : BaseAzureActivity<string>
    {
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
    }
}