using System;
using System.Activities;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsBuildExtensions.Activities.Scripting;
using TfsBuildExtensions.Activities.Scripting.PowerShell;

namespace TfsBuildExtensions.Activities.Tests.Scripting
{
    [TestClass]
    public class InvokePowerShellCommandAsyncTests
    {
        [TestMethod]
        public void PowershellActivity_ReturnsMembers_WhenGetMemberIsInvoked()
        {
            var activity = new InvokePowershellCommandAsync { Script = "Get-Help Get-Item" };
            var outputs = WorkflowInvoker.Invoke(activity);
            Assert.IsNotNull(outputs);
        }

        [TestMethod]
        [ExpectedException(typeof(PowerShellExecutionException), AllowDerivedTypes = true)]
        public void PowershellActivity_ThrowsCommandNotFoundException_OnUnhandledRuntimeException()
        {
            var activity = new InvokePowershellCommandAsync { Script = "Get-Helps Get-Item" };
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
            var activity = new InvokePowershellCommandAsync { Script = "   " };
            WorkflowInvoker.Invoke(activity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PowershellActivity_ThrowsArgumentNullException_WhenServerCommandAndNoWorkspaceProvided()
        {
            // Arrange
            var activity = new InvokePowershellCommandAsync { Script = "$/Test Path/Not A Real Path" };

            // Act
            WorkflowInvoker.Invoke(activity);
        }
    }
}
