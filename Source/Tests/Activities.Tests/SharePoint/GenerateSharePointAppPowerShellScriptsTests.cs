using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsBuildExtensions.Activities.Tests.SharePoint
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.SharePoint;

    [TestClass]
    public class GenerateSharePointAppPowerShellScriptsTests
    {
        [TestMethod]
        public void Can_generate_a_command_for_a_remote_machine()
        {
            // Arrange

            // Act
            var actual = SharePointAppDeployment.GeneratePowerShellScript("serverName", SharePointAppAction.Import_SPAppPackage, "2013", "DeveloperSite", "sharepoint.wsp", "http://localhost", @"c:\my files\sharepoint.wsp", string.Empty);

            // Assert
            Assert.AreEqual(@"invoke-command -computername serverName {Add-PsSnapin Microsoft.SharePoint.PowerShell; Import-SPAppPackage -Path 'c:\my files\sharepoint.wsp' -Site http://localhost -Source DeveloperSite}", actual);
        }

        [TestMethod]
        public void Can_generate_a_command_for_a_remote_machine_with_extra_parameters()
        {
            // Arrange

            // Act
            var actual = SharePointAppDeployment.GeneratePowerShellScript("serverName", SharePointAppAction.Import_SPAppPackage, "2013", "DeveloperSite", "sharepoint.wsp", "http://localhost", @"c:\my files\sharepoint.wsp", "-madeup1 parma -madeup2 'abc xyz'");

            // Assert
            Assert.AreEqual(@"invoke-command -computername serverName {Add-PsSnapin Microsoft.SharePoint.PowerShell; Import-SPAppPackage -LiteralPath 'c:\my files\sharepoint.wsp' -madeup1 parma -madeup2 'abc xyz'}", actual);
        }

        [TestMethod]
        public void Can_generate_an_importapp_for_a_local_machine()
        {
            // Arrange

            // Act
            var actual = SharePointAppDeployment.GeneratePowerShellScript(string.Empty, SharePointAppAction.Import_SPAppPackage, "2013", "DeveloperSite", "sharepoint.wsp", "http://localhost", @"c:\my files\sharepoint.wsp", string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Import-SPAppPackage -Path 'c:\my files\sharepoint.wsp' -Site http://localhost -Source DeveloperSite", actual);
        }

        [TestMethod]
        public void Can_generate_an_installapp_for_a_local_machine_with_webApp()
        {
            // Arrange

            // Act
            var actual = SharePointAppDeployment.GeneratePowerShellScript(string.Empty, SharePointAppAction.Install_SPApp, "2013", "DeveloperSite", "sharepoint.wsp", "http://spsite", string.Empty, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPApp –Identity sharepoint.wsp –Web http://spsite", actual);
        }

        [TestMethod]
        public void Can_generate_an_installapp_for_a_local_machine_without_webApp()
        {
            // Arrange

            // Act
            var actual = SharePointAppDeployment.GeneratePowerShellScript(string.Empty, SharePointAppAction.Install_SPApp, "2010", "OldVersion", "sharepoint.wsp", string.Empty, string.Empty, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPApp –Identity sharepoint.wsp", actual);
        }

        [TestMethod]
        public void Can_generate_an_installsolution_for_a_local_machine_webApp_but_with_force()
        {
            // Arrange

            // Act
            var actual = SharePointAppDeployment.GeneratePowerShellScript(string.Empty, SharePointAppAction.Install_SPApp, "2010", "OldVersion", "sharepoint.wsp", string.Empty, string.Empty, string.Empty);

            // Assert
            Assert.AreEqual(@"Add-PsSnapin Microsoft.SharePoint.PowerShell; Install-SPApp –Identity sharepoint.wsp -Force", actual);
        }        
    }
}

