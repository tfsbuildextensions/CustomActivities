//-----------------------------------------------------------------------
// <copyright file="GetDeploymentTest.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Azure.HostedServices;

    [TestClass]
    public class GetDeploymentTest
    {
        [TestMethod]
        public void GetDeployment_ValidDeployment_DeploymentContext()
        {
            // Arrange           
            var target = new GetDeployment
            {
                Slot = "Production",
                ServiceName = string.Empty,
                SubscriptionId = string.Empty,
                CertificateThumbprintId = string.Empty
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(target, new Dictionary<string, object>());
            Model.DeploymentInfoContext ctx = results["DeploymentContext"] as Model.DeploymentInfoContext;

            // Assert
            Assert.IsNotNull(ctx);
        }

        [TestMethod]
        public void GetHostedServices_Subscription_List()
        {
            // Arrange           
            var target = new GetHostedServices
            {
                SubscriptionId = string.Empty,
                CertificateThumbprintId = string.Empty
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(target, new Dictionary<string, object>());
            var services = results["ServiceList"] as HostedServiceList;

            // Assert
            Assert.IsNotNull(services);
        }
    }
}
