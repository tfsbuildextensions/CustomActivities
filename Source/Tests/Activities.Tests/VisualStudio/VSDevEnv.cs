//-----------------------------------------------------------------------
// <copyright file="FileTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.VisualStudio;

    /// <summary>
    /// This is a test class for SqlExecuteTest and is intended
    /// to contain all SqlExecuteTest Unit Tests
    /// </summary>
    [TestClass]
    public class VSDevEnvTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void DevEnvNoPlatformTest()
        {
            // Initialise Instance
            var target = new TfsBuildExtensions.Activities.VisualStudio.VSDevEnv { Action = VSDevEnvAction.Rebuild, Configuration = "Debug", FilePath = @"C:\Users\Michael\Documents\visual studio 2012\Projects\ConsoleApplication1\ConsoleApplication1.sln", OutputFile=@"D:\a\log.txt", Platform = "AnyCPU"};

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke();

            // note no result here... this test s for manual testing purposes
        }
    }
}
