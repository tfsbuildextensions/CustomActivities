//-----------------------------------------------------------------------
// <copyright file="IBaseActivityMinimumArguments.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities
{
    using System.Activities;
    using System.ComponentModel;

    /// <summary>
    /// Represents the minimum number of arguments that an activity must implement
    /// </summary>
    public interface IBaseActivityMinimumArguments
    {
        /// <summary>
        /// Set to true to fail the build if the activity logs any errors. Default is false
        /// </summary>
        [Description("Set to true to fail the build if errors are logged")]
        InArgument<bool> FailBuildOnError { get; set; }

        /// <summary>
        /// Set to true to fail the build if the activity logs any errors. Default is false
        /// </summary>
        [Description("Set to true to make all warnings errors")]
        InArgument<bool> TreatWarningsAsErrors { get; set; }

        /// <summary>
        /// Set to true to ignore any unhandled exceptions thrown by activities. Default is false
        /// </summary>
        [Description("Set to true to ignore unhandled exceptions")]
        InArgument<bool> IgnoreExceptions { get; set; }

        /// <summary>
        /// Set to true to log the entire stack in the event of an exception. Default is true
        /// <para></para>
        /// <remarks>This parameter is ignored, if <see cref="FailBuildOnError"/> is true or <see cref="TreatWarningsAsErrors"/> is true </remarks>
        /// </summary>
        [Description("Set to true to log the entire stack in the event of an exception")]
        InArgument<bool> LogExceptionStack { get; set; }
    }
}