//-----------------------------------------------------------------------
// <copyright file="MetricsLogger.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeMetrics
{
    /// <summary>
    /// Implements IMetricLogger
    /// </summary>
    public class MetricsLogger : IMetricsLogger
    {
        private readonly ICodeMetricsLogger activity;

        /// <summary>
        /// Initializes a new instance of the MetricsLogger class
        /// </summary>
        /// <param name="activity">Interface to activity</param>
        public MetricsLogger(ICodeMetricsLogger activity)
        {
            this.activity = activity;
        }

        /// <summary>
        /// Logs error message back to the calling activity
        /// </summary>
        /// <param name="message">Message</param>
        public void LogError(string message)
        {
            this.activity.LogError(message);
        }

        /// <summary>
        /// Logs information message back to activity
        /// </summary>
        /// <param name="message">Message</param>
        public void LogMessage(string message)
        {
            this.activity.LogMessage(message);
        }
    }
}