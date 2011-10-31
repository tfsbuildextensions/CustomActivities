//-----------------------------------------------------------------------
// <copyright file="StateChangeTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization.RequiresVirtualPCTests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests that chnage running state of the VM
    /// </summary>
    [TestClass]
    public class StateChangeTests
    {
        /// <summary>
        /// The name of the VM under test
        /// </summary>
        private readonly string VMName = "ScratchXP";

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
        public void Can_start_a_stopped_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Startup };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_restart_a_VM_that_is_not_off()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Restart };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_shutdown_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Shutdown };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_save_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Save };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_pause_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Pause };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_resume_a_paused_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Resume };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }
        
        [TestMethod]
        public void Can_logoff_from_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.LogOff };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_turnoff_from_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Turnoff };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_reset_from_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.Restart };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }
    }
}
