//-----------------------------------------------------------------------
// <copyright file="ICodeMetricsLogger.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeMetrics
{
    /// <summary>
    /// Interface for logging progress
    /// </summary>
    public interface ICodeMetricsLogger
    {
        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="message">Error description</param>
        void LogError(string message);

        /// <summary>
        /// Logs information
        /// </summary>
        /// <param name="message">Information message</param>
        void LogMessage(string message);
    }
}