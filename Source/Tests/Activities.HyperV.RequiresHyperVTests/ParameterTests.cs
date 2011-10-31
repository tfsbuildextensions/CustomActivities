//-----------------------------------------------------------------------
// <copyright file="ParameterTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Virtualization.RequiresHyperVTests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Virtualization;

    /// <summary>
    /// Test to check parameter logic
    /// </summary>
    [TestClass]
    public class ParameterTests
    {
        private TestContext testContextInstance;

        public ParameterTests()
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
        [ExpectedException(typeof(ArgumentException))]
        public void Exeption_no_parameters_specified()
        {
            // arrange
            var target = new HyperV();
            Dictionary<string, object> args = new Dictionary<string, object>
            {
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // attribute checks value
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Exception_thrown_if_no_VM_specified()
        {
            // arrange
            var target = new HyperV();
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "ServerName",  "Server" }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // attribute checks value
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Exception_thrown_if_empty_VM_specified()
        {
            // arrange
            var target = new HyperV();
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "ServerName",  "Server" },
                 { "VMName",  string.Empty }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // attribute checks value
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Exception_thrown_if_no_server_specified()
        {
            // arrange
            var target = new HyperV();
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  "VM" }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // attribute checks value
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Exception_thrown_if_empty_servername_specified()
        {
            // arrange
            var target = new HyperV();
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "ServerName",  string.Empty },
                 { "VMName", "VMName" }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // attribute checks value
        }
    }
}
