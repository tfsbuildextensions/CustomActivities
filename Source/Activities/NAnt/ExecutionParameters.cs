//-----------------------------------------------------------------------
// <copyright file="ExecutionParameters.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//---------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// ExecutionParameters
    /// </summary>
    public sealed class ExecutionParameters
    {
        /// <summary>
        /// Initializes a new instance of the NAntParameters class.
        /// </summary>
        public ExecutionParameters()
        {
            this.Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// Properties
        /// </summary>
        public Dictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// TargetFramework
        /// </summary>
        public string TargetFramework { get; set; }

        /// <summary>
        /// BuildFilePath
        /// </summary>
        public string BuildFilePath { get; set; }

        /// <summary>
        /// Verbose
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Debug
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// BuildFilePathExists
        /// </summary>
        public bool BuildFilePathExists
        {
            get
            {
                return this.BuildFilePath != null && File.Exists(this.BuildFilePath);
            }
        }

        internal string CreateCommandLine(string logFile)
        {
            var paramList = new List<string>();

            if (!string.IsNullOrEmpty(this.TargetFramework))
            {
                paramList.Add(string.Format("-t:{0}", this.TargetFramework.Trim()));
            }

            paramList.AddRange(this.Properties.Select(kv => string.Format("-D:{0}=\"{1}\"", kv.Key, kv.Value)));

            paramList.Add(string.Format("-verbose{0}", this.Verbose ? "+" : "-"));
            paramList.Add(string.Format("-debug{0}", this.Debug ? "+" : "-"));
            paramList.Add(string.Format("-buildfile:{0}", this.BuildFilePath));
            if (!string.IsNullOrEmpty(logFile))
            {
                paramList.Add(string.Format("-logfile:\"{0}\"", logFile));
            }

            return string.Join(" ", paramList);
        }
    }
}
