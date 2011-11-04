//-----------------------------------------------------------------------
// <copyright file="ExpandVariablesTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
                { "Inputs", null }
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
                { "Inputs", new string[0] }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.IsFalse(((IEnumerable<string>)actual["Result"]).Any());
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
                { "Inputs", new [] { "My user var: $(Value1)/$(Value2)" } },
                { "Variables", new Dictionary<string, string> { { "Value1", "value1" },  { "Value2", "value2" } } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My user var: value1/value2", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_DoNotExpandVariablesWhenExecuteInvoked_WithVariableToken()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "My user var: $(Value1)" } },
                { "Variables", new Dictionary<string, string> { { "$(Value1)", "value1" },  { "Value2", "value2" } } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My user var: $(Value1)", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_DoNotExpandUnknownVariablesWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "My user var: $(Value1)" } },
                { "Variables", new Dictionary<string, string> { { "Value2", "value2" } } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My user var: $(Value1)", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandUserVariablesWhenExecuteInvoked_WithUserVariableOverridingBuildVariable()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "My user var: $(BuildId)" } },
                { "Variables", new Dictionary<string, string> { { "BuildId", "-1" } } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My user var: -1", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandUserVariablesWhenExecuteInvoked_WithUserVariableOverridingEnvironmentVariable()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "My user var: $(TMP)" } },
                { "Variables", new Dictionary<string, string> { { "TMP", "[TEMP]" } } },
                { "IncludeEnvironmentVariables", true }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My user var: [TEMP]", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
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
                { "Inputs", new [] { "My temp dir: $(TMP)" } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My temp dir: $(TMP)", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandEnvironmentVariablesWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "My temp dir: $(TMP)" } },
                { "IncludeEnvironmentVariables", true }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My temp dir: " + Environment.GetEnvironmentVariable("TMP"), ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
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
                { "Inputs", new [] { "My build number: $(BuildNumber)" } }
            };

            var invoker = new WorkflowInvoker(target);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual("My build number: $(BuildNumber)", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildNumberVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(BuildNumber)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildNumber, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildIdVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(BuildId)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(LinkingUtilities.DecodeUri(buildDetail.Object.Uri.ToString()).ToolSpecificId, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildDefinitionNameVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(BuildDefinitionName)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildDefinition.Name, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildDefinitionIdVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(BuildDefinitionId)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildDefinition.Id, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildTeamProjectVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(TeamProject)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.BuildDefinition.TeamProject, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildDropLocationVariableWhenExecuteInvoked()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(DropLocation)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(buildDetail.Object.DropLocation, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_DoNotExpandBuildAgentWhenExecuteInvokedOutsideAgentScope()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(BuildAgent)" } },
                { "IncludeBuildVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();
            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(string.Empty, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        [TestMethod]
        public void ExpandVariables_ExpandBuildAgentWhenExecuteInvokedInsideAgentScope()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(BuildAgent)" } },
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
            Assert.AreEqual(1, ((IEnumerable<string>)actual["Result"]).Count(), "Expected only one output parameter.");
            Assert.AreEqual(buildAgent.Object.Name, ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
        }

        #endregion

        #region Multi Inputs Tests

        [TestMethod]
        public void ExpandVariables_ExpandVariablesWhenExecuteInvoked_WithMultipleInputs()
        {
            // arrange
            var target = new ExpandVariables();
            var parameters = new Dictionary<string, object>
            {
                { "Inputs", new [] { "$(Value1)", "$(BuildNumber)", "$(TMP)" } },
                { "Variables", new Dictionary<string, string> { { "Value1", "value1" } } },
                { "IncludeBuildVariables", true },
                { "IncludeEnvironmentVariables", true }
            };

            var buildDetail = GetMockedIBuildDetail();

            var invoker = new WorkflowInvoker(target);
            invoker.Extensions.Add(buildDetail.Object);

            // act
            var actual = invoker.Invoke(parameters);

            // assert
            Assert.AreEqual(3, ((IEnumerable<string>)actual["Result"]).Count(), "Expected 3 output parameter.");
            Assert.AreEqual("value1", ((IEnumerable<string>)actual["Result"]).ElementAt(0).ToString());
            Assert.AreEqual(buildDetail.Object.BuildNumber, ((IEnumerable<string>)actual["Result"]).ElementAt(1).ToString());
            Assert.AreEqual(Environment.GetEnvironmentVariable("TMP"), ((IEnumerable<string>)actual["Result"]).ElementAt(2).ToString());
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
