// -----------------------------------------------------------------------
// <copyright file="ActivityContextProxy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// -----------------------------------------------------------------------

using System;
using System.Activities;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace TfsBuildExtensions.Activities.CodeQuality.History
{
    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public interface IActivityContextProxy
    {
        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.Enabled"/> values for the current context.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.SourceFileName"/> values for the current context.
        /// </summary>
        string SourceFileName { get; }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HistoryDirectory"/> values for the current context.
        /// </summary>
        string HistoryDirectory { get; }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HistoryFileName"/> values for the current context.
        /// </summary>
        string HistoryFileName { get; }

        /// <summary>
        /// Returns the <see cref="IBuildDetail"/> for the current context.
        /// </summary>
        IBuildDetail BuildDetail { get; }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HowManyFilesToKeepInDirectory"/> values for the current context.
        /// </summary>
        Int16 HowManyFilesToKeepInDirectory { get; }

        /// <summary>
        /// Call the <see cref="BaseCodeActivity.LogBuildError"/> method
        /// </summary>
        void LogBuildError(string errorMessage);

        /// <summary>
        /// Call the <see cref="BaseCodeActivity.LogBuildMessage"/> method
        /// </summary>
        void LogBuildMessage(string msg);
    }

    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public class ActivityContextProxy : IActivityContextProxy
    {
        private readonly CodeMetricsHistory _activity;
        private readonly CodeActivityContext _context;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActivityContextProxy(CodeMetricsHistory activity, CodeActivityContext context)
        {
            _activity = activity;
            _context = context;
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.Enabled"/> values for the current context.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _activity.Enabled.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.SourceFileName"/> values for the current context.
        /// </summary>
        public string SourceFileName
        {
            get
            {
                return _activity.SourceFileName.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HistoryDirectory"/> values for the current context.
        /// </summary>
        public string HistoryDirectory
        {
            get
            {
                return _activity.HistoryDirectory.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HistoryFileName"/> values for the current context.
        /// </summary>
        public string HistoryFileName
        {
            get
            {
                return _activity.HistoryFileName.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetricsHistory.HowManyFilesToKeepInDirectory"/> values for the current context.
        /// </summary>
        public Int16 HowManyFilesToKeepInDirectory
        {
            get
            {
                return _activity.HowManyFilesToKeepInDirectory.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="IBuildDetail"/> for the current context.
        /// </summary>
        public IBuildDetail BuildDetail
        {
            get
            {
                return _context.GetExtension<IBuildDetail>();
            }
        }

        /// <summary>
        /// Call the <see cref="BaseCodeActivity.LogBuildError"/> method
        /// </summary>
        public void LogBuildError(string errorMessage)
        {
            _activity.LogBuildError(errorMessage);
        }

        /// <summary>
        /// Call the <see cref="BaseCodeActivity.LogBuildMessage"/> method
        /// </summary>
        public void LogBuildMessage(string msg)
        {
            _activity.LogBuildMessage(msg);
        }
    }
}
