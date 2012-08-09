//-----------------------------------------------------------------------
// <copyright file="StyleCopTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.CodeQuality;

    /// <summary>
    /// A set of test that check the passing of setting files into stylecop
    /// These rely on the paths beinbg correct for the source and setting files,
    /// that the StyleCop.CSharp and StyleCopCShare.Rules are in the same folder as the activity
    /// and that the max violations are set to > 0 (defaults to 100000)
    /// </summary>
    [TestClass]
    public class StyleCopTests
    {
        [TestMethod]
        public void Check_a_single_file_with_default_rules_shows_violations_and_fails()
        {
            // arrange
            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {

                 { "SourceFiles", new string[] { @"TestFiles\FileWith7Errors.cs" } },
                 { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(false, results["Succeeded"]);
            Assert.AreEqual(7, results["ViolationCount"]);
        }


       

        [TestMethod]
        public void Check_a_single_file_with_default_rules_shows_no_violations_and_passes()
        {
            // arrange
            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "SourceFiles", new string[] { @"TestFiles\FileWith0Errors.cs" } },
                 { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(true, results["Succeeded"]);
            Assert.AreEqual(0, results["ViolationCount"]);
        }

        [TestMethod]
        public void Check_a_single_file_with_some_rules_disabled_shows_less_violations_and_fails()
        {
            // arrange
            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "SourceFiles", new string[] { @"TestFiles\FileWith7Errors.cs" } }, 
                 { "SettingsFile", @"TestFiles\SettingsDisableSA1200.StyleCop" }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(false, results["Succeeded"]);
            Assert.AreEqual(3, results["ViolationCount"]);
        }

        [TestMethod]
        public void Check_a_directory_with_defaults_rules_shows_violations_and_fails()
        {
            // arrange
            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "SourceFiles", new string[] { @"TestFiles" } },
                 { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(false, results["Succeeded"]);
            Assert.AreEqual(10, results["ViolationCount"]);
        }

        [TestMethod]
        public void Check_a_directory_with_defaults_rules_will_creating_a_text_logfile_showing_violations()
        {
            // arrange
            var fileName = "LogFile.Txt";
            System.IO.File.Delete(fileName);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "SourceFiles", new string[] { @"TestFiles" } },
                 { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                 { "LogFile", fileName }
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.IsTrue(System.IO.File.Exists(fileName));
            Assert.AreEqual(10, System.IO.File.ReadAllLines(fileName).Length);
        }

        [TestMethod]
        public void Check_a_directory_with_limit_on_violation_count_shows_only_first_few_violations()
        {
            // arrange
            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "SourceFiles", new string[] { @"TestFiles" } },
                { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                { "MaximumViolationCount", 2 },
            };

            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(false, results["Succeeded"]);
            Assert.AreEqual(2, results["ViolationCount"]);  // we get 10 issues, but report only 2
        }
    }
}
