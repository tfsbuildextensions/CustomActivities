//-----------------------------------------------------------------------
// <copyright file="SqlExecuteTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.SqlServer;

    [TestClass]
    public class DacVersionTests
    {
        // test SqlServerProj file prefix.
        private const string TestFilePrefix = @"TestDatabase";

        [TestMethod]
        [DeploymentItem(@"Framework\TestFiles\Database.sqlproj")]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SqlServerProj_UpdatesDacVersion_WhenExecuteInvoked()
        {
            // arrange
            File.Copy(@"Database.sqlproj", TestFilePrefix + ".sqlproj", true);

            var target = new DacVersion{ SqlProjFilePath = TestFilePrefix + ".sqlproj", Version = "1.0.156.3" };
            var invoker = new WorkflowInvoker(target);

            // act
            invoker.Invoke();

            // assert
            var text = File.ReadAllText(TestFilePrefix + ".sqlproj");
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("<DacVersion>1.0.156.3</DacVersion>", DateTime.Today), StringComparison.Ordinal));
        }
    }
}
