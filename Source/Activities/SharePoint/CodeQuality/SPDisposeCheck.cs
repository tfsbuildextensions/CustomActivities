//-----------------------------------------------------------------------
// <copyright file="SPDisposeCheck.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.SharePoint
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// The SPDisposeCheck activity provides a basic wrapper over SPDisposeCheck.exe. See http://msdn.microsoft.com/en-gb/library/bb429449(VS.80).aspx for more details.
    /// <para/>
    /// </summary>   
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SPDisposeCheck : BaseCodeActivity
    {
        /// <summary>
        /// Sets the path to SPDisposeCheckerCmd.exe. Default is [Program Files]\Microsoft\SharePoint Dispose Check\SPDisposeCheck.exe
        /// </summary>
        public InArgument<string> SPDisposePath { get; set; }

        /// <summary>
        /// Sets the Path to Assemblies
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AssemblyDirectory { get; set; }

        /// <summary>
        /// Set the name of the file for the output report
        /// </summary>    
        public InArgument<string> LogFile { get; set; }

        /// <summary>
        /// Gets AnalysisFailed. True if SPDisposeChecker logged Code Analysis errors to the Output file.
        /// </summary>
        public OutArgument<bool> AnalysisFailed { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            string spdisposeIntPath;

            if (string.IsNullOrEmpty(this.SPDisposePath.Get(this.ActivityContext)))
            {
                string programFilePath = Environment.GetEnvironmentVariable("ProgramFiles");
                if (string.IsNullOrEmpty(programFilePath))
                {
                    this.LogBuildError("Failed to read a value from the ProgramFiles Environment Variable");
                    return;
                }

                if (System.IO.File.Exists(programFilePath + @"\Microsoft\SharePoint Dispose Check\SPDisposeCheck.exe"))
                {
                    spdisposeIntPath = programFilePath + @"\Microsoft\SharePoint Dispose Check\SPDisposeCheck.exe";
                }                
                else
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "SPDisposeCheck.exe was not found in the default location. Use SPDisposeCheckPath to specify it. Searched at: {0}", programFilePath + @"\Microsoft\SharePoint Dispose Check\"));
                    return;
                }
            }
            else
            {
                spdisposeIntPath = this.SPDisposePath.Get(this.ActivityContext);
            }

            string arguments = string.Empty;

            if (this.AssemblyDirectory.Get(this.ActivityContext) != null)
            {
                arguments += string.Format("\"{0}\"", this.AssemblyDirectory.Get(this.ActivityContext));               
            }

            if (!string.IsNullOrEmpty(this.LogFile.Get(this.ActivityContext)))
            {
                arguments += " -xml";
            }

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = spdisposeIntPath;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = arguments;
                this.LogBuildMessage(string.Format("Running {0} {1}", proc.StartInfo.FileName, proc.StartInfo.Arguments));
                proc.Start();

                string outputStream = proc.StandardOutput.ReadToEnd();
                if (outputStream.Length > 0)
                {
                    this.LogBuildMessage("****************************************************");
                    this.LogBuildMessage("SPDisposeCheck Started.");
                    this.LogBuildMessage("****************************************************");

                    this.LogBuildMessage(outputStream);

                    if (!string.IsNullOrEmpty(this.LogFile.Get(this.ActivityContext)))
                    {
                         string fileLoc = this.LogFile.Get(this.ActivityContext) + @"\SPDiposeCheckOutput.xml";

                         if (!File.Exists(fileLoc))
                         {
                             using (FileStream fs = File.Create(fileLoc))
                             {
                             }
                         }
                        
                         using (StreamWriter sw = new StreamWriter(fileLoc))
                         {
                             sw.Write(outputStream);
                         }
                    }
                }

                string errorStream = proc.StandardError.ReadToEnd();
                if (errorStream.Length > 0)
                {
                    this.LogBuildError(errorStream);
                }

                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    this.LogBuildError(string.Format("SPDisposeCheck completed with {0} errors, see logfile for details", proc.ExitCode.ToString(CultureInfo.CurrentCulture)));
                    this.AnalysisFailed.Set(this.ActivityContext, true);
                    return;
                }                
            }

            this.LogBuildMessage("SPDisposeCheck completed and reported no issues", BuildMessageImportance.High);     
        }
    }
}
