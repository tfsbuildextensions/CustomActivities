//-----------------------------------------------------------------------
// <copyright file="BaseActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.ComponentModel;
    using TfsBuildExtensions.Activities.Internal;

    /// <summary>
    /// Base class for implementing activities in code as if derived from <see cref="System.Activities.Activity"/>
    /// <para>
    /// This class is abstract and in order for derived activities to be implemented they need to implement the
    /// CreateBody method which has to return the Activity to be executed.
    /// </para>
    /// <para>The activity implemented by the derived class will be wrapped around a try catch and this class implements the
    /// same semantics as <see cref="TfsBuildExtensions.Activities.BaseCodeActivity"/>
    /// </para>
    /// </summary>
    public abstract class BaseActivity : Activity, IBaseActivityMinimumArguments
    {
        private InArgument<bool> logExceptionStack = true;

        /// <summary>
        /// Initializes a new instance of the BaseActivity class
        /// </summary>
        protected BaseActivity()
        {
            this.Implementation = () => this.CreateBody();
        }

        /// <summary>
        /// Set to true to fail the build if the activity logs any errors. Default is false
        /// </summary>
        [Description("Set to true to fail the build if errors are logged")]
        public InArgument<bool> FailBuildOnError { get; set; }

        /// <summary>
        /// Set to true to fail the build if the activity logs any errors. Default is false
        /// </summary>
        [Description("Set to true to make all warnings errors")]
        public InArgument<bool> TreatWarningsAsErrors { get; set; }

        /// <summary>
        /// Set to true to ignore any unhandled exceptions thrown by activities. Default is false
        /// </summary>
        [Description("Set to true to ignore unhandled exceptions")]
        public InArgument<bool> IgnoreExceptions { get; set; }

        /// <summary>
        /// Set to true to log the entire stack in the event of an exception. Default is true
        /// <para></para>
        /// <remarks>This parameter is ignored, if <see cref="FailBuildOnError"/> is true or <see cref="TreatWarningsAsErrors"/> is true </remarks>
        /// </summary>
        [Description("Set to true to log the entire stack in the event of an exception")]
        public InArgument<bool> LogExceptionStack
        {
            get { return this.logExceptionStack; }
            set { this.logExceptionStack = value; }
        }

        /// <summary>
        /// Creates the code for wrapping an activity.
        /// Returns a <see cref="System.Activities.Statements.TryCatch "/>activity that implements error handling logic
        /// </summary>
        /// <returns>The code for wrapping activity</returns>
        internal Activity CreateBody()
        {
            var exceptionArgument = new DelegateInArgument<Exception>();

            return new TryCatch
            {
                Try = this.CreateInternalBody(),

                Catches =
                {
                    new Catch<FailingBuildException>
                    {
                        Action = new ActivityAction<FailingBuildException>
                        {
                            Handler = new @If
                            {
                                Condition = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),

                                Else = new Rethrow()
                            }
                        }
                    },

                    new Catch<Exception>
                    {
                        Action = new ActivityAction<Exception>
                        {
                            Argument = exceptionArgument,
                            Handler = new Sequence
                            {
                                Activities =
                                {
                                    new @If
                                    {
                                        Condition = new InArgument<bool>(env => this.LogExceptionStack.Get(env)),

                                        Then = new LogBuildError
                                        {
                                            Message = new InArgument<string>(env => FormatLogExceptionStackMessage(exceptionArgument.Get(env)))
                                        }
                                    },
                                    new @If
                                    {
                                        Condition = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),

                                        Else = new Rethrow()
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Returns the implementation for the derived activity
        /// </summary>
        /// <returns>the code to execute the activity</returns>
        protected abstract Activity CreateInternalBody();

        /// <summary>
        /// Formats the message for logging the exception
        /// </summary>
        /// <param name="exception">the exception to be logged</param>
        /// <returns>The formated message</returns>
        private static string FormatLogExceptionStackMessage(Exception exception)
        {
            string innerExceptionMessage = string.Empty;

            if (exception.InnerException != null)
            {
                innerExceptionMessage = string.Format("Inner Exception: {0}", exception.InnerException.Message);
            }

            return string.Format("Error: {0}. Stack Trace: {1}. {2}", exception.Message, exception.StackTrace, innerExceptionMessage);
        }
    }
}