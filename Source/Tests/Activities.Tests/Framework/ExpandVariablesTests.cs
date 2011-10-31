//-----------------------------------------------------------------------
// <copyright file="ExpandVariablesTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TfsBuildExtensions.Activities.Framework;

    /// <summary>
    /// This is a test class for AssemblyInfo and is intended
    /// to contain all AssemblyInfo Unit Tests
    /// </summary>
    [TestClass]
    public class ExpandVariablesTests
    {
        #region Properties

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Empty Parameters Tests

        [TestMethod]
        public void ExpandVariables_ReturnsNullWhenExecuteInvoked_WithNullInput()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", null }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.IsNull(actual["Result"]);
        }

        [TestMethod]
        public void ExpandVariables_ReturnsEmptyWhenExecuteInvoked_WithEmptyInput()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", string.Empty }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(string.Empty, (string)actual["Result"]);
        }

        #endregion

        #region User Variables Tests

        [TestMethod]
        public void ExpandVariables_ExpandVariablesWhenExecuteInvoked_WithVariableNameOnly()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My user var: $(Value1)/$(Value2)" },
                { "Variables", new Dictionary<string, string> { { "Value1", "value1" },  { "Value2", "value2" } } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My user var: value1/value2", actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_DoNotExpandVariablesWhenExecuteInvoked_WithVariableToken()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My user var: $(Value1)" },
                { "Variables", new Dictionary<string, string> { { "$(Value1)", "value1" },  { "Value2", "value2" } } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My user var: $(Value1)", actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_DoNotExpandUnknownVariablesWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My user var: $(Value1)" },
                { "Variables", new Dictionary<string, string> { { "Value2", "value2" } } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My user var: $(Value1)", actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandUserVariablesWhenExecuteInvoked_WithUserVariableOverridingBuildVariable()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My user var: $(BuildId)" },
                { "Variables", new Dictionary<string, string> { { "BuildId", "-1" } } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My user var: -1", actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandUserVariablesWhenExecuteInvoked_WithUserVariableOverridingEnvironmentVariable()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My user var: $(TMP)" },
                { "Variables", new Dictionary<string, string> { { "TMP", "[TEMP]" } } },
                { "IncludeEnvironmentVariables", true }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My user var: [TEMP]", actual["Result"].ToString());
        }

        #endregion

        #region Environment Variables Tests

        [TestMethod]
        public void ExpandVariables_DoNotExpandEnvironmentVariablesWhenExecuteInvoked_WithIncludeEnvironmentVariablesEqualsFalse()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My temp dir: $(TMP)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My temp dir: $(TMP)", actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandEnvironmentVariablesWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My temp dir: $(TMP)" },
                { "IncludeEnvironmentVariables", true }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My temp dir: " + Environment.GetEnvironmentVariable("TMP"), actual["Result"].ToString());
        }

        #endregion

        #region Build Variables Tests

        [TestMethod]
        public void ExpandVariables_DoNotExpandBuildVariablesWhenExecuteInvoked_WithIncludeBuildVariablesEqualsFalse()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "My build number: $(BuildNumber)" }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual("My build number: $(BuildNumber)", actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildNumberVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(BuildNumber)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildNumber, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildIdVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(BuildId)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(LinkingUtilities.DecodeUri(buildDetail.Object.Uri.ToString()).ToolSpecificId, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildDefinitionNameVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(BuildDefinitionName)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildDefinition.Name, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildDefinitionIdVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(BuildDefinitionId)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildDefinition.Id, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildTeamProjectVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(TeamProject)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildDefinition.TeamProject, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildDropLocationVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(DropLocation)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.DropLocation, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_DoNotExpandBuildAgentWhenExecuteInvokedOutsideAgentScope()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(BuildAgent)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(string.Empty, actual["Result"].ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildAgentWhenExecuteInvokedInsideAgentScope()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Input", "$(BuildAgent)" },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var buildAgent = GetMockedIBuildAgent();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);
            invoker.Extensions.Add(buildAgent.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, actual.Count, "Expected only one output parameter.");
            Assert.AreEqual(buildAgent.Object.Name, actual["Result"].ToString());
        }

        #endregion

        #region Private Helpers

        // Returns a mocked IBuildDetail
        private static Mock<IBuildDetail> GetMockedIBuildDetail()
        {
            var buildDef = new Mock<IBuildDefinition>();
            buildDef.SetupAllProperties();

            buildDef.Object.Name = "MyBuildDefinition";
            buildDef.SetupGet(bd => bd.Id).Returns("MyBuildDefinitionId");
            buildDef.SetupGet(bd => bd.TeamProject).Returns("MyTeamProject");

            var buildDetail = new Mock<IBuildDetail>();
            buildDetail.SetupAllProperties();

            buildDetail.Object.BuildNumber = "MyBuildNumber";
            buildDetail.Object.DropLocation = "MyDropLocation";
            buildDetail.SetupGet(bd => bd.Uri).Returns(new Uri("vstfs:///tool/artifact/4"));

            buildDetail.SetupGet(bd => bd.BuildDefinition).Returns(buildDef.Object);

            return buildDetail;
        }

        // Returns a mocked IBuildAgent
        private static Mock<IBuildAgent> GetMockedIBuildAgent()
        {
            var buildAgent = new Mock<IBuildAgent>();
            buildAgent.SetupAllProperties();

            buildAgent.Object.Name = "MyBuildAgent";

            return buildAgent;
        }

        #endregion
    }
}
