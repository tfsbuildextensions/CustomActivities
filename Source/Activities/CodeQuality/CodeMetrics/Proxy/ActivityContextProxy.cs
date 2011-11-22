// -----------------------------------------------------------------------
// <copyright file="ActivityContextProxy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// -----------------------------------------------------------------------

using System.Activities;
using System.Collections.Generic;

namespace TfsBuildExtensions.Activities.CodeQuality.Proxy
{
    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public interface IActivityContextProxy
    {
        /// <summary>
        /// Returns the <see cref="CodeMetrics.FilesToProcess"/> values for the current context.
        /// </summary>
        IEnumerable<string> FilesToProcess { get; }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.FilesToIgnore"/> values for the current context.
        /// </summary>
        IEnumerable<string> FilesToIgnore { get; }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.BinariesDirectory"/> values for the current context.
        /// </summary>
        string BinariesDirectory { get; }

        /// <summary>
        /// Call the <see cref="BaseCodeActivity.LogBuildMessage"/> method.
        /// </summary>
        void LogBuildMessage(string msg);
    }

    /// <summary>
    /// Provides access to an activity's context without any direct references (decoupling concerns)
    /// </summary>
    public class ActivityContextProxy : IActivityContextProxy
    {
        private readonly CodeMetrics _activity;
        private readonly CodeActivityContext _context;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActivityContextProxy(CodeMetrics activity, CodeActivityContext context)
        {
            _activity = activity;
            _context = context;
        }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.FilesToProcess"/> values for the current context.
        /// </summary>
        public IEnumerable<string> FilesToProcess
        {
            get
            {
                return _activity.FilesToProcess.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.FilesToIgnore"/> values for the current context.
        /// </summary>
        public IEnumerable<string> FilesToIgnore
        {
            get
            {
                return _activity.FilesToIgnore.Get(_context);
            }
        }

        /// <summary>
        /// Returns the <see cref="CodeMetrics.BinariesDirectory"/> values for the current context.
        /// </summary>
        public string BinariesDirectory
        {
            get
            {
                return _activity.BinariesDirectory.Get(_context);
            }
        }

        /// <summary>
        /// Call the <see cref="BaseCodeActivity.LogBuildMessage"/> method.
        /// </summary>
        public void LogBuildMessage(string msg)
        {
            _activity.LogBuildMessage(msg);
        }
    }
}
