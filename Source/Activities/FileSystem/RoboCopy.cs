//-----------------------------------------------------------------------
// <copyright file="RoboCopy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.FileSystem
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// RoboCopyAction
    /// </summary>
    public enum RoboCopyAction
    {
        /// <summary>
        /// Copy
        /// </summary>
        Copy
    }

    /// <summary>
    /// <para>This activity wraps RoboCopy. Successful non-zero exit codes from Robocopy. Use the ReturnCode property to access the exit code from Robocopy</para>
    /// <b>Valid Action values are:</b>
    /// <para><i>Copy</i></para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class RoboCopy : BaseCodeActivity
    {
        // Set a Default action
        private RoboCopyAction action = RoboCopyAction.Copy;
        private InArgument<string> files = "*.*";

        /// <summary>
        /// Source Directory (drive:\path or \\server\share\path).
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Source { get; set; }

        /// <summary>
        /// Destination Dir  (drive:\path or \\server\share\path).
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Destination { get; set; }

        /// <summary>
        /// File(s) to copy  (names/wildcards: default is "*.*").
        /// </summary>
        public InArgument<string> Files
        {
            get { return this.files; }
            set { this.files = value; }
        }

        /// <summary>
        /// Gets the Return Code from RoboCopy
        /// </summary>
        public OutArgument<int> ReturnCode { get; set; }

        /// <summary>
        /// Type 'robocopy.exe /?' at the command prompt for all available options
        /// </summary>
        public InArgument<string> Options { get; set; }

        /// <summary>
        /// Set to true to log output to the build progress screen. Default is false
        /// </summary>
        public InArgument<bool> LogToBuild { get; set; }

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public RoboCopyAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary> 
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case RoboCopyAction.Copy:
                    this.Copy();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private void Copy()
        {
            ProcessStartInfo psi = new ProcessStartInfo { FileName = "RoboCopy.exe", UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, Arguments = this.GenerateCommandLineCommands() };
            this.LogBuildMessage("Running " + psi.FileName + " " + psi.Arguments);

            using (Process process = Process.Start(psi))
            {
                using (ManualResetEvent mreOut = new ManualResetEvent(false), mreErr = new ManualResetEvent(false))
                {
                    process.OutputDataReceived += (o, e) =>
                    {
                        if (e.Data == null)
                        {
                            mreOut.Set();
                        }
                        else
                        {
                            LogBuildMessage(e.Data);
                        }
                    };

                    process.BeginOutputReadLine();
                    process.ErrorDataReceived += (o, e) =>
                    {
                        if (e.Data == null)
                        {
                            mreErr.Set();
                        }
                        else
                        {
                            LogBuildMessage(e.Data);
                        }
                    };

                    process.BeginErrorReadLine();
                    process.StandardInput.Close();
                    process.WaitForExit();

                    mreOut.WaitOne();
                    mreErr.WaitOne();

                    this.ReturnCode.Set(this.ActivityContext, process.ExitCode);
                    switch (process.ExitCode)
                    {
                        case 0:
                            this.LogBuildMessage("Return Code 0. No errors occurred, and no copying was done. The source and destination directory trees are completely synchronized.");
                            break;
                        case 1:
                            this.LogBuildMessage("Return Code 1. One or more files were copied successfully (that is, new files have arrived).");
                            break;
                        case 2:
                            this.LogBuildMessage("Return Code 2. Some Extra files or directories were detected. Examine the output log. Some housekeeping may be needed.");
                            break;
                        case 3:
                            this.LogBuildMessage("Return Code 3. One or more files were copied successfully (that is, new files have arrived). Some Extra files or directories were detected. Examine the output log. Some housekeeping may be needed.");
                            break;
                        case 4:
                            this.LogBuildMessage("Return Code 4. Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary.");
                            break;
                        case 5:
                            this.LogBuildMessage("Return Code 5. One or more files were copied successfully (that is, new files have arrived). Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary.");
                            break;
                        case 6:
                            this.LogBuildMessage("Return Code 6. Some Extra files or directories were detected. Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary.");
                            break;
                        case 7:
                            this.LogBuildMessage("Return Code 7. One or more files were copied successfully (that is, new files have arrived). Some Extra files or directories were detected. Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary.");
                            break;
                        case 8:
                            this.LogBuildError("Return Code 8. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 9:
                            this.LogBuildError("Return Code 9. One or more files were copied successfully (that is, new files have arrived). Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 10:
                            this.LogBuildError("Return Code 10. Some Extra files or directories were detected. Examine the output log. Some housekeeping may be needed. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 11:
                            this.LogBuildError("Return Code 11. One or more files were copied successfully (that is, new files have arrived). Some Extra files or directories were detected. Examine the output log. Some housekeeping may be needed. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 12:
                            this.LogBuildError("Return Code 12. Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 13:
                            this.LogBuildError("Return Code 13. One or more files were copied successfully (that is, new files have arrived). Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 14:
                            this.LogBuildError("Return Code 14. Some Extra files or directories were detected. Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 15:
                            this.LogBuildError("Return Code 15. One or more files were copied successfully (that is, new files have arrived). Some Extra files or directories were detected. Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary. Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.");
                            break;
                        case 16:
                            this.LogBuildError("Return Code 16. Serious error. RoboCopy did not copy any files. This is either a usage error or an error due to insufficient access privileges on the source or destination directories.");
                            break;
                    }
                }
            }
        }

        private string GenerateCommandLineCommands()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\"{0}\" \"{1}\" {2} ", this.Source.Get(this.ActivityContext), this.Destination.Get(this.ActivityContext), this.Files.Get(this.ActivityContext));
            if (this.Options.Expression != null)
            {
                sb.Append(this.Options.Get(this.ActivityContext));
            }

            return sb.ToString();
        }
    }
}