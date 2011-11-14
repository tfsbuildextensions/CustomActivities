//-----------------------------------------------------------------------
// <copyright file="GenerateSharePointPowerShellScriptsTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------


namespace TfsBuildExtensions.Activities.Tests.ClickOnce
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.ClickOnce;
    using System.IO;
    using System.Activities;

    [TestClass]
    public class ClickOnceTests
    {
        [TestMethod]
        public void ClickOnce_Can_DeployApplication()
        {
            // cleanup
            if (File.Exists(@"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug\TestApp.application"))
                System.IO.File.Delete(@"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug\TestApp.application");
            if (File.Exists(@"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug\TestApp.exe.manifest"))
                System.IO.File.Delete(@"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug\TestApp.exe.manifest");
            if (Directory.Exists(@"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug\app.publish"))
                Directory.Delete(@"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug\app.publish", true);

            var workflow = new ClickOnceDeployment();
         
            workflow.OnlineOnly = false;
            workflow.ApplicationName = "TestApp";
            workflow.BinLocation = @"C:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\bin\Debug";
            workflow.Version = "1.0.0.1";
            workflow.MageFilePath = @"C:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools\Mage.exe";
            workflow.CertFilePath = @"c:\tfs\test2\CustomBuildSource\CustomBuildArtifacts\TestApp\TestApp_TemporaryKey.pfx";
            workflow.CertPassword = "Mike";
            workflow.ManifestCertificateThumbprint = "9C6B1A418C9DF9E42F31AE811B659F4773FF6AA3";
            workflow.PublishLocation = @"\\dlvrn2010md\builddropfolder\testc1";
            workflow.InstallLocation = "http://localhost:8055/Testclickonce2";
            workflow.Publisher = "CompanyA";
            workflow.TargetFrameworkVersion = "4.0";
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            var invokeResponse = workflowInvoker.Invoke();
        }
    }
}
