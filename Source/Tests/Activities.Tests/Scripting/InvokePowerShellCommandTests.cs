//-----------------------------------------------------------------------
// <copyright file="InvokePowershellCommandTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Scripting;

    [TestClass]
    public class InvokePowerShellCommandTests
    {
        [TestMethod]
        public void PowershellActivity_ReturnsMembers_WhenGetMemberIsInvoked()
        {
            var activity = new InvokePowerShellCommand { Script = "Get-Help Get-Item" };
            var outputs = WorkflowInvoker.Invoke(activity);
            Assert.IsNotNull(outputs);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotFoundException), AllowDerivedTypes = true)]
        public void PowershellActivity_ThrowsCommandNotFoundException_OnUnhandledRuntimeException()
        {
            var activity = new InvokePowerShellCommand { Script = "Get-Helps Get-Item" };
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
            var activity = new InvokePowerShellCommand { Script = "   " };
            WorkflowInvoker.Invoke(activity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PowershellActivity_ThrowsArgumentNullException_WhenServerCommandAndNoWorkspaceProvided()
        {
            // Arrange
            var activity = new InvokePowerShellCommand { Script = "$/Test Path/Not A Real Path" };
            
            // Act
            WorkflowInvoker.Invoke(activity);
        }
    }
}
