//-----------------------------------------------------------------------
// <copyright file="FxCopTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.CodeQuality;

    /// <summary>
    /// FxCopTests
    /// </summary>
    [TestClass]
    public class FxCopTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Zip Files by Path
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FxCopBasicPassTest()
        {
            // Initialise Instance
            var target = new FxCop { FxCopPath = @"D:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe", OutputFile = @"d:\a\fxcoplog1.txt" };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { @"D:\Projects\MSBuildExtensionPack\Releases\4.0.4.0\Main\BuildBinaries\MSBuild.ExtensionPack.StyleCop.dll" } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            // Invoke the Workflow
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke(parameters);

            // Test the result
            Assert.AreEqual("false", actual["AnalysisFailed"].ToString());
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FxCopBasicFailTest()
        {
            // Initialise Instance
            var target = new FxCop { FxCopPath = @"D:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe", OutputFile = @"d:\a\fxcoplog1.txt" };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { @"C:\Users\Michael\Documents\visual studio 2010\Projects\FxCopFailTest\FxCopFailTest\bin\Debug\FxCopFailTest.dll" } },
                { "Rules", new[] { @"+D:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\Rules\DesignRules.dll" } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            // Invoke the Workflow
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke(parameters);

            // Test the result
            Assert.AreEqual("true", actual["AnalysisFailed"].ToString());
        }
    }
}
