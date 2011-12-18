//-----------------------------------------------------------------------
// <copyright file="ProcessSharePointPowerShellScriptsOutputTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Tests.SharePoint
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.SharePoint;

    [TestClass]
    public class ProcessSharePointPowerShellScriptsOutputTests
    {
        [TestMethod]
        public void Cannot_process_a_line_with_no_values_from_getsolution()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, "  ,  ,          ");

            // Assert
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Cannot_process_an_empty_line_from_getsolution()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, string.Empty);

            // Assert
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Can_process_single_line_from_getsolution()
        {
            // Arrange
            var data = "DisplayName : visualwebpartproject1.wsp" + Environment.NewLine +
              "Deployed    : True" + Environment.NewLine +
              "Id          : 4804dbf0-8a04-4ee9-92f9-d671f2cfd069";
              
            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, data);

            // Assert
            Assert.AreEqual(@"Name: [visualwebpartproject1.wsp], ID [4804dbf0-8a04-4ee9-92f9-d671f2cfd069], Deployed [True]", actual[0].ToString());
        }

        [TestMethod]
        public void Can_process_multi_line_from_getsolution()
        {
            // Arrange
            var data = "DisplayName : visualwebpartproject1.wsp" + Environment.NewLine +
                "Deployed    : True" + Environment.NewLine +
                "Id          : 4804dbf0-8a04-4ee9-92f9-d671f2cfd069" + Environment.NewLine +
                Environment.NewLine +
                "DisplayName : visualwebpartproject2.wsp" + Environment.NewLine +
                "Deployed    : True" + Environment.NewLine +
                "Id          : ffc41f41-f86d-490a-84d7-a1eb4f1fa1e6";

            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, data);

            // Assert
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(@"Name: [visualwebpartproject1.wsp], ID [4804dbf0-8a04-4ee9-92f9-d671f2cfd069], Deployed [True]", actual[0].ToString());
            Assert.AreEqual(@"Name: [visualwebpartproject2.wsp], ID [ffc41f41-f86d-490a-84d7-a1eb4f1fa1e6], Deployed [True]", actual[1].ToString());
        }

        [TestMethod]
        public void Can_process_multi_line_from_getfeature()
        {
            // Arrange
            var data =
                "DisplayName : PublishingStapling" + Environment.NewLine +
                "Id          : 001f4bd7-746d-403b-aa09-a6cc43de7942" + Environment.NewLine +
                "" + Environment.NewLine +
                "DisplayName : BasicWebParts" + Environment.NewLine +
                "Id          : 00bfea71-1c5e-4a24-b310-ba51c3eb7a57";

            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetFeature, data);

            // Assert
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(@"Name: [PublishingStapling], ID [001f4bd7-746d-403b-aa09-a6cc43de7942], Deployed [True]", actual[0].ToString());
            Assert.AreEqual(@"Name: [BasicWebParts], ID [00bfea71-1c5e-4a24-b310-ba51c3eb7a57], Deployed [True]", actual[1].ToString());
        }


        [TestMethod]
        public void Cannot_process_a_line_with_no_values_from_getfeature()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetFeature, "  ,  ,          ");

            // Assert
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Cannot_process_an_empty_line_from_getfeature()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetFeature, string.Empty);

            // Assert
            Assert.AreEqual(0, actual.Length);
        }
    }
}
