//-----------------------------------------------------------------------
// <copyright file="WindowsProcess.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Diagnostics;
    using System.Text;

    internal delegate void LogMessage(string message);

    /// <summary>
    /// The WindowsProcess class encapsulates a Windows process and implements the IProcess interface.
    /// </summary>
    internal class WindowsProcess : IProcess
    {
        private readonly Action<string> logMessage;
        private readonly StringBuilder stdOut; 
        private readonly StringBuilder stdError; 

        public WindowsProcess()
        {
            this.logMessage = null;
            this.stdOut = new StringBuilder();
            this.stdError = new StringBuilder();
        }

        public WindowsProcess(Action<string> logMessage)
        {
            this.logMessage = logMessage;
        }

        /// <summary>
        /// Gets the standard output.
        /// </summary>
        internal string StandardOutput
        {
            get { return this.stdOut.ToString(); }
        }

        /// <summary>
        /// Gets the standard error.
        /// </summary>
        internal string StandardError
        {
            get { return this.stdError.ToString(); }
        }

        /// <summary>
        /// Executes a windows process.
        /// </summary>
        /// <param name="executablePath">The path to the executable file.</param>
        /// <param name="commandLineArguments">Command line arguments to be passed to the executable file.</param>
        /// <param name="workingDirectory">The working directory to execute the executable file from.</param>
        /// <returns>True if the process ran successfully. False otherwise</returns>
        public bool Execute(string executablePath, string commandLineArguments, string workingDirectory)
        {
            using (var proc = new Process())
            {
                try
                {
                    var startInfo = new ProcessStartInfo(executablePath, commandLineArguments)
                        {
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            WorkingDirectory = workingDirectory
                        };

                    this.LogMessageInternal("Running " + startInfo + " " + commandLineArguments);
                    proc.StartInfo = startInfo;
                    proc.OutputDataReceived += this.StandardOutHandler;
                    proc.ErrorDataReceived += this.StandardErrorHandler;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit(int.MaxValue);
                }
                finally
                {
                    // get the exit code and release the process handle
                    if (!proc.HasExited)
                    {
                        // not exited yet within our timeout so kill the process
                        proc.Kill();

                        while (!proc.HasExited)
                        {
                            System.Threading.Thread.Sleep(50);
                        }
                    }
                }
                
                return proc.ExitCode == 0;
            }
        }

        private void LogMessageInternal(string message)
        {
            if (this.logMessage != null)
            {
                this.logMessage(message);
            }
        }

        private void StandardErrorHandler(object sendingProcess, DataReceivedEventArgs lineReceived)
        {
            // Collect the error output.
            if (!string.IsNullOrEmpty(lineReceived.Data))
            {
                // Add the text to the collected errors.
                this.stdError.AppendLine(lineReceived.Data);
            }
        }

        private void StandardOutHandler(object sendingProcess, DataReceivedEventArgs lineReceived)
        {
            // Collect the command output.
            if (!string.IsNullOrEmpty(lineReceived.Data))
            {
                // Add the text to the collected output.
                this.stdOut.AppendLine(lineReceived.Data);
            }
        }
    }
}
