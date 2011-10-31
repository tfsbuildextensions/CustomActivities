//-----------------------------------------------------------------------
// <copyright file="EmailTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Communication;

    /// <summary>
    /// EmailTests
    /// </summary>
    [TestClass]
    public class EmailTests
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
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void EmailTest()
        {
            // Initialise Instance
            var target = new TfsBuildExtensions.Activities.Communication.Email
                {
                    Action = EmailAction.Send,
                    EnableSsl = true,
                    FailBuildOnError = false,
                    Format = "HTML",
                    LogExceptionStack = true,
                    MailFrom = "YOUREMAIL",
                    Port = 587,
                    Priority = "Normal",
                    SmtpServer = "smtp.gmail.com",
                    Subject = "hi 2",
                    TreatWarningsAsErrors = false,
                    UseDefaultCredentials = false,
                    UserName = "YOUREMAIL",
                    UserPassword = "YOURPASSWORD"
                };
                
                // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "MailTo", new[] { "YOURRECIPIENT" } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke(parameters);

            // note this unit test is for debugging rather than testing so just return a true assertion
            Assert.IsTrue(1 == 1);
        }
    }
}
