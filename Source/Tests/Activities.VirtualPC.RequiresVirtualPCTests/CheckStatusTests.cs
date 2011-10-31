//-----------------------------------------------------------------------
// <copyright file="CheckStatusTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization.RequiresVirtualPCTests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests tgat check the status of the vm
    /// </summary>
    [TestClass]
    public class CheckStatusTests
    {
        /// <summary>
        /// The name of the VM under test
        /// </summary>
        private readonly string VMName = "ScratchXP";

        private TestContext testContextInstance;

        public CheckStatusTests()
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
        public void Can_check_if_vm_is_running()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.IsHeartBeating };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
            Assert.AreEqual(true, results["Result"]);
        }

        [TestMethod]
        public void Can_check_if_vm_is_screenlocked()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.IsScreenLocked };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
            Assert.AreEqual(true, results["Result"], "Is not locked");
        }

        [TestMethod]
        public void Can_list_the_vms_on_host()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.List };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(1, results["VirtualMachineCount"]);
            Assert.AreEqual(this.VMName, ((string[])results["VirtualMachines"])[0]);
        }

        [TestMethod]
        public void Wait_until_a_running_VM_is_under_a_given_load()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.WaitForLowCpuUtilization };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName },
                 { "MaxCpuUsage", 50 },
                 { "MaxCpuThreshold", 7 }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }
    }
}
