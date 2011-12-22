//-----------------------------------------------------------------------
// <copyright file="IMetricsLogger.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeMetrics
{
    /// <summary>
    /// Interface for logging metric errors back to the build log
    /// </summary>
    public interface IMetricsLogger
    {
        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">Message</param>
        void LogError(string message);

        /// <summary>
        /// Log information message
        /// </summary>
        /// <param name="message">Message</param>
        void LogMessage(string message);
    }
}