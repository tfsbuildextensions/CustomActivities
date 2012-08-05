//-----------------------------------------------------------------------
// <copyright file="SqlExecuteTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.SqlServer;

    /// <summary>
    /// This is a test class for SqlExecuteTest and is intended
    /// to contain all SqlExecuteTest Unit Tests
    /// </summary>
    [TestClass]
    public class SqlExecuteTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// A test for ExecuteScalar
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SqlExecuteScalarTest()
        {
            // Initialise Instance
            var target = new SqlExecute { Action = SqlExecuteAction.ExecuteScalar };
            var parameters = new Dictionary<string, object>
            {
                { "Sql", "SELECT CONVERT(CHAR(10), GETDATE(), 103)" },
                { "ConnectionString", "Data Source=.;Initial Catalog=;Integrated Security=True" },
                { "UseTransaction", true },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke(parameters);

            // Test the result
            Assert.AreEqual(DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-gb")), actual["ScalarResult"].ToString());
        }

        /// <summary>
        /// A test for Execute using files
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SqlExecuteFilesTest()
        {
            // Create a temp file and write some dummy attribute to it
            FileInfo f = new FileInfo(System.IO.Path.GetTempFileName());
            File.WriteAllLines(f.FullName, new[] { "SELECT CONVERT(CHAR(10), GETDATE(), 103)" });

            // Initialise Instance
            var target = new SqlExecute { Action = SqlExecuteAction.Execute };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
                { "ConnectionString", "Data Source=.;Initial Catalog=;Integrated Security=True" },
                { "UseTransaction", true },
                { "CommandTimeout", 30 },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);
        }

        /// <summary>
        /// A test for Execute using files wiht exception
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        [ExpectedException(typeof(ApplicationException))]
        public void SqlExecuteFilesExceptionTest()
        {
            // Create a temp file and write some content to it which will error
            FileInfo f = new FileInfo(System.IO.Path.GetTempFileName());
            File.WriteAllLines(f.FullName, new[] { "SELECT CONVERT(CHdAR(10), GETDATE(), 103)" });

            // Initialise Instance
            var target = new SqlExecute { Action = SqlExecuteAction.Execute };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
                { "ConnectionString", "Data Source=.;Initial Catalog=;Integrated Security=True" },
                { "UseTransaction", true },
                { "CommandTimeout", 30 },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);
        }
    }
}