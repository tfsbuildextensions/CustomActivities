// -----------------------------------------------------------------------
// <copyright file="ActivityContextProxy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// -----------------------------------------------------------------------
#pragma warning disable 1591
namespace TfsBuildExtensions.Activities.CodeQuality.Proxy
{
    using System.Activities;
    using System.Collections.Generic;

    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public interface IActivityContextProxy
    {
        IEnumerable<string> FilesToProcess { get; }

        IEnumerable<string> FilesToIgnore { get; }

        string BinariesDirectory { get; }

        void LogBuildMessage(string message);
    }

    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public class ActivityContextProxy : IActivityContextProxy
    {
        private readonly CodeMetrics activity;
        private readonly CodeActivityContext context;

        public ActivityContextProxy(CodeMetrics activity, CodeActivityContext context)
        {
            this.activity = activity;
            this.context = context;
        }

        public IEnumerable<string> FilesToProcess
        {
            get
            {
                return this.activity.FilesToProcess.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.FilesToIgnore"/> values for the current context.
        /// </summary>
        public IEnumerable<string> FilesToIgnore
        {
            get
            {
                return this.activity.FilesToIgnore.Get(this.context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.BinariesDirectory"/> values for the current context.
        /// </summary>
        public string BinariesDirectory
        {
            get
            {
                return this.activity.BinariesDirectory.Get(this.context);
            }
        }

        public void LogBuildMessage(string message)
        {
            this.activity.LogBuildMessage(message);
        }
    }
}
