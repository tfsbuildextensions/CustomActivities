//-----------------------------------------------------------------------
// <copyright file="CacheTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Tests
{
    using System; 
    using System.Activities;
    using System.Collections.Generic;
    using System.Xml.XPath;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public void Setting_the_cache_option_causes_the_results_to_be_cached_in_the_default_directory()
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
                 { "CacheResults", true },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.IsTrue(System.IO.File.Exists(resultsFile));
            var document = new XPathDocument(resultsFile);
            var nav = document.CreateNavigator();
            Assert.AreEqual(6d, nav.Evaluate("count(/stylecopresultscache/sourcecode/violations/violation)"));
        }

        [TestMethod]
        public void Not_setting_the_cache_option_causes_the_results_to_not_be_cached_in_the_default_directory()
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
                 { "CacheResults", false },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            Assert.IsFalse(System.IO.File.Exists(resultsFile));
        }

        /* parameters that currently have no unit tests
         <x:Property Name="ForceFullAnalysis" Type="InArgument(x:Boolean)" />
         */
    }
}
