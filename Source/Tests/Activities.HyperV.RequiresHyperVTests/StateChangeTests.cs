//-----------------------------------------------------------------------
// <copyright file="StateChangeTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Virtualization.RequiresHyperVTests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Gneral operations Hyper V tests
    /// </summary>
    [TestClass]
    public class StateChangeTests
    {
        /// <summary>
        /// The name of the HyperV this.Server under test, can use '.' for localhost
        /// </summary>
        private readonly string Server = "vengeance";

        /// <summary>
        /// The name of the VM under test
        /// </summary>
        private readonly string VMName = "w2k8Server";

        private TestContext testContextInstance;

        public StateChangeTests()
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
        public void Can_start_a_stopped_or_paused_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Start };

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
        public void Can_shutdown_a_running_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Shutdown };
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
        public void Can_immediately_halt_a_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Turnoff };
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
        public void Can_pause_a_running_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Pause };

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
        public void Can_save_and_stop_a_running_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Suspend };

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
        public void Can_start_a_stopped_VM_or_restart_a_running_VM()
        {
            // arrange
            var target = new HyperV { Action = HyperVAction.Restart };

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
    }
}
