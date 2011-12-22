//-----------------------------------------------------------------------
// <copyright file="Running_Code_Metrics.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.CodeMetrics.Extended;
    
    /// <summary>
    /// This is a test class for CodeMetricsTest and is intended
    /// to contain all CodeMetricsTest Unit Tests
    /// </summary>
    [TestClass]
    public class RunningCodeMetrics
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        private bool MetricsInstalled { get; set; }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.MetricsInstalled =
                File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\metrics.exe"));
        }

        #endregion

        /// <summary>
        /// A test for ProcessMetrics
        /// </summary>
        [TestMethod]
        public void ShouldFailWhenMetricsNotInstalled()
        {
            if (this.MetricsInstalled)
            {
                Assert.IsTrue(true);
                return;
            }

            var mock = new Moq.Mock<IMetricsLogger>();

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            var target = MetricsInvoker.Create(new List<string> { "*.dll" }, @"c:\binaries", "Metrics.exe", mock.Object);
            Assert.IsNull(target);
            mock.Verify(m => m.LogError(Moq.It.Is<string>(s => s.Contains("Could not locate"))));
        }

        [TestMethod]
        public void ShouldNotFailWhenMetricsIsInstalled()
        {
            if (!this.MetricsInstalled)
            {
                Assert.IsTrue(true);
                return;
            }

            var mock = new Moq.Mock<IMetricsLogger>();

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            var target = MetricsInvoker.Create(new List<string> { "*.dll" }, @"c:\binaries", "Metrics.exe", mock.Object);
            Assert.IsNotNull(target);
            mock.Verify(m => m.LogError(Moq.It.IsAny<string>()), Moq.Times.Never());
        }

        [TestMethod]
        public void ShouldCreateProperArguments()
        {
            if (!this.MetricsInstalled)
            {
                Assert.IsTrue(true);
                return;
            }

            const string RootPath = @"c:\binaries";
            const string Output = "MetricsOutput.xml";

            var mock = new Moq.Mock<IMetricsLogger>();

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            var target = MetricsInvoker.Create(new List<string> { "*.dll", "*.exe" }, RootPath, Output, mock.Object);
            Assert.IsNotNull(target);
            Assert.AreEqual(string.Format("/f:\"{0}\\*.dll\" /f:\"{0}\\*.exe\" /out:\"{1}\"", RootPath, Output), target.Argument);
        }
    }
}
