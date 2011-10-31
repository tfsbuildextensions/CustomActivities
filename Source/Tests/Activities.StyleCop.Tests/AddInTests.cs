//-----------------------------------------------------------------------
// <copyright file="AddInTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using TfsBuildExtensions.Activities.CodeQuality;

    [TestClass]
    public class AddInTests
    {
        [TestMethod]
        public void Extra_rules_can_loaded_from_a_directory_that_is_not_a_sub_directory_of_current_location()
        {
            // arrange
            var resultsFile = "StyleCop.Cache";
            System.IO.File.Delete(resultsFile);

            // create the activity
            var target = new StyleCop();

            // create a parameter set
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                 { "SourceFiles", new string[] { @"TestFiles\FileWith6Errors.cs" } },
                 { "SettingsFile", @"TestFiles\AllSettingsEnabled.StyleCop" },
                 { "AdditionalAddInPaths", new string[] { @"..\Activities.StyleCop.Tests\AddIns" } }, // the directory cannot be a sub directory of current as this is automatically scanned
              };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.AreEqual(false, results["Succeeded"]);
            Assert.AreEqual(7, results["ViolationCount"]); // 6 core violations + the extra custom one
        }
    }
}
