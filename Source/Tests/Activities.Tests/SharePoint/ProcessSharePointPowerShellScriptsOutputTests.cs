using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsBuildExtensions.Activities.SharePoint;

namespace TfsBuildExtensions.Activities.Tests.SharePoint
{
    [TestClass]
    public class ProcessSharePointPowerShellScriptsOutputTests
    {

        [TestMethod]
        public void Cannot_process_a_line_with_no_values_from_getsolution()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, "  ,  ,          ");

            //assert
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Cannot_process_an_empty_line_from_getsolution()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, string.Empty);

            //assert
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Can_process_single_line_from_getsolution()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, "  visualwebpartproject1.wsp  ,    4804dbf0-8a04-4ee9-92f9-d671f2cfd069 ,True          ");

            //assert
            Assert.AreEqual(@"Name: [visualwebpartproject1.wsp], ID [4804dbf0-8a04-4ee9-92f9-d671f2cfd069], Deployed [True]", actual[0].ToString());
        }

        [TestMethod]
        public void Can_process_multi_line_from_getsolution()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetSolution, "  visualwebpartproject1.wsp,      4804dbf0-8a04-4ee9-92f9-d671f2cfd069 ,True          " + Environment.NewLine + "  visualwebpartproject2.wsp   ,   FFC41F41-F86D-490A-84D7-A1EB4F1FA1E6, True          ");

            //assert
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(@"Name: [visualwebpartproject1.wsp], ID [4804dbf0-8a04-4ee9-92f9-d671f2cfd069], Deployed [True]", actual[0].ToString());
            Assert.AreEqual(@"Name: [visualwebpartproject2.wsp], ID [ffc41f41-f86d-490a-84d7-a1eb4f1fa1e6], Deployed [True]", actual[1].ToString());
        }


        [TestMethod]
        public void Can_process_multi_line_from_getfeature()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetFeature, "PublishingStapling, 001f4bd7-746d-403b-aa09-a6cc43de7942" + Environment.NewLine + "BasicWebParts, 00bfea71-1c5e-4a24-b310-ba51c3eb7a57" + Environment.NewLine + "XmlFormLibrary,  00bfea71-1e1d-4562-b56a-f05371bb0115 " + Environment.NewLine + "  LinksList,  00bfea71-2062-426c-90bf-714c59600103 " + Environment.NewLine + " workflowProcessList,00bfea71-2d77-4a75-9fca-76516689e21a " + Environment.NewLine + "  GridList ,  00bfea71-3a1d-41d3-a0ee-651d11570120 ");

            //assert
            Assert.AreEqual(6, actual.Length);
            Assert.AreEqual(@"Name: [PublishingStapling], ID [001f4bd7-746d-403b-aa09-a6cc43de7942], Deployed [True]", actual[0].ToString());
            Assert.AreEqual(@"Name: [BasicWebParts], ID [00bfea71-1c5e-4a24-b310-ba51c3eb7a57], Deployed [True]", actual[1].ToString());
        }

        [TestMethod]
        public void Cannot_process_a_line_with_no_values_from_getfeature()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetFeature, "  ,  ,          ");

            //assert
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Cannot_process_an_empty_line_from_getfeature()
        {
            //arrange

            //act
            var actual = SharePointDeployment.ProcessPowerShellOutput(SharePointAction.GetFeature, string.Empty);

            //assert
            Assert.AreEqual(0, actual.Length);
        }

    }
}
