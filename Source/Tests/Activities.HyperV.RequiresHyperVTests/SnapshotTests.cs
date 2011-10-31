//-----------------------------------------------------------------------
// <copyright file="SnapshotTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Virtualization.RequiresHyperVTests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test for snapshotting a VM
    /// </summary>
    [TestClass]
    public class SnapshotTests
    {
        /// <summary>
        /// The name of the HyperV this.Server under test, can use '.' for localhost
        /// </summary>
        private readonly string Server = "triumph";

        /// <summary>
        /// The name of the VM under test
        /// </summary>
        private readonly string VMName = "testvm";

        private TestContext testContextInstance;

        public SnapshotTests()
        {
        }

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

        #region Additional test attributes
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        #endregion

        [TestMethod]
        public void Can_take_snapshot_of_a_running_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Snapshot };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "ServerName", this.Server },
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Succeeded"]);
        }

        [TestMethod]
        public void Can_restore_the_last_snapshot_to_a_stopped_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.ApplyLastSnapshot };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "ServerName", this.Server },
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Succeeded"]);
        }

         [TestMethod]
        public void Cannot_restore_the_last_snapshot_to_a_running_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.ApplyLastSnapshot };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "ServerName", this.Server },
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // actually you see the error 32775
            Assert.AreEqual(false, results["Succeeded"]);
        }

         [TestMethod]
         public void Can_restore_a_named_snapshot_to_a_stopped_VM()
         {
             // arrange
             var target = new HyperV { Action = HyperVAction.ApplyNamedSnapshot };

             Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "ServerName", this.Server },
                 { "VMName",  this.VMName },
                 { "SnapshotName", "Ready to test" }
            };
             WorkflowInvoker invoker = new WorkflowInvoker(target);

             // act
             var results = invoker.Invoke(args);

             // assert
             // actually you see the error 32775
             Assert.AreEqual(true, results["Succeeded"]);
         }

         [TestMethod]
         public void Can_restore_a_different_named_snapshot_to_a_stopped_VM()
         {
             // arrange
             var target = new HyperV { Action = HyperVAction.ApplyNamedSnapshot };

             Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "ServerName", this.Server },
                 { "VMName",  this.VMName },
                 { "SnapshotName", "TestVM" }
            };
             WorkflowInvoker invoker = new WorkflowInvoker(target);

             // act
             var results = invoker.Invoke(args);

             // assert
             // actually you see the error 32775
             Assert.AreEqual(true, results["Succeeded"]);
         }
    }
}