//-----------------------------------------------------------------------
// <copyright file="MetricsInvoker.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeMetrics.Extended
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Class that wraps the call to Metrics.exe
    /// </summary>
    public class MetricsInvoker
    {
        private readonly string metricsExePath;
        private readonly string output;
        private readonly IMetricsLogger logger;

        private MetricsInvoker(string metricsExePath, IEnumerable<string> filesToProcess, string rootPath, string output, IMetricsLogger logger)
        {
            this.metricsExePath = metricsExePath;
            this.output = output;
            this.logger = logger;

            this.Argument = string.Empty;
            foreach (var file in filesToProcess)
            {
                this.Argument += string.Format(" /f:\"{0}\"", Path.Combine(rootPath, file));
            }

            this.Argument += string.Format(" /out:\"{0}\"", output);
            this.Argument = this.Argument.Trim();
        }

        /// <summary>
        /// Argument to Metrics.exe. 
        /// </summary>
        public string Argument { get; private set; }

        /// <summary>
        /// Creates an instance of MetricsInvoker
        /// </summary>
        /// <param name="filesToProcess">Which files to process, Can be wildcards or explicit file names</param>
        /// <param name="rootPath">Root path where to look for binaries</param>
        /// <param name="output">The resulting output from metrics.exe</param>
        /// <param name="logger">Instance of IMetricsLogger</param>
        /// <returns>A MetricsInvoker instance </returns>
        public static MetricsInvoker Create(IEnumerable<string> filesToProcess, string rootPath, string output, IMetricsLogger logger)
        {
            string metricsExePath = Path.Combine(CodeQuality.CodeMetrics.ProgramFilesX86(), @"Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\metrics.exe");
            if (!File.Exists(metricsExePath))
            {
                logger.LogError("Could not locate " + metricsExePath + ". Please download Visual Studio Code Metrics PowerTool 10.0 at http://www.microsoft.com/downloads/en/details.aspx?FamilyID=edd1dfb0-b9fe-4e90-b6a6-5ed6f6f6e615");
                return null;
            }

            return new MetricsInvoker(metricsExePath, filesToProcess, rootPath, output, logger);
        }

        /// <summary>
        /// Calls metrics.exe
        /// </summary>
        /// <returns>True if the exit code from metrics.exe was 0 (zero)</returns>
        public bool Invoke()
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = this.metricsExePath;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = this.Argument;
                this.logger.LogMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
                proc.Start();

                string outputStream = proc.StandardOutput.ReadToEnd();
                if (outputStream.Length > 0)
                {
                    this.logger.LogMessage(outputStream);
                }

                string errorStream = proc.StandardError.ReadToEnd();
                if (errorStream.Length > 0)
                {
                    this.logger.LogError(errorStream);
                }

                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    this.logger.LogError(proc.ExitCode.ToString(CultureInfo.CurrentCulture));
                    return false;
                }

                if (!File.Exists(this.output))
                {
                    this.logger.LogError("Could not locate file " + this.output);
                    return false;
                }
            }

            return true;
        }
    }
}
