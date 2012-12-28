//-----------------------------------------------------------------------
// <copyright file="EmailTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.CodeQuality.Test
{
    using System;
    using System.Collections.Generic;
    using System.Activities;
    using Activities.CodeQuality;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Unit test class for VSTest
    /// </summary>
    [TestClass]
    public class VSTestTests
    {
        [TestMethod]
        public void TestThatCorrectExecutableIsCalled()
        {
            string vsTestConsoleExePath = @"%VS110COMNTOOLS%\..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe";
            var process = new Mock<IProcess>();
            var target = new VSTest(process.Object)
            {
                PublishTestResults = false
            };

            var parameters = new Dictionary<string, object>
            {
                { "TestAssemblies", new[] { @"C:\Projects\Test\TestAssembly.dll" } }
            };

            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            process.Verify(s => s.Execute(It.Is<string>( e => e==Environment.ExpandEnvironmentVariables(vsTestConsoleExePath)), It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        public void TestThatSingleTestAssemblyIsPassedCorrectlyInCommandLine()
        {   
            var process = new Mock<IProcess>();
            var target = new VSTest(process.Object)
            {
                PublishTestResults = false
            };

            var parameters = new Dictionary<string, object>
            {
                { "TestAssemblies", new[] { @"C:\Projects\Test\TestAssembly.dll" } }
            };

            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            process.Verify(s => s.Execute(It.IsAny<string>(), It.Is<string>(e => e == @"""C:\Projects\Test\TestAssembly.dll"""), It.IsAny<string>()));
        }

        [TestMethod]
        public void TestThatMultipleTestAssembliesArePassedCorrectlyInCommandLine()
        {
            var process = new Mock<IProcess>();
            var target = new VSTest(process.Object)
            {
                PublishTestResults = false
            };

            var parameters = new Dictionary<string, object>
            {
                { "TestAssemblies", new[] { @"C:\Projects\Test\TestAssembly.dll", @"C:\Projects\Test\TestAssembly2.dll" } }
            };

            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            process.Verify(s => s.Execute(It.IsAny<string>(), It.Is<string>(e => e == @"""C:\Projects\Test\TestAssembly.dll"" ""C:\Projects\Test\TestAssembly2.dll"""), It.IsAny<string>()));
        }

        [TestMethod]
        public void TestThatTestCaseFilterIsPassedCorrectlyInCommandLine()
        {
            var process = new Mock<IProcess>();
            var target = new VSTest(process.Object)
            {
                PublishTestResults = false,
                TestCaseFilter = "Category1"
            };

            var parameters = new Dictionary<string, object>
            {
                { "TestAssemblies", new[] { @"C:\Projects\Test\TestAssembly.dll"}}
            };

            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            process.Verify(s => s.Execute(It.IsAny<string>(), It.Is<string>(e => e.IndexOf(@"/testcasefilter:""Category1""", StringComparison.InvariantCultureIgnoreCase) > 0), It.IsAny<string>()));
        }

        [TestMethod]
        public void TestThatSettingsFileIsPassedCorrectlyInCommandLine()
        {
            var process = new Mock<IProcess>();
            var target = new VSTest(process.Object)
            {
                PublishTestResults = false,
                Settings = "testSettings1.runsettings"
            };

            var parameters = new Dictionary<string, object>
            {
                { "TestAssemblies", new[] { @"C:\Projects\Test\TestAssembly.dll"}}
            };

            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            process.Verify(s => s.Execute(It.IsAny<string>(), It.Is<string>(e => e.IndexOf(@"/settings:""testSettings1.runsettings""", StringComparison.InvariantCultureIgnoreCase) > 0), It.IsAny<string>()));
        }

        [TestMethod]
        public void TestThatPublishResultsCreatedtheCorrectLoggerInCommandLine()
        {
            var process = new Mock<IProcess>();
            var buildDetail = new Mock<IBuildDetail>();
            buildDetail.SetupGet(b=> b.BuildNumber).Returns("1.0.0.1");
            buildDetail.SetupGet(b => b.TeamProject).Returns("MyTeamProject");

            var target = new VSTest(process.Object)
            {
                PublishTestResults = true,
                ProjectCollectionUrl = @"http://tfsserver:8080/collection1/abcd"
            };

            var parameters = new Dictionary<string, object>();
            parameters.Add("TestAssemblies", new[] { @"C:\Projects\Test\TestAssembly.dll"});
            parameters.Add("Build", buildDetail.Object);

            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            process.Verify(s => s.Execute(It.IsAny<string>(), It.Is<string>(e => e.IndexOf(@"/Logger:TfsPublisher;Collection=""http://tfsserver:8080/collection1/abcd"";BuildName=""1.0.0.1"";TeamProject=""MyTeamProject"";Flavor=""Release"";Platform=""Any CPU""", StringComparison.InvariantCultureIgnoreCase) > 0), It.IsAny<string>()));
        }        
    }
}