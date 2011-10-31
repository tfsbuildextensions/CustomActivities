//-----------------------------------------------------------------------
// <copyright file="ILMergeTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TfsBuildExtensions.Activities.Framework;

    /// <summary>
    /// This is a test class for SqlExecuteTest and is intended
    /// to contain all SqlExecuteTest Unit Tests
    /// </summary>
    [TestClass]
    public class ILMergeTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// A test for ExecuteScalar
        /// </summary>
        [TestMethod]
        public void ILMergeTest()
        {
            string outputFile = @"C:\myAssembly.dll";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            // Initialise Instance
            var target = new TfsBuildExtensions.Activities.Framework.ILMerge { Action = ILMergeAction.Merge, OutputFile = outputFile };
            
            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "InputAssemblies", new[] { @"D:\Projects\teambuild2010contrib\MAIN\Source\Activities.ILMerge.Tests\TestFiles\ClassLibrary1.dll", @"D:\Projects\teambuild2010contrib\MAIN\Source\Activities.ILMerge.Tests\TestFiles\ClassLibrary2.dll" } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke(parameters);

            // Test the result
            Assert.IsTrue(File.Exists(outputFile));
        }
    }
}
