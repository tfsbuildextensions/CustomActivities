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
    using TfsBuildExtensions.Activities.FileSystem;

    /// <summary>
    /// This is a test class for SqlExecuteTest and is intended
    /// to contain all SqlExecuteTest Unit Tests
    /// </summary>
    [TestClass]
    public class FileTests
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
        public void FileReplaceTest()
        {
            // Initialise Instance
            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Replace, RegexPattern = "Michael", Replacement = "Mike" };

            // Create a temp file and write some dummy attribute to it
            FileInfo f = new FileInfo(System.IO.Path.GetTempFileName());
            System.IO.File.WriteAllLines(f.FullName, new[] { "Michael" });

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke(parameters);

            // read the updated file back.
            using (System.IO.StreamReader file = new System.IO.StreamReader(f.FullName))
            {
                // Test the result
                Assert.AreEqual("Mike", file.ReadLine());
            }
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileTouch_UpdateReadOnlyFile_WhenExecuteInvokedWithForce()
        {
            // arrange
            var originalTime = DateTime.Now.AddDays(-1);
            var f = new FileInfo(System.IO.Path.GetTempFileName()) { LastAccessTime = originalTime, LastWriteTime = originalTime, Attributes = FileAttributes.ReadOnly };

            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Touch, Force = true };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            f = new FileInfo(f.FullName);

            Assert.AreEqual(DateTime.Today, f.LastWriteTime.Date);
            Assert.AreEqual(DateTime.Today, f.LastAccessTime.Date);
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileTouch_SkipReadOnlyFile_WhenExecuteInvokedWithoutForce()
        {
            // arrange
            var originalTime = DateTime.Now.AddDays(-1);
            var f = new FileInfo(System.IO.Path.GetTempFileName()) { LastAccessTime = originalTime, LastWriteTime = originalTime, Attributes = FileAttributes.ReadOnly };

            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Touch };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            f = new FileInfo(f.FullName);

            Assert.AreEqual(originalTime, f.LastWriteTime);
            Assert.AreEqual(originalTime, f.LastAccessTime);
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileTouch_UpdateFile_WhenExecuteInvokedWithTime()
        {
            // arrange
            var originalTime = DateTime.Now;
            var f = new FileInfo(System.IO.Path.GetTempFileName()) { LastAccessTime = originalTime, LastWriteTime = originalTime };

            var expectedTime = DateTime.Now.AddDays(-2);
            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Touch, Time = expectedTime };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            f = new FileInfo(f.FullName);

            Assert.AreEqual(expectedTime, f.LastWriteTime);
            Assert.AreEqual(expectedTime, f.LastAccessTime);
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileTouch_UpdateFile_WhenExecuteInvokedWithoutTime()
        {
            // arrange
            var originalTime = DateTime.Now.AddDays(-1);
            var f = new FileInfo(System.IO.Path.GetTempFileName()) { LastAccessTime = originalTime, LastWriteTime = originalTime };

            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Touch };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            f = new FileInfo(f.FullName);

            Assert.AreEqual(DateTime.Today, f.LastWriteTime.Date);
            Assert.AreEqual(DateTime.Today, f.LastAccessTime.Date);
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileTouch_UpdateFiles_WhenExecuteInvoked()
        {
            // arrange
            var originalTime = DateTime.Now;
            var f1 = new FileInfo(System.IO.Path.GetTempFileName()) { LastAccessTime = originalTime, LastWriteTime = originalTime };
            var f2 = new FileInfo(System.IO.Path.GetTempFileName()) { LastAccessTime = originalTime, LastWriteTime = originalTime };

            var expectedTime = DateTime.Now.AddDays(-2);
            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Touch, Time = expectedTime };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f1.FullName, f2.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            f1 = new FileInfo(f1.FullName);
            f2 = new FileInfo(f2.FullName);

            Assert.AreEqual(expectedTime, f1.LastWriteTime);
            Assert.AreEqual(expectedTime, f1.LastAccessTime);

            Assert.AreEqual(expectedTime, f2.LastWriteTime);
            Assert.AreEqual(expectedTime, f2.LastAccessTime);
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileDelete_DeleteReadOnlyFile_WhenExecuteInvokedWithForce()
        {
            // arrange
            var f = new FileInfo(System.IO.Path.GetTempFileName()) { Attributes = FileAttributes.ReadOnly };

            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Delete, Force = true };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.IsFalse(System.IO.File.Exists(f.FullName));
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileDelete_SkipReadOnlyFile_WhenExecuteInvokedWithoutForce()
        {
            // arrange
            var f = new FileInfo(System.IO.Path.GetTempFileName()) { Attributes = FileAttributes.ReadOnly };

            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Delete };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.IsTrue(System.IO.File.Exists(f.FullName));
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void FileDelete_DeleteFiles_WhenExecuteInvoked()
        {
            // arrange
            var f1 = new FileInfo(System.IO.Path.GetTempFileName()) { Attributes = FileAttributes.ReadOnly };
            var f2 = new FileInfo(System.IO.Path.GetTempFileName());

            var target = new TfsBuildExtensions.Activities.FileSystem.File { Action = FileAction.Delete };

            var invoker = new WorkflowInvoker(target);

            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { f1.FullName, f2.FullName } },
            };

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.IsTrue(System.IO.File.Exists(f1.FullName));
            Assert.IsFalse(System.IO.File.Exists(f2.FullName));
        }
    }
}
