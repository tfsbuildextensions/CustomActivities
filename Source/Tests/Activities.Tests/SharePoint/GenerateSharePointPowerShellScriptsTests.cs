//-----------------------------------------------------------------------
// <copyright file="GenerateSharePointPowerShellScriptsTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Tests.SharePoint
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.SharePoint;

    [TestClass]
    public class GenerateSharePointPowerShellScriptsTests
    {
        [TestMethod]
        public void Can_generate_a_command_for_a_remote_machine()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript("serverName", SharePointAction.AddSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, string.Empty);

            // Assert
            Assert.AreEqual(@"invoke-command -computername serverName {Add-PsSnapin Microsoft.SharePoint.PowerShell; Add-SPSolution -LiteralPath 'c:\my files\sharepoint.wsp'}", actual);
        }

        [TestMethod]
        public void Can_generate_a_command_for_a_remote_machine_with_extra_parameters()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript("serverName", SharePointAction.AddSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, "-madeup1 parma -madeup2 'abc xyz'");

            // Assert
            Assert.AreEqual(@"invoke-command -computername serverName {Add-PsSnapin Microsoft.SharePoint.PowerShell; Add-SPSolution -LiteralPath 'c:\my files\sharepoint.wsp' -madeup1 parma -madeup2 'abc xyz'}", actual);
        }

        [TestMethod]
        public void Can_generate_an_addsolution_for_a_local_machine()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.AddSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Add-SPSolution -LiteralPath 'c:\my files\sharepoint.wsp'", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_with_Gacdeploy_with_webApp()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.InstallSolution, "2010", "OldVersion", "sharepoint.wsp", "http://spsite", string.Empty, string.Empty, true, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPSolution –Identity sharepoint.wsp –WebApplication http://spsite -GACDeployment", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_without_Gacdeploy_or_webApp()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.InstallSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPSolution –Identity sharepoint.wsp", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_without_Gacdeploy_or_webApp_but_with_force()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.InstallSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, true, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPSolution –Identity sharepoint.wsp -Force", actual);
        }

        [TestMethod]
        public void Can_generate_an_upgradesolution_for_a_local_machine_without_Gacdeploy()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UpdateSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, false, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Update-SPSolution –Identity sharepoint.wsp –LiteralPath 'c:\my files\sharepoint.wsp'", actual);
        }

        [TestMethod]
        public void Can_generate_an_upgradesolution_for_a_local_machine_with_Gacdeploy()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UpdateSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, true, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Update-SPSolution –Identity sharepoint.wsp –LiteralPath 'c:\my files\sharepoint.wsp' -GACDeployment", actual);
        }

        [TestMethod]
        public void Can_generate_an_upgradesolution_for_a_local_machine_with_Gacdeploy_and_force()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UpdateSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, @"c:\my files\sharepoint.wsp", string.Empty, true, true, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Update-SPSolution –Identity sharepoint.wsp –LiteralPath 'c:\my files\sharepoint.wsp' -GACDeployment -Force", actual);
        }

         [TestMethod]
        public void Can_generate_an_uninstallsolution_for_a_local_machine()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UninstallSolution, "2010", "OldVersion", "sharepoint.wsp", "http://sp2010", string.Empty, string.Empty, false, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Uninstall-SPSolution –Identity sharepoint.wsp -Confirm:$false –WebApplication http://sp2010", actual);
        }

        [TestMethod]
         public void Can_generate_an_uninstallsolution_for_a_local_machine_with_no_webapp()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.UninstallSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Uninstall-SPSolution –Identity sharepoint.wsp -Confirm:$false", actual);
         }

         [TestMethod]
        public void Can_generate_a_removesolution_for_a_local_machine()
        {
            // Arrange

            // Act
            var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.RemoveSolution, "2010", "OldVersion", "sharepoint.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Remove-SPSolution –Identity sharepoint.wsp -Confirm:$false", actual);
        }

         [TestMethod]
         public void Can_generate_a_enable_for_a_local_machine_with_url()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.EnableFeature, "2010", "OldVersion", string.Empty, "http://sp2010", string.Empty, "featurename", false, false, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; enable-spfeature –Identity featurename -Url http://sp2010", actual);
         }

         [TestMethod]
         public void Can_generate_a_enable_for_a_local_machine_without_url()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.EnableFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, "featurename", false, false, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; enable-spfeature –Identity featurename", actual);
         }

         [TestMethod]
         public void Can_generate_a_enable_for_a_local_machine_without_url_with_force()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.EnableFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, "featurename", false, true, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; enable-spfeature –Identity featurename -Force", actual);
         }

         [TestMethod]
         public void Can_generate_a_disable_for_a_local_machine_with_url()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.DisableFeature, "2010", "OldVersion", string.Empty, "http://sp2010", string.Empty, "feature name", false, false, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; disable-spfeature –Identity feature_name -Confirm:$false -Url http://sp2010", actual);
         }

         [TestMethod]
         public void Can_generate_a_disable_for_a_local_machine_without_url()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.DisableFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, "featurename", false, false, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; disable-spfeature –Identity featurename -Confirm:$false", actual);
         }

         [TestMethod]
         public void Can_generate_a_disable_for_a_local_machine_without_url_with_force()
         {
             // Arrange

             // Act
             var actual = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.DisableFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, "featurename", false, true, string.Empty);

             // Assert
             Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; disable-spfeature –Identity featurename -Confirm:$false -Force", actual);
         }

         [TestMethod]
         public void Can_generate_a_get_feature_status_for_a_local_machine_by_id()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, "CB08B62C-20DD-4C69-B100-D80770BEB88E", false, true, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; get-spfeature | where {$_.id -eq 'CB08B62C-20DD-4C69-B100-D80770BEB88E'} | fl -property Displayname, Id ;", str);
         }

         [TestMethod]
         public void Can_generate_a_get_feature_status_for_a_local_machine_by_name()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, "my feature", false, true, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; get-spfeature | where {$_.displayname -eq 'my_feature'} | fl -property Displayname, Id ;", str);
         }

         [TestMethod]
         public void Can_generate_a_get_feature_status_for_a_local_machine_for_all_features()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetFeature, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, string.Empty, false, true, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; get-spfeature | fl -property Displayname, Id ;", str);
         }

         [TestMethod]
         public void Can_generate_a_get_solution_status_for_a_local_machine__for_all_solutions()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetSolution, "2010", "OldVersion", string.Empty, string.Empty, string.Empty, string.Empty, false, false, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; get-spsolution | fl -property Displayname, Deployed, Id ;", str);
         }

         [TestMethod]
         public void Can_generate_a_get_solution_status_for_a_local_machine_by_id()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetSolution, "2010", "OldVersion", "CB08B62C-20DD-4C69-B100-D80770BEB88E", string.Empty, string.Empty, string.Empty, false, false, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; get-spsolution | where {$_.id -eq 'CB08B62C-20DD-4C69-B100-D80770BEB88E'} | fl -property Displayname, Deployed, Id ;", str);
         }

         [TestMethod]
         public void Can_generate_a_get_solution_status_for_a_local_machine_by_name()
         {
             string str = SharePointDeployment.GeneratePowerShellScript(string.Empty, SharePointAction.GetSolution, "2010", "OldVersion", "wspname.wsp", string.Empty, string.Empty, string.Empty, false, false, string.Empty);
             Assert.AreEqual<string>("Add-PsSnapin Microsoft.SharePoint.PowerShell; get-spsolution | where {$_.name -eq 'wspname.wsp'} | fl -property Displayname, Deployed, Id ;", str);
         }
    }
}
