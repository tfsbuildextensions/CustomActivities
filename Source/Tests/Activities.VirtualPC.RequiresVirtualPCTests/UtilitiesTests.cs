//-----------------------------------------------------------------------
// <copyright file="UtilitiesTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Virtualization.RequiresVirtualPCTests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TfsBuildExtensions.Activities.Virtualization.Extended;

    /// <summary>
    /// Test tools to alter a vm settings
    /// </summary>
    [TestClass]
    public class UtilitiesTests
    {
        /// <summary>
        /// The name of the VM under test
        /// </summary>
        private readonly string VMName = "ScratchXP";

        private TestContext testContextInstance;

        public UtilitiesTests()
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
        public void Can_take_a_screen_shot()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.TakeScreenshot };
            var imageFile = "image.bmp";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName },
                 { "FileName",  imageFile }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
            Assert.IsTrue(File.Exists(imageFile));
        }

        [TestMethod]
        public void Can_merge_a_change_disk_on_a_VM_and_will_wait_for_completion()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.MergeUndoDisks, WaitForCompletion = -1 };

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
        public void Can_click_the_mouse_on_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.RunScript };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName },
                 { "Script", new ScriptItem[] { new ScriptItem(ScriptAction.ClickLeft, string.Empty) } }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_run_notepad_via_a_script_a_running_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.RunScript };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName },
                 { "Script", new ScriptItem[] 
                    { 
                        new ScriptItem(ScriptAction.TypeKeySequence, "Key_LeftWindows"),
                      new ScriptItem(ScriptAction.TypeKeySequence, "Key_Up"),
                      new ScriptItem(ScriptAction.TypeKeySequence, "Key_Up"),
                      new ScriptItem(ScriptAction.TypeKeySequence, "Key_Up"),
                      new ScriptItem(ScriptAction.TypeKeySequence, "Key_Enter"),
                      new ScriptItem(ScriptAction.TypeAsciiText, "notepad"),
                      new ScriptItem(ScriptAction.TypeKeySequence, "Key_Enter"),
                      new ScriptItem(ScriptAction.TypeAsciiText, "Here is some text")
                    } 
                  }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_add_a_disk_from_a_stopped_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.AddHardDiskConnection };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName },
                 { "FileName",  @"C:\VPC Images\ScratchXP\ExtraDrive.vhd" },
                 { "DeviceNumber", 1 },
                 { "BusNumber", 1 }
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_remove_a_disk_from_a_stopped_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.RemoveHardDiskConnection };

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "VMName",  this.VMName },
                 { "FileName",  @"C:\VPC Images\ScratchXP\ExtraDrive.vhd" },
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Success"]);
        }

        [TestMethod]
        public void Can_discard_saved_state_from_a_stopped_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.DiscardSavedState };

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
        public void Can_discard_undo_disk_from_a_stopped_VM()
        {
            // arrange
            var target = new VirtualPC { Action = VirtualPCAction.DiscardUndoDisks };

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
