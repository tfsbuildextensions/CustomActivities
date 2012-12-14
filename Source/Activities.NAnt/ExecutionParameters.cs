//-----------------------------------------------------------------------
// <copyright file="ExecutionParameters.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//---------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System.Collections.Generic;
    using System.IO;

    public sealed class ExecutionParameters
    {
        /// <summary>
        /// Initializes a new instance of the NAntParameters class.
        /// </summary>
        public ExecutionParameters()
        {
            this.Properties = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Properties { get; private set; }

        public string TargetFramework { get; set; }

        public string BuildFilePath { get; set; }

        public bool Verbose { get; set; }

        public bool Debug { get; set; }

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

            foreach (var kv in this.Properties)
            {
                paramList.Add(string.Format("-D:{0}=\"{1}\"", kv.Key, kv.Value));
            }

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
