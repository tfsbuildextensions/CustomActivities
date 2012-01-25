//-----------------------------------------------------------------------
// <copyright file="CatNetScan.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.SharePoint
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// The CatNetScan activity provides a basic wrapper over CATNetCmd.exe. See http://msdn.microsoft.com/en-gb/library/bb429449(VS.80).aspx for more details.
    /// <para/>
    /// </summary>   
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class CatNetScan : BaseCodeActivity
    {
        /// <summary>
        /// Sets the path to CATNetCmd.exe. Default is [Program Files]\Microsoft\CAT.NET\CATNetCmd.exe
        /// </summary>
        public InArgument<string> CatNetPath { get; set; }

        /// <summary>
        /// Required. The path of an assembly file to analyze. Multiple file paths and wildcards are not supported. This is a required parameter. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AssemblyDirectory { get; set; }

        /// <summary>
        /// Required. The path to a directory which contains .NET configuration files for analysis.
        /// </summary>
        public InArgument<string> ConfigDirectory { get; set; }

        /// <summary>
        /// Required. The file to store the analysis report in.  It is also used to do an analysis of how many issues were found. 
        /// </summary>   
        [RequiredArgument]
        public InArgument<string> Report { get; set; }

        /// <summary>
        /// Optional. The path to a file or directory that contains analysis rule(s).  The engine will use the default rules included with the product by default. 
        /// </summary>
        public InArgument<string> Rules { get; set; }

        /// <summary>
        /// Optional. The XSL file to use to transform the report.  By default, the packaged XSL transform included in the product will be used. 
        /// </summary>
        public InArgument<string> ReportXsl { get; set; }

        /// <summary>
        /// Optional. The output file to store the XSLT transform output in.  By default, the HTML report will be saved in 'report.html' in the current working directory.  
        /// </summary>
        public InArgument<string> ReportXslOutput { get; set; }

        /// <summary>
        /// Optional. Enables flag to display verbose message when displaying results.
        /// </summary>
        public InArgument<bool> Verbose { get; set; }

        /// <summary>
        /// Gets AnalysisFailed. True if CatNetScan logged Code Analysis errors to the Output file.
        /// </summary>
        public OutArgument<bool> AnalysisFailed { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            string catNetPath;
         
            if (string.IsNullOrEmpty(this.CatNetPath.Get(this.ActivityContext)))
            {
                string programFilePath = Environment.GetEnvironmentVariable("ProgramFiles");
                if (string.IsNullOrEmpty(programFilePath))
                {
                    this.LogBuildError("Failed to read a value from the ProgramFiles Environment Variable");
                    return;
                }

                if (System.IO.File.Exists(programFilePath + @"\Microsoft\CAT.NET\CATNetCmd.exe"))
                {
                    catNetPath = programFilePath + @"\Microsoft\CAT.NET\CATNetCmd.exe";
                }
                else
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "CATNetCmd.exe was not found in the default location. Use CATNetCmd to specify it. Searched at: {0}", programFilePath + @"\Microsoft\CAT.NET\"));
                    return;
                }
            }
            else
            {
                catNetPath = this.CatNetPath.Get(this.ActivityContext);
            }

            string arguments = string.Empty;

            if (!string.IsNullOrEmpty(this.AssemblyDirectory.Get(this.ActivityContext)))
            {
                arguments += string.Format("/file:\"{0}\"", this.AssemblyDirectory.Get(this.ActivityContext));             
            }

            if (!string.IsNullOrEmpty(this.ConfigDirectory.Get(this.ActivityContext)))
            {
                arguments += string.Format(" /configdir:\"{0}\"", this.ConfigDirectory.Get(this.ActivityContext));
            }

            if (!string.IsNullOrEmpty(this.Rules.Get(this.ActivityContext)))
            {
                arguments += string.Format(" /rules:\"{0}\"", this.Rules.Get(this.ActivityContext));
            }

            if (!string.IsNullOrEmpty(this.Report.Get(this.ActivityContext)))
            {
                arguments += string.Format(" /report:\"{0}\"", this.Report.Get(this.ActivityContext));
            }

            if (!string.IsNullOrEmpty(this.ReportXsl.Get(this.ActivityContext)))
            {
                arguments += string.Format(" /reportxsl:\"{0}\"", this.ReportXsl.Get(this.ActivityContext));
            }

            if (!string.IsNullOrEmpty(this.ReportXslOutput.Get(this.ActivityContext)))
            {
                arguments += string.Format(" /reportxsloutput:\"{0}\"", this.ReportXslOutput.Get(this.ActivityContext));
            }

            if (this.Verbose.Get(this.ActivityContext))
            {
                arguments += " /verbose";
            }

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = catNetPath;
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
                    this.LogBuildMessage("Cat.Net Scan Started.");
                    this.LogBuildMessage("****************************************************");

                    this.LogBuildMessage(outputStream);
                }

                string errorStream = proc.StandardError.ReadToEnd();
                if (errorStream.Length > 0)
                {
                    this.LogBuildError(errorStream);
                }

                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    // note the command line not seem to return errorlevels under normal usage, even if parameters are mistyped
                    this.LogBuildError(string.Format("CAT.NET analysis exited with errorcode {0}", proc.ExitCode.ToString(CultureInfo.CurrentCulture)));
                    this.AnalysisFailed.Set(this.ActivityContext, true);
                    return;
                }

                // check for the number of issues
                var document = new System.Xml.XPath.XPathDocument(this.Report.Get(this.ActivityContext));
                var nav = document.CreateNavigator();
                var issueCount = (double)nav.Evaluate("sum(/Report/Rules/Rule/TotalResults)"); // this xpath should return a double

                if (issueCount != 0)
                {
                    // note the command line not seem to return errorlevels under normal usage, even if parameters are mistyped
                    this.LogBuildError(string.Format("CAT.NET analysis reported {0} issues, see logfile for details", issueCount));
                    this.AnalysisFailed.Set(this.ActivityContext, true);
                    return;
                }
            }

            this.LogBuildMessage("CAT.NET analysis completed and reported no issues.", BuildMessageImportance.High);
        }
    }
}
