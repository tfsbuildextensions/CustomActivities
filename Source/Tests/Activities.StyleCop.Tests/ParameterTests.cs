//-----------------------------------------------------------------------
// <copyright file="ParameterTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.CodeQuality;

    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Exception_violations_found_if_required_parameters_missing()
        {
            // arrange
            var target = new StyleCop();
            var args = new Dictionary<string, object>
            {
            };
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // act
            var results = invoker.Invoke(args);

            // assert
            // trapped by exception handler
        }
    }
}
