//-----------------------------------------------------------------------
// <copyright file="ReadAssemblyInfoTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Framework;

    /// <summary>
    /// This is a test class for ReadAssemblyInfo and is intended
    /// to contain all ReadAssemblyInfo Unit Tests
    /// </summary>
    [TestClass]
    public class ReadAssemblyInfoTests
    {
        #region Fields

        // test AssemblyInfo file prefix.
        private const string TestFilePrefix = @"TestAssemblyInfo";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Setup / Cleanup

        /// <summary>
        /// Cleanups after each unit test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            foreach (var path in Directory.GetFiles(".", TestFilePrefix + "*.*"))
            {
                File.Delete(path);
            }
        }

        #endregion

        #region Get action Tests

        /// <summary>
        /// Tests if the attribute values are read.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void ReadAssemblyInfo_ReadAttributes_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new ReadAssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "File", TestFilePrefix + ".cs" },
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual("AssemblyCompanyValue", (string)actual["AssemblyCompany"]);
            Assert.AreEqual("AssemblyConfigurationValue", (string)actual["AssemblyConfiguration"]);
            Assert.AreEqual("AssemblyCopyrightValue", (string)actual["AssemblyCopyright"]);
            Assert.AreEqual("AssemblyDescriptionValue", (string)actual["AssemblyDescription"]);
            Assert.AreEqual("AssemblyProductValue", (string)actual["AssemblyProduct"]);
            Assert.AreEqual("AssemblyTitleValue", (string)actual["AssemblyTitle"]);
            Assert.AreEqual("AssemblyTrademarkValue", (string)actual["AssemblyTrademark"]);
            Assert.AreEqual("AssemblyCultureValue", (string)actual["AssemblyCulture"]);
            Assert.IsNotNull(actual["AssemblyDelaySign"]);
            Assert.IsTrue(((bool?)actual["AssemblyDelaySign"]).Value);
            Assert.IsNotNull(actual["Guid"]);
            Assert.AreEqual(new System.Guid("B0EAC358-5AB5-45DE-9975-E1D8D8030944"), ((System.Guid?)actual["Guid"]).Value);
            Assert.AreEqual("AssemblyKeyFileValue", (string)actual["AssemblyKeyFile"]);
            Assert.AreEqual("AssemblyKeyNameValue", (string)actual["AssemblyKeyName"]);
            Assert.IsNotNull(actual["CLSCompliant"]);
            Assert.IsTrue(((bool?)actual["CLSCompliant"]).Value);
            Assert.IsNotNull(actual["ComVisible"]);
            Assert.IsTrue(((bool?)actual["ComVisible"]).Value);
            Assert.AreEqual("1.2.0.0", (string)actual["AssemblyVersion"]);
            Assert.AreEqual("1.2.3.4", (string)actual["AssemblyFileVersion"]);
            Assert.AreEqual("AssemblyInformationalVersionValue", (string)actual["AssemblyInformationalVersion"]);
        }

        /// <summary>
        /// Tests if the attribute values are read.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\EmptyAssemblyInfo.cs")]
        public void ReadAssemblyInfo_ReadAttributes_WhenExecuteInvokedWithEmptyFile()
        {
            // arrange
            File.Copy(@"EmptyAssemblyInfo.cs", TestFilePrefix + ".cs", true);

            var target = new ReadAssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "File", TestFilePrefix + ".cs" },
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.IsNull(actual["AssemblyCompany"]);
            Assert.IsNull(actual["AssemblyConfiguration"]);
            Assert.IsNull(actual["AssemblyCopyright"]);
            Assert.IsNull(actual["AssemblyDescription"]);
            Assert.IsNull(actual["AssemblyProduct"]);
            Assert.IsNull(actual["AssemblyTitle"]);
            Assert.IsNull(actual["AssemblyTrademark"]);
            Assert.IsNull(actual["AssemblyCulture"]);
            Assert.IsNull(actual["AssemblyDelaySign"]);
            Assert.IsNull(actual["Guid"]);
            Assert.IsNull(actual["AssemblyKeyFile"]);
            Assert.IsNull(actual["AssemblyKeyName"]);
            Assert.IsNull(actual["CLSCompliant"]);
            Assert.IsNull(actual["ComVisible"]);
            Assert.IsNull(actual["AssemblyVersion"]);
            Assert.IsNull(actual["AssemblyFileVersion"]);
            Assert.IsNull(actual["AssemblyInformationalVersion"]);
        }

        #endregion
    }
}
