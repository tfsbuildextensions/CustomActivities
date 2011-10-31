//-----------------------------------------------------------------------
// <copyright file="LoggingTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.XPath;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.CodeQuality;

    [TestClass]
    public class LoggingTests
    {
        [TestMethod]
        public void Check_a_file_with_no_issues_and_defaults_rules_will_not_create_a_text_logfile()
        {
            // arrange
            var fileName = "LogFile.Txt";
            System.IO.File.Delete(fileName);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "SourceFiles", new string[] { @"TestFiles\FileWith0Errors.cs" } },
                 { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                 { "LogFile", fileName }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.IsFalse(System.IO.File.Exists(fileName));
        }
#if DEBUG
    // test relies on Debug.WriteLine
        [TestMethod]
        public void Can_choose_to_list_a_file_added_in_the_build_log()
        {
            // arrange
            var monitor = new DebugMonitor("Adding file to check");
            Trace.Listeners.Add(monitor);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "SourceFiles", new string[] { @"TestFiles\FileWith6Errors.cs" } },
                { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                { "ShowOutput", true }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(1, monitor.Writes);
        }

        [TestMethod]
        public void Can_choose_to_not_list_a_file_added_in_the_build_log()
        {
            // arrange
            var monitor = new DebugMonitor("Adding file to check");
            Trace.Listeners.Add(monitor);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "SourceFiles", new string[] { @"TestFiles\FileWith6Errors.cs" } },
                { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                { "ShowOutput", false }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(0, monitor.Writes);
        }

        [TestMethod]
        public void Can_choose_to_not_list_a_directory_of_files_added_in_the_build_log()
        {
            // arrange
            var monitor = new DebugMonitor("Adding file to check");
            Trace.Listeners.Add(monitor);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "SourceFiles", new string[] { @"TestFiles" } },
                { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                { "ShowOutput", false }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(0, monitor.Writes);
        }

        [TestMethod]
        public void Can_choose_to_list_a_directory_of_files_added_in_the_build_log()
        {
            // arrange
            var monitor = new DebugMonitor("Adding file to check");
            Trace.Listeners.Add(monitor);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "SourceFiles", new string[] { @"TestFiles" } },
                { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                { "ShowOutput", true }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(3, monitor.Writes);
        }
#endif

        [TestMethod]
        public void Can_set_the_name_of_the_name_of_output_XML_file()
        {
            // arrange
            var resultsFile = "out.xml";
            System.IO.File.Delete(resultsFile);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "SourceFiles", new string[] { @"TestFiles\FileWith6Errors.cs" } },
                { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                { "XmlOutputFile", resultsFile },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.IsTrue(System.IO.File.Exists(resultsFile));
            var document = new XPathDocument(resultsFile);
            var nav = document.CreateNavigator();
            Assert.AreEqual(6d, nav.Evaluate("count(/StyleCopViolations/Violation)"));
        }
    }
}
