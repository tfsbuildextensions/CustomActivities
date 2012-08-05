//-----------------------------------------------------------------------
// <copyright file="AssemblyInfoTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// This is a test class for AssemblyInfo and is intended
    /// to contain all AssemblyInfo Unit Tests
    /// </summary>
    [TestClass]
    public class AssemblyInfoTests
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

        #region Exception Tests

        /// <summary>
        /// Tests if a <see cref="FileNotFoundException"/> is thrown when one of the specified file is not found.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [ExpectedException(typeof(FileNotFoundException))]
        public void AssemblyInfo_ThrowsFileNotFoundException_OnUnknownFile()
        {
            // arrange
            File.Copy("AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs", TestFilePrefix + ".vb" } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);
        }

        /// <summary>
        /// Tests if a <see cref="FormatException"/> is thrown when an invalid token is specified.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [ExpectedException(typeof(FormatException))]
        public void AssemblyInfo_ThrowsFormatException_OnInvalidToken()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(invalid).3.0.0" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);
        }

        /// <summary>
        /// Tests if a <see cref="FormatException"/> is thrown when an invalid version is read from the file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (2).cs")]
        [ExpectedException(typeof(FormatException))]
        public void AssemblyInfo_ThrowsFormatException_OnInvalidAssemblyVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (2).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "4.3.0.0" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);
        }

        /// <summary>
        /// Tests if a <see cref="FormatException"/> is thrown when an invalid version is specified.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [ExpectedException(typeof(FormatException))]
        public void AssemblyInfo_ThrowsFormatException_OnInvalidSetAssemblyVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "4.3.0" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);
        }

        /// <summary>
        /// Tests if a <see cref="FormatException"/> is thrown when an invalid file version is read from the file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (2).cs")]
        [ExpectedException(typeof(FormatException))]
        public void AssemblyInfo_ThrowsFormatException_OnOldInvalidAssemblyFileVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (2).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyFileVersion", "4.3.0.0" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);
        }

        /// <summary>
        /// Tests if a <see cref="FormatException"/> is thrown when an invalid file version is specified.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [ExpectedException(typeof(FormatException))]
        public void AssemblyInfo_ThrowsFormatException_OnInvalidSetAssemblyFileVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyFileVersion", "4.3.0" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);
        }

        #endregion

        #region No updates Tests

        /// <summary>
        /// Tests if no attributes are updated when default values (<see langwork="null" />) are used.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_NoUpdates_WhenExecuteInvokedWithDefaultValues()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyCompany(\"AssemblyCompanyValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyConfiguration(\"AssemblyConfigurationValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyCopyright(\"AssemblyCopyrightValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyCulture(\"AssemblyCultureValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyDescription(\"AssemblyDescriptionValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyProduct(\"AssemblyProductValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyTitle(\"AssemblyTitleValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyTrademark(\"AssemblyTrademarkValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyDelaySign(true)]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyKeyFile(\"AssemblyKeyFileValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyKeyName(\"AssemblyKeyNameValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyVersion(\"1.2.0.0\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyFileVersion(\"1.2.3.4\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyInformationalVersion(\"AssemblyInformationalVersionValue\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: CLSCompliant(true)]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: Guid(\"B0EAC358-5AB5-45DE-9975-E1D8D8030944\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: ComVisible(true)]", StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyVersion Tests

        /// <summary>
        /// Tests if the assembly version is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(increment).$(date:yyyy).1234" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyVersion(\"1.3.{0:yyyy}.1234\")]", DateTime.Today), StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the max assembly version is computed.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (3).cs")]
        public void AssemblyInfo_ComputesMaxAssemblyVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + "1.cs", true);
            File.Copy(@"AssemblyInfo (3).cs", TestFilePrefix + "2.cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + "1.cs", TestFilePrefix + "2.cs" } },
                { "AssemblyVersion", "$(current).$(increment).$(current).$(current)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual("1.3.0.0", actual["MaxAssemblyVersion"].ToString());
        }

        /// <summary>
        /// Tests if the assembly versions enumerable is filled.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (3).cs")]
        public void AssemblyInfo_FillAssemblyVersions_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + "1.cs", true);
            File.Copy(@"AssemblyInfo (3).cs", TestFilePrefix + "2.cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + "1.cs", TestFilePrefix + "2.cs" } },
                { "AssemblyVersion", "$(current).$(increment).$(current).$(current)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var versions = actual["AssemblyVersions"] as IEnumerable<Version>;
            Assert.AreEqual(2, versions.Count());
            Assert.AreEqual("1.3.0.0", versions.ElementAt(0).ToString());
            Assert.AreEqual("1.2.0.0", versions.ElementAt(1).ToString());
        }

        /// <summary>
        /// Tests if the assembly versions enumerable is empty if no AssemblyVersion is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (4).cs")]
        public void AssemblyInfo_EmptyAssemblyVersions_WhenExecuteInvokedOnFileWithoutAssemblyVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (4).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(increment).$(current).$(current)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var versions = actual["AssemblyVersions"] as IEnumerable<Version>;
            Assert.AreEqual(0, versions.Count());
        }

        /// <summary>
        /// Tests if the assembly version is updated when a '*' wildcard character is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (5).cs")]
        public void AssemblyInfo_UpdatesAssemblyVersion_WhenExecuteInvokedWithWildcard1()
        {
            // arrange
            File.Copy(@"AssemblyInfo (5).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(increment).0.0" },
                { "AssemblyInformationalVersion", "$(version)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyVersion(\"1.3.*\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyInformationalVersion(\"1.3.0.0\")]", StringComparison.Ordinal));
            Assert.AreEqual("1.3.0.0", actual["MaxAssemblyVersion"].ToString());

            var versions = actual["AssemblyVersions"] as IEnumerable<Version>;
            Assert.AreEqual(1, versions.Count());
            Assert.AreEqual("1.3.0.0", versions.ElementAt(0).ToString());
        }

        /// <summary>
        /// Tests if the assembly version is updated when a '*' wildcard character is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (6).cs")]
        public void AssemblyInfo_UpdatesAssemblyVersion_WhenExecuteInvokedWithWildcard2()
        {
            // arrange
            File.Copy(@"AssemblyInfo (6).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(increment).$(increment).0" },
                { "AssemblyInformationalVersion", "$(version)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyVersion(\"1.3.1.*\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyInformationalVersion(\"1.3.1.0\")]", StringComparison.Ordinal));
            Assert.AreEqual("1.3.1.0", actual["MaxAssemblyVersion"].ToString());

            var versions = actual["AssemblyVersions"] as IEnumerable<Version>;
            Assert.AreEqual(1, versions.Count());
            Assert.AreEqual("1.3.1.0", versions.ElementAt(0).ToString());
        }

        #endregion

        #region AssemblyFileVersion Tests

        /// <summary>
        /// Tests if the assembly file version is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyFileVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyFileVersion", "$(current).$(increment).$(date:yyyy).1234" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyFileVersion(\"1.3.{0:yyyy}.1234\")]", DateTime.Today), StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the max assembly file version is computed.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (3).cs")]
        public void AssemblyInfo_ComputesMaxAssemblyFileVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + "1.cs", true);
            File.Copy(@"AssemblyInfo (3).cs", TestFilePrefix + "2.cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + "1.cs", TestFilePrefix + "2.cs" } },
                { "AssemblyFileVersion", "$(current).$(increment).$(current).$(current)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual("1.3.3.4", actual["MaxAssemblyFileVersion"].ToString());
        }

        /// <summary>
        /// Tests if the assembly file versions enumerable is filled.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (3).cs")]
        public void AssemblyInfo_FillAssemblyFileVersions_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + "1.cs", true);
            File.Copy(@"AssemblyInfo (3).cs", TestFilePrefix + "2.cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + "1.cs", TestFilePrefix + "2.cs" } },
                { "AssemblyFileVersion", "$(current).$(increment).$(current).$(current)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var versions = actual["AssemblyFileVersions"] as IEnumerable<Version>;
            Assert.AreEqual(2, versions.Count());
            Assert.AreEqual("1.3.3.4", versions.ElementAt(0).ToString());
            Assert.AreEqual("1.2.3.4", versions.ElementAt(1).ToString());
        }

        /// <summary>
        /// Tests if the assembly file versions enumerable is empty if no AssemblyVersion is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (4).cs")]
        public void AssemblyInfo_EmptyAssemblyFileVersions_WhenExecuteInvokedOnFileWithoutAssemblyFileVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (4).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyFileVersion", "$(current).$(increment).$(current).$(current)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var versions = actual["AssemblyFileVersions"] as IEnumerable<Version>;
            Assert.AreEqual(0, versions.Count());
        }

        /// <summary>
        /// Tests if the assembly file version is updated when a '*' wildcard character is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (5).cs")]
        public void AssemblyInfo_UpdatesAssemblyFileVersion_WhenExecuteInvokedWithWildcard1()
        {
            // arrange
            File.Copy(@"AssemblyInfo (5).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyFileVersion", "$(current).$(increment).0.0" },
                { "AssemblyInformationalVersion", "$(fileversion)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyFileVersion(\"2.4.*\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyInformationalVersion(\"2.4.0.0\")]", StringComparison.Ordinal));
            Assert.AreEqual("2.4.0.0", actual["MaxAssemblyFileVersion"].ToString());

            var versions = actual["AssemblyFileVersions"] as IEnumerable<Version>;
            Assert.AreEqual(1, versions.Count());
            Assert.AreEqual("2.4.0.0", versions.ElementAt(0).ToString());
        }

        /// <summary>
        /// Tests if the assembly file version is updated when a '*' wildcard character is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (6).cs")]
        public void AssemblyInfo_UpdatesAssemblyFileVersion_WhenExecuteInvokedWithWildcard2()
        {
            // arrange
            File.Copy(@"AssemblyInfo (6).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyFileVersion", "$(current).$(increment).$(increment).0" },
                { "AssemblyInformationalVersion", "$(fileversion)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyFileVersion(\"2.4.2.*\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyInformationalVersion(\"2.4.2.0\")]", StringComparison.Ordinal));
            Assert.AreEqual("2.4.2.0", actual["MaxAssemblyFileVersion"].ToString());

            var versions = actual["AssemblyFileVersions"] as IEnumerable<Version>;
            Assert.AreEqual(1, versions.Count());
            Assert.AreEqual("2.4.2.0", versions.ElementAt(0).ToString());
        }

        #endregion

        #region AssemblyInformationalVersion Tests

        /// <summary>
        /// Tests if the assembly informational version is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyInformationalVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyInformationalVersion", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyInformationalVersion(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the max assembly informational version is computed.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (3).cs")]
        public void AssemblyInfo_ComputesMaxAssemblyInformationalVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (3).cs", TestFilePrefix + "2.cs", true);
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + "1.cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + "1.cs", TestFilePrefix + "2.cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyInformationalVersion", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(string.Format("Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}", DateTime.Today), actual["MaxAssemblyInformationalVersion"].ToString());
        }

        /// <summary>
        /// Tests if the assembly informational versions enumerable is filled.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (3).cs")]
        public void AssemblyInfo_FillAssemblyInformationalVersions_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + "1.cs", true);
            File.Copy(@"AssemblyInfo (3).cs", TestFilePrefix + "2.cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + "1.cs", TestFilePrefix + "2.cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyInformationalVersion", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var versions = actual["AssemblyInformationalVersions"] as IEnumerable<string>;
            Assert.AreEqual(2, versions.Count());
            Assert.AreEqual(string.Format("Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}", DateTime.Today), versions.ElementAt(0));
            Assert.AreEqual(string.Format("Value 1.1.0.0/1.1.4.0 at {0:yyyyMMdd}", DateTime.Today), versions.ElementAt(1));
        }

        /// <summary>
        /// Tests if the assembly file versions enumerable is filled with empty string if no AssemblyInformationalVersion is present.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (4).cs")]
        public void AssemblyInfo_EmptyAssemblyInformationalVersions_WhenExecuteInvokedOnFileWithoutAssemblyInformationalVersion()
        {
            // arrange
            File.Copy(@"AssemblyInfo (4).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyInformationalVersion", "$(fileversion)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var versions = actual["AssemblyInformationalVersions"] as IEnumerable<string>;
            Assert.AreEqual(1, versions.Count());
            Assert.AreEqual(string.Empty, versions.ElementAt(0));
        }

        #endregion

        #region AssemblyCompany Tests

        /// <summary>
        /// Tests if the assembly company is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyCompany_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyCompany", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyCompany(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyConfiguration Tests

        /// <summary>
        /// Tests if the assembly configuration is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyConfiguration_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyConfiguration", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyConfiguration(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyCopyright Tests

        /// <summary>
        /// Tests if the assembly copyright is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyCopyright_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyCopyright", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyCopyright(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyDescription Tests

        /// <summary>
        /// Tests if the assembly description is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyDescription_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyDescription", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyDescription(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyProduct Tests

        /// <summary>
        /// Tests if the assembly product is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyProduct_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyProduct", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyProduct(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyTitle Tests

        /// <summary>
        /// Tests if the assembly title is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyTitle_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyTitle", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyTitle(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyTrademark Tests

        /// <summary>
        /// Tests if the assembly trademark is updated using token and fixed values.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyTrademark_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyTrademark", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyTrademark(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyCulture Tests

        /// <summary>
        /// Tests if the assembly culture is updated and tokens are not expanded.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyCulture_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyCulture", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyCulture(\"Value $(version)/$(fileversion) at $(date:yyyyMMdd)\")]", StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyDelaySign Tests

        /// <summary>
        /// Tests if the assembly delay sign is updated in a csharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyDelaySign_WhenExecuteInvokedOnCSharpFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyDelaySign", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyDelaySign(false)]", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the assembly delay sign is updated in a vb.net file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).vb")]
        public void AssemblyInfo_UpdatesAssemblyDelaySign_WhenExecuteInvokedOnVBFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).vb", TestFilePrefix + ".vb", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".vb" } },
                { "AssemblyDelaySign", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".vb");
            Assert.AreNotEqual(-1, text.IndexOf("<Assembly: AssemblyDelaySign(False)>", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the assembly delay sign is updated in a fsharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).fs")]
        public void AssemblyInfo_UpdatesAssemblyDelaySign_WhenExecuteInvokedOnFSharpFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).fs", TestFilePrefix + ".fs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".fs" } },
                { "AssemblyDelaySign", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".fs");
            Assert.AreNotEqual(-1, text.IndexOf("[<assembly: AssemblyDelaySign(false)>]", StringComparison.Ordinal));
        }

        #endregion

        #region Guid Tests

        /// <summary>
        /// Tests if the GUID is updated.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesGuid_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var expected = System.Guid.NewGuid();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "Guid", expected }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: Guid(\"" + expected.ToString() + "\")]", StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyKeyFile Tests

        /// <summary>
        /// Tests if the assembly key file is updated and tokens are not expanded.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyKeyFile_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyKeyFile", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyKeyFile(\"Value $(version)/$(fileversion) at $(date:yyyyMMdd)\")]", StringComparison.Ordinal));
        }

        #endregion

        #region AssemblyKeyName Tests

        /// <summary>
        /// Tests if the assembly key name is updated and tokens are not expanded.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesAssemblyKeyName_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyKeyName", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyKeyName(\"Value $(version)/$(fileversion) at $(date:yyyyMMdd)\")]", StringComparison.Ordinal));
        }

        #endregion

        #region CLSCompliant Tests

        /// <summary>
        /// Tests if the CLS compliance is updated in a csharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesCLSCompliant_WhenExecuteInvokedOnCSharpFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "CLSCompliant", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: CLSCompliant(false)]", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the CLS compliance is updated in a vb.net file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).vb")]
        public void AssemblyInfo_UpdatesCLSCompliant_WhenExecuteInvokedOnVBFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).vb", TestFilePrefix + ".vb", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".vb" } },
                { "CLSCompliant", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".vb");
            Assert.AreNotEqual(-1, text.IndexOf("<Assembly: CLSCompliant(False)>", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the CLS compliance is updated in a fsharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).fs")]
        public void AssemblyInfo_UpdatesCLSCompliant_WhenExecuteInvokedOnFSharpFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).fs", TestFilePrefix + ".fs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".fs" } },
                { "CLSCompliant", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".fs");
            Assert.AreNotEqual(-1, text.IndexOf("[<assembly: CLSCompliant(false)>]", StringComparison.Ordinal));
        }

        #endregion

        #region ComVisible Tests

        /// <summary>
        /// Tests if the COM visibility is updated in a csharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_UpdatesComVisible_WhenExecuteInvokedOnCSharpFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "ComVisible", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: ComVisible(false)]", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the COM visibility is updated in a vb.net file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).vb")]
        public void AssemblyInfo_UpdatesComVisible_WhenExecuteInvokedOnVBFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).vb", TestFilePrefix + ".vb", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".vb" } },
                { "ComVisible", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".vb");
            Assert.AreNotEqual(-1, text.IndexOf("<Assembly: ComVisible(False)>", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests if the COM visibility is updated in a fsharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).fs")]
        public void AssemblyInfo_UpdatesComVisible_WhenExecuteInvokedOnFSharpFile()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).fs", TestFilePrefix + ".fs", true);

            var target = new AssemblyInfo();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".fs" } },
                { "ComVisible", false }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".fs");
            Assert.AreNotEqual(-1, text.IndexOf("[<assembly: ComVisible(false)>]", StringComparison.Ordinal));
        }

        #endregion

        #region Long attribute name Tests

        /// <summary>
        /// Tests if the attributes with long names (suffixes with Attribute) are updated in a csharp file.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (7).cs")]
        public void AssemblyInfo_UpdatesAttributes_WhenExecuteInvokedOnCSharpFileWithLongAttributeNames()
        {
            // arrange
            File.Copy(@"AssemblyInfo (7).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo();
            var expectedGuid = System.Guid.NewGuid();
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
                { "AssemblyVersion", "$(current).$(current).0.0" },
                { "AssemblyFileVersion", "$(current).$(current).$(increment).0" },
                { "AssemblyInformationalVersion", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyCompany", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyConfiguration", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyCopyright", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyDescription", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyProduct", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyTitle", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyTrademark", "Value $(version)/$(fileversion) at $(date:yyyyMMdd)" },
                { "AssemblyCulture", "NewCulture" },
                { "AssemblyDelaySign", false },
                { "Guid", expectedGuid },
                { "AssemblyKeyFile", "NewKeyFile" },
                { "AssemblyKeyName", "NewKeyName" },
                { "CLSCompliant", false },
                { "ComVisible", false },
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".cs");

            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyVersionAttribute(\"1.2.0.0\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyFileVersionAttribute(\"1.2.4.0\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyInformationalVersionAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyCompanyAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyConfigurationAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyCopyrightAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyDescriptionAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyProductAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyTitleAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("[assembly: AssemblyTrademarkAttribute(\"Value 1.2.0.0/1.2.4.0 at {0:yyyyMMdd}\")]", DateTime.Today), StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyCultureAttribute(\"NewCulture\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyDelaySignAttribute(false)]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: GuidAttribute(\"" + expectedGuid.ToString() + "\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyKeyFileAttribute(\"NewKeyFile\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: AssemblyKeyNameAttribute(\"NewKeyName\")]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: CLSCompliantAttribute(false)]", StringComparison.Ordinal));
            Assert.AreNotEqual(-1, text.IndexOf("[assembly: ComVisibleAttribute(false)]", StringComparison.Ordinal));
        }

        #endregion

        #region Get action Tests

        /// <summary>
        /// Tests if the attribute values are read.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [DeploymentItem(@"Framework\TestFiles\AssemblyInfo (1).cs")]
        public void AssemblyInfo_ReadAttributes_WhenExecuteInvokedWithGetAction()
        {
            // arrange
            File.Copy(@"AssemblyInfo (1).cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo() { Action = AssemblyInfoAction.Get };
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
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
        public void AssemblyInfo_ReadAttributes_WhenExecuteInvokedWithGetActionAndEmptyFile()
        {
            // arrange
            File.Copy(@"EmptyAssemblyInfo.cs", TestFilePrefix + ".cs", true);

            var target = new AssemblyInfo() { Action = AssemblyInfoAction.Get };
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { TestFilePrefix + ".cs" } },
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
