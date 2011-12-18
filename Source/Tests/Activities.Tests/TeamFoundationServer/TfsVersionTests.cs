//-----------------------------------------------------------------------
// <copyright file="TfsVersionTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.TeamFoundationServer;

    /// <summary>
    /// This is a test class for TfsVersionTest and is intended
    /// to contain all TfsVersionTest Unit Tests
    /// </summary>
    [TestClass]
    public class TfsVersionTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }
       
        /// <summary>
        /// A test for GetVersion which makes use of Elapsed time formatting
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void GetVersionTestElapsed()
        {
            // Initialise Instance
            var target = new TfsVersion { Action = TfsVersionAction.GetVersion, VersionTemplateFormat = "0.0.1000.0", StartDate = Convert.ToDateTime("1 Mar 2009"), VersionFormat = TfsVersionVersionFormat.Elapsed, UseUtcDate = true };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Major", "3" },
                { "Minor", "1" },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            IBuildDetail t = new MockIBuildDetail { BuildNumber = "MyBuild_" + DateTime.Now.ToString("yyyyMMdd") + ".2" };
            t.BuildDefinition.Name = "MyBuild";
            invoker.Extensions.Add(t);

            var actual = invoker.Invoke(parameters);

            // Test the result
            DateTime d = Convert.ToDateTime("1 Mar 2009");
            TimeSpan ts = DateTime.Now - d;
            string days = ts.Days.ToString();
            Assert.AreEqual("3.1.1" + days + ".2", actual["Version"].ToString());
        }

        /// <summary>
        /// A test for GetVersion which makes use of DateTime formatting
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void GetVersionTestDateTime()
        {
            // Initialise Instance
            var target = new TfsVersion { Action = TfsVersionAction.GetVersion, PaddingCount = 5, PaddingDigit = '1', DateFormat = "MMdd", Delimiter = "." };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
                                 {
                                     { "Major", "1" },
                                     { "Minor", "5" },
                                 };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            IBuildDetail t = new MockIBuildDetail { BuildNumber = string.Format("MyBuild_{0}{1}{2}.2", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) };
            t.BuildDefinition.Name = "MyBuild";
            invoker.Extensions.Add(t);
            
            var actual = invoker.Invoke(parameters);

            // Test the result
            Assert.AreEqual(string.Format("1.5.1{0}{1}.2", DateTime.Now.Month, DateTime.Now.Day), actual["Version"].ToString());
        }

        /// <summary>
        /// A test for GetVersion which makes use of DateTime formatting and CombineBuildAndRevision
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void GetVersionTestCombineBuildAndRevisionDateTime()
        {
            // Initialise Instance
            var target = new TfsVersion { CombineBuildAndRevision = true, Action = TfsVersionAction.GetVersion, PaddingCount = 5, PaddingDigit = '1', DateFormat = "MMdd", Delimiter = "." };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Major", "1" },
                { "Minor", "5" },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            IBuildDetail t = new MockIBuildDetail { BuildNumber = "MyBuild_20101101.2" };
            t.BuildDefinition.Name = "MyBuild";
            invoker.Extensions.Add(t);

            var actual = invoker.Invoke(parameters);

            // Test the result
            Assert.AreEqual("1.5.11101.111012", actual["Version"].ToString());
        }

        /// <summary>
        /// A test for GetVersion which makes use of Elapsed time formatting and CombineBuildAndRevision
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void GetVersionTestCombineBuildAndRevisionElapsed()
        {
            // Initialise Instance
            var target = new TfsVersion { CombineBuildAndRevision = true, Action = TfsVersionAction.GetVersion, VersionTemplateFormat = "0.0.1000.0", StartDate = Convert.ToDateTime("1 Mar 2009"), Delimiter = ".", VersionFormat = TfsVersionVersionFormat.Elapsed, UseUtcDate = true };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Major", "3" },
                { "Minor", "1" },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            IBuildDetail t = new MockIBuildDetail { BuildNumber = "MyBuild_" + DateTime.Now.ToString("yyyyMMdd") + ".2" };
            t.BuildDefinition.Name = "MyBuild";
            invoker.Extensions.Add(t);

            var actual = invoker.Invoke(parameters);

            // Test the result
            DateTime d = Convert.ToDateTime("1 Mar 2009");
            TimeSpan ts = DateTime.Now - d;
            string days = ts.Days.ToString();
            Assert.AreEqual("3.1.1" + days + "." + days + "2", actual["Version"].ToString());
        }

        /// <summary>
        /// A test for SetVersion
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SetVersionTest()
        {
            // Initialise Instance
            var target = new TfsVersion { Action = TfsVersionAction.SetVersion, TextEncoding = "UTF8" };
            
            // Create a temp file and write some dummy attribute to it
            FileInfo f = new FileInfo(System.IO.Path.GetTempFileName());
            File.WriteAllLines(f.FullName, new[] { "[assembly:AssemblyFileVersion(\"1.0.0.0\")]" });

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Version", "1.0.0.1" },
                { "Files", new[] { f.FullName } },
            };

            // Invoke the Workflow
            WorkflowInvoker.Invoke(target, parameters);

            // read the updated file back.
            using (System.IO.StreamReader file = new System.IO.StreamReader(f.FullName))
            {
                // Test the result
                Assert.AreEqual("[assembly:AssemblyFileVersion(\"1.0.0.1\")]", file.ReadLine());
            }
        }

        /// <summary>
        /// A test for SetVersion
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SetVersionTestIncludingAssemblyVersion()
        {
            // Initialise Instance
            var target = new TfsVersion { Action = TfsVersionAction.SetVersion, TextEncoding = "UTF8", SetAssemblyVersion = true, ForceSetVersion = true };
            
            // Create a temp file and write some dummy attribute to it
            FileInfo f = new FileInfo(System.IO.Path.GetTempFileName());
            File.WriteAllLines(f.FullName, new[] { "[assembly:AssemblyFileVersion(\"1.0.0.0\")]", "[assembly:AssemblyVersion(\"1.0.0.0\")]" });

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Version", "1.0.0.1" },
                { "Files", new[] { f.FullName } },
            };

            // Invoke the Workflow
            WorkflowInvoker.Invoke(target, parameters);

            // read the updated file back.
            using (System.IO.StreamReader file = new System.IO.StreamReader(f.FullName))
            {
                // Test the result
                Assert.AreEqual("[assembly:AssemblyFileVersion(\"1.0.0.1\")]", file.ReadLine());
                Assert.AreEqual("[assembly:AssemblyVersion(\"1.0.0.1\")]", file.ReadLine());
            }
        }
    }
}
