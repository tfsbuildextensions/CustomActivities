// -----------------------------------------------------------------------
// <copyright file="ActivityContextProxy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// -----------------------------------------------------------------------
#pragma warning disable 1591
namespace TfsBuildExtensions.Activities.CodeQuality.History
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public interface IActivityContextProxy
    {
        bool Enabled { get; }

        string SourceFileName { get; }

        string HistoryDirectory { get; }

        string HistoryFileName { get; }

        IBuildDetail BuildDetail { get; }

        short HowManyFilesToKeepInDirectory { get; }

        void LogBuildError(string errorMessage);

        void LogBuildMessage(string message);
    }

    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public class ActivityContextProxy : IActivityContextProxy
    {
        private readonly CodeMetricsHistory activity;
        private readonly CodeActivityContext context;

        public ActivityContextProxy(CodeMetricsHistory activity, CodeActivityContext context)
        {
            this.activity = activity;
            this.context = context;
        }

        public bool Enabled
        {
            get
            {
                return this.activity.Enabled.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.SourceFileName"/> values for the current context.
        /// </summary>
        public string SourceFileName
        {
            get
            {
                return this.activity.SourceFileName.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HistoryDirectory"/> values for the current context.
        /// </summary>
        public string HistoryDirectory
        {
            get
            {
                return this.activity.HistoryDirectory.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HistoryFileName"/> values for the current context.
        /// </summary>
        public string HistoryFileName
        {
            get
            {
                return this.activity.HistoryFileName.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HowManyFilesToKeepInDirectory"/> values for the current context.
        /// </summary>
        public short HowManyFilesToKeepInDirectory
        {
            get
            {
                return this.activity.HowManyFilesToKeepInDirectory.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="IBuildDetail"/> for the current context.
        /// </summary>
        public IBuildDetail BuildDetail
        {
            get
            {
                return this.context.GetExtension<IBuildDetail>();
            }
        }

        public void LogBuildError(string errorMessage)
        {
            this.activity.LogBuildError(errorMessage);
        }

        public void LogBuildMessage(string message)
        {
            this.activity.LogBuildMessage(message);
        }
    }
}
