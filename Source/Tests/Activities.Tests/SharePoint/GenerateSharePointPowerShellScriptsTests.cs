using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsBuildExtensions.Activities.SharePoint;

namespace TfsBuildExtensions.Activities.Tests.SharePoint
{
    [TestClass]
    public class GenerateSharePointPowerShellScriptsTests
    {
        [TestMethod]
        public void Can_generate_a_command_for_a_remote_machine()
        {
            //arrange

            //act
            var actual = SharePointDeployment.GeneratePowerShellScript("serverName", SharePointAction.AddSolution, "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, string.Empty);

            //assert
            Assert.AreEqual(@"invoke-command -computername serverName {Add-PsSnapin Microsoft.SharePoint.PowerShell; Add-SPSolution -LiteralPath 'c:\my files\sharepoint.wsp'}", actual);
        }

        [TestMethod]
        public void Can_generate_a_command_for_a_remote_machine_with_extra_parameters()
        {
            //arrange

            //act
            var actual = SharePointDeployment.GeneratePowerShellScript("serverName", SharePointAction.AddSolution, "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, "-madeup1 parma -madeup2 'abc xyz'");

            //assert
            Assert.AreEqual(@"invoke-command -computername serverName {Add-PsSnapin Microsoft.SharePoint.PowerShell; Add-SPSolution -LiteralPath 'c:\my files\sharepoint.wsp' -madeup1 parma -madeup2 'abc xyz'}", actual);
        }

        [TestMethod]
        public void Can_generate_an_addsolution_for_a_local_machine()
        {
            //arrange

            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.AddSolution, "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Add-SPSolution -LiteralPath 'c:\my files\sharepoint.wsp'", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_with_Gacdeploy_with_webApp()
        {
            //arrange

            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.InstallSolution, "sharepoint.wsp", "http://spsite", string.Empty, string.Empty, true, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPSolution –Identity sharepoint.wsp –WebApplication http://spsite -GACDeployment", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_without_Gacdeploy_or_webApp()
        {
            //arrange

            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.InstallSolution, "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPSolution –Identity sharepoint.wsp", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_without_Gacdeploy_or_webApp_but_with_force()
        {
            //arrange

            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.InstallSolution, "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, true, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPSolution –Identity sharepoint.wsp -Force", actual);
        }


        [TestMethod]
        public void Can_generate_an_upgradesolution_for_a_local_machine_without_Gacdeploy()
        {
            //arrange


            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UpdateSolution, "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Update-SPSolution –Identity sharepoint.wsp –LiteralPath 'c:\my files\sharepoint.wsp'", actual);
        }

        [TestMethod]
        public void Can_generate_an_upgradesolution_for_a_local_machine_with_Gacdeploy()
        {
            //arrange


            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UpdateSolution, "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, true, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Update-SPSolution –Identity sharepoint.wsp –LiteralPath 'c:\my files\sharepoint.wsp' -GACDeployment", actual);
        }

        [TestMethod]
        public void Can_generate_an_upgradesolution_for_a_local_machine_with_Gacdeploy_and_force()
        {
            //arrange


            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UpdateSolution, "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, true, true, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Update-SPSolution –Identity sharepoint.wsp –LiteralPath 'c:\my files\sharepoint.wsp' -GACDeployment -Force", actual);
        }


         [TestMethod]
        public void Can_generate_an_uninstallsolution_for_a_local_machine()
        {
            //arrange


            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UninstallSolution, "sharepoint.wsp", "http://sp2010", string.Empty, string.Empty, false, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Uninstall-SPSolution –Identity sharepoint.wsp -Confirm:$false –WebApplication http://sp2010", actual);
        }

        [TestMethod]
         public void Can_generate_an_uninstallsolution_for_a_local_machine_with_no_webapp()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UninstallSolution, "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Uninstall-SPSolution –Identity sharepoint.wsp -Confirm:$false", actual);
         }

         [TestMethod]
        public void Can_generate_a_removesolution_for_a_local_machine()
        {
            //arrange


            //act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.RemoveSolution, "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);

            //assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Remove-SPSolution –Identity sharepoint.wsp -Confirm:$false", actual);
        }


         [TestMethod]
         public void Can_generate_a_enable_for_a_local_machine_with_url()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.EnableFeature, string.Empty, "http://sp2010", string.Empty, "featurename", false, false, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; enable-spfeature –Identity featurename -Url http://sp2010", actual);
         }

         [TestMethod]
         public void Can_generate_a_enable_for_a_local_machine_without_url()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.EnableFeature, string.Empty, string.Empty, string.Empty, "featurename", false, false, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; enable-spfeature –Identity featurename", actual);
         }

         [TestMethod]
         public void Can_generate_a_enable_for_a_local_machine_without_url_with_force()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.EnableFeature, string.Empty, string.Empty, string.Empty, "featurename", false, true, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; enable-spfeature –Identity featurename -Force", actual);
         }

         [TestMethod]
         public void Can_generate_a_disable_for_a_local_machine_with_url()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.DisableFeature, string.Empty, "http://sp2010", string.Empty, "feature name", false, false, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; disable-spfeature –Identity feature_name -Confirm:$false -Url http://sp2010", actual);
         }

         [TestMethod]
         public void Can_generate_a_disable_for_a_local_machine_without_url()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.DisableFeature, string.Empty, string.Empty, string.Empty, "featurename", false, false, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; disable-spfeature –Identity featurename -Confirm:$false", actual);
         }

         [TestMethod]
         public void Can_generate_a_disable_for_a_local_machine_without_url_with_force()
         {
             //arrange


             //act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.DisableFeature, string.Empty, string.Empty, string.Empty, "featurename", false, true, string.Empty);

             //assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; disable-spfeature –Identity featurename -Confirm:$false -Force", actual);
         }

         [TestMethod]
         public void Can_generate_a_get_feature_status_for_a_local_machine_by_id()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetFeature, string.Empty, string.Empty, string.Empty, "CB08B62C-20DD-4C69-B100-D80770BEB88E", false, true, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; $result=get-spfeature | where {$_.id -eq 'CB08B62C-20DD-4C69-B100-D80770BEB88E'}; foreach ($line in $result) {'{0}, {1}' -f $line.Displayname, $line.Id}", str);
         }

         [TestMethod]
         public void Can_generate_a_get_feature_status_for_a_local_machine_by_name()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetFeature, string.Empty, string.Empty, string.Empty, "my feature", false, true, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; $result=get-spfeature | where {$_.displayname -eq 'my_feature'}; foreach ($line in $result) {'{0}, {1}' -f $line.Displayname, $line.Id}", str);
         }

         [TestMethod]
         public void Can_generate_a_get_feature_status_for_a_local_machine_for_all_features()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetFeature, string.Empty, string.Empty, string.Empty, string.Empty, false, true, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; $result=get-spfeature; foreach ($line in $result) {'{0}, {1}' -f $line.Displayname, $line.Id}", str);
         }

         [TestMethod]
         public void Can_generate_a_get_solution_status_for_a_local_machine__for_all_solutions()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetSolution, string.Empty, string.Empty, string.Empty, string.Empty, false, false, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; $result=get-spsolution; foreach ($line in $result) {'{0}, {1}, {2}' -f $line.Displayname, $line.Id, $Line.Deployed}", str);
         }

         [TestMethod]
         public void Can_generate_a_get_solution_status_for_a_local_machine_by_id()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetSolution, "CB08B62C-20DD-4C69-B100-D80770BEB88E", string.Empty, string.Empty, string.Empty, false, false, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; $result=get-spsolution | where {$_.id -eq 'CB08B62C-20DD-4C69-B100-D80770BEB88E'}; foreach ($line in $result) {'{0}, {1}, {2}' -f $line.Displayname, $line.Id, $Line.Deployed}", str);
         }

         [TestMethod]
         public void Can_generate_a_get_solution_status_for_a_local_machine_by_name()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetSolution, "wspname.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; $result=get-spsolution | where {$_.name -eq 'wspname.wsp'}; foreach ($line in $result) {'{0}, {1}, {2}' -f $line.Displayname, $line.Id, $Line.Deployed}", str);
         }





    }
}
