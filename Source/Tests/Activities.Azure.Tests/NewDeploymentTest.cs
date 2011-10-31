// ----------------------------------------------------------------------------
// <copyright file="NewDeploymentTest.cs" company="Shingl, inc.">
// Copyright (c) 2011 All Rights Reserved
// </copyright>
// <author>Stuart Schaefer</author>
// <email>stuart@shingl.com</email>
// <date>2011-08-12</date>
// ----------------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Azure.HostedServices;

    [TestClass]
    public class NewDeploymentTest
    {
        [TestMethod]
        public void NewDeployment_ValidDeployment_DeploymentContext()
        {
            // Arrange           
            //"128791C3DE5076FBA441F2EAEE72B4F932C9B377",
            var target = new NewDeployment
            {
                SubscriptionId = "5889ed88-7690-4ecb-9c16-bd76e737ddd3",
                CertificateThumbprintId = "76619850EDE5B64F41F084BA2C08C7FCF1AB1F41",
                Slot = "Production",
                ServiceName = "shinglworkflow",
                ConfigurationFilePath = "C:\\Users\\stuart\\Desktop\\Shingl.Production.WorkflowHost.cscfg",
                // DeploymentName = "workflow",
                DeploymentLabel = "Shingl.Production.WorkflowHost - 20110813_001132", // Shows up as deployment name in Azure
                PackageUrl = "http://shingl.blob.core.windows.net/mydeployments/20110813_001132_Shingl.Role.WorkflowHost.cspkg"
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(target, new Dictionary<string, object>());
            string operationId = (string) results["OperationId"];

            // Assert
            Assert.IsNotNull(operationId);
        }
    }
}
