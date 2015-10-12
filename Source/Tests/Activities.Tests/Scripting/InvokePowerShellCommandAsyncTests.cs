﻿namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Scripting;
    using TfsBuildExtensions.Activities.Scripting.PowerShell;

    [TestClass]
    public class InvokePowerShellCommandAsyncTests
    {
        [TestMethod]
        public void PowershellActivity_ReturnsMembers_WhenGetMemberIsInvoked()
        {
            var activity = new InvokePowerShellCommandAsync { Script = "Get-Help Get-Item" };
            var outputs = WorkflowInvoker.Invoke(activity);
            Assert.IsNotNull(outputs);
        }

        [TestMethod]
        [ExpectedException(typeof(PowerShellExecutionException), AllowDerivedTypes = true)]
        public void PowershellActivity_ThrowsCommandNotFoundException_OnUnhandledRuntimeException()
        {
            var activity = new InvokePowerShellCommandAsync { Script = "Get-Helps Get-Item" };
            WorkflowInvoker.Invoke(activity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PowershellActivity_ThrowsArgumentNullException_OnEmptyCommand()
        {
            var activity = new InvokePowerShellCommand { Script = string.Empty };
            WorkflowInvoker.Invoke(activity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PowershellActivity_ThrowsArgumentNullException_OnWhitespaceCommand()
        {
            var activity = new InvokePowerShellCommandAsync { Script = "   " };
            WorkflowInvoker.Invoke(activity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PowershellActivity_ThrowsArgumentNullException_WhenServerCommandAndNoWorkspaceProvided()
        {
            // Arrange
            var activity = new InvokePowerShellCommandAsync { Script = "$/Test Path/Not A Real Path" };

            // Act
            WorkflowInvoker.Invoke(activity);
        }
    }
}
