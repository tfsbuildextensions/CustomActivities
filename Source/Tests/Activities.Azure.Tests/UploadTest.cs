//-----------------------------------------------------------------------
// <copyright file="UploadTest.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Tests
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Azure.HostedServices;
    using TfsBuildExtensions.Activities.Azure.StorageAccounts;

    [TestClass]
    public class UploadTest
    {
        [TestMethod]
        public void Upload_ValidPackage_PackageUrl()
        {
            // Arrange 
            var keys = new Variable<StorageServiceKeys> { Default = null, Name = "Keys" };

            var sequence = new Sequence
            {
                Variables =
                {
                    keys
                },

                Activities =
                {
                    new GetStorageKeys
                    {
                        SubscriptionId = "",
                        CertificateThumbprintId = "",
                        ServiceName = "",
                        StorageKeys = keys
                    },
                    new UploadPackageToBlobStorage
                    {
                        SubscriptionId = "",
                        CertificateThumbprintId = "",
                        LocalPackagePath = "",
                        StorageServiceName = "",
                        StorageKeys = keys
                    }
                }
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(sequence, new Dictionary<string, object>());
        }
    }
}
