//-----------------------------------------------------------------------
// <copyright file="VB6.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// This activity wraps the VB6 compiler. 
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions VB6 Compile Sequence">
    /// <Sequence.Variables>
    ///     <Variable x:TypeArguments="x:String" Default="[SourcesDirectory + &quot;\Development\Iteration 2\VB6\AComponent.vbp&quot;]" Name="ProjectToCompile" />
    /// </Sequence.Variables>
    /// <!-- Compile a VB6 project using default settings -->    
    /// <tav:VB6 
    ///     DisplayName="Compile VB6 for Project" 
    ///     ProjectFile="[ProjectToCompile]" />
    /// <!-- Compile a VB6 project, change project properties  -->    
    /// <tav:VB6 
    ///     DisplayName="Compile VB6 for Project" 
    ///     ProjectFile="[ProjectToCompile]" 
    ///     ChangeProperty="RevisionVer=4;CompatibleMode=0" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.Agent)]
    [System.ComponentModel.Description("Activity to build VB6 projects.")]
    public sealed class VB6 : BaseCodeActivity
    {
        private const char Separator = ';';
        private InArgument<string> toolPath = @"C:\Program Files\Microsoft Visual Studio\VB98\VB6.exe";
        private InArgument<string> outDir = string.Empty;
        private InArgument<string> logFile = string.Empty;

        /// <summary>
        /// Name of VB6 project to compile
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        [Description("Name of VB6 project to compile")]
        public InArgument<string> ProjectFile { get; set; }

        /// <summary>
        /// Output directory. Default is build drop location + vb project directory
        /// </summary>
        [Browsable(true)]
        [Description("Output directory. Default is build drop location + vb project directory")]
        public InArgument<string> OutDir
        {
            get { return this.outDir; }
            set { this.outDir = value; }
        }

        /// <summary>
        /// Logfile for compilation output. Default is build drop location + vb project filename + .log
        /// </summary>
        [Browsable(true)]
        [Description("Logfile for compilation output. Default is build drop location + vb project filename + .log")]
        public InArgument<string> LogFile
        {
            get { return this.logFile; }
            set { this.logFile = value; }
        }
        
        /// <summary>
        /// Path to VB6 compiler. Default is C:\Program Files\Microsoft Visual Studio\VB98\VB6.exe
        /// </summary>
        [Browsable(true)]
        [Description(@"Path to VB6 compiler. Default is C:\Program Files\Microsoft Visual Studio\VB98\VB6.exe")]
        public InArgument<string> ToolPath
        {
            get { return this.toolPath; }
            set { this.toolPath = value; }
        }

        /// <summary>
        /// Working directory used when compiling
        /// </summary>
        [Browsable(true)]
        [Description("Working directory used when compiling")]
        public InArgument<string> WorkingDirectory { get; set; }

        /// <summary>
        /// Project properties to change at build time. Specify properties to change as key=value;key=value
        /// </summary>
        [Browsable(true)]
        [Description("Project properties to change at build time. Specify properties to change as key=value;key=value")]
        public InArgument<string> ChangeProperty { get; set; }

        /// <summary>
        /// Gets the Return Code from the VB6 compile activity
        /// </summary>
        [Description("Gets the Return Code from the VB6 compile activity")]
        public OutArgument<int> ReturnCode { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity.
        /// </summary>
        protected override void InternalExecute()
        {
            this.CompileVB6();
        }

        private void CompileVB6()
        {
            using (Process proc = new Process())
            {
                string fileName = this.toolPath.Get(this.ActivityContext);
                if (!File.Exists(fileName))
                {
                    this.LogBuildError(
                        string.Format(
                            "VB6.exe was not found in the default location. Use ToolPath to specify it. Searched at: {0}",
                            fileName));
                    return;
                }

                proc.StartInfo.FileName = fileName;

                this.ChangeProjectProperties();
                string finalLogFile = this.GenerateLogFile();

                if (this.WorkingDirectory.Expression != null)
                {
                    proc.StartInfo.WorkingDirectory = this.WorkingDirectory.Get(this.ActivityContext);
                }

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = this.GenerateCommandLineCommands(this.ProjectFile.Get(this.ActivityContext), this.GenerateOutDir(), finalLogFile);
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments, BuildMessageImportance.High);

                proc.Start();

                string outputStream = proc.StandardOutput.ReadToEnd();
                if (outputStream.Length > 0)
                {
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
                    this.LogBuildError(File.ReadAllText(finalLogFile));
                }

                if (finalLogFile.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    this.LogBuildLink("VB6 Log File", new Uri(finalLogFile));
                }

                this.ReturnCode.Set(this.ActivityContext, proc.ExitCode);
            }
        }

        private string GenerateOutDir()
        {
            string finalDir = this.OutDir.Get(this.ActivityContext);
            if (string.IsNullOrEmpty(finalDir))
            {
                var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                if (!string.IsNullOrEmpty(buildDetail.DropLocation))
                {
                    finalDir = buildDetail.DropLocation;
                }
                else
                {
                    var projectFile = this.ProjectFile.Get(this.ActivityContext);
                    finalDir = Path.GetDirectoryName(projectFile);                    
                }

                this.LogBuildMessage(string.Format("Generated out directory {0}.", finalDir));
            }

            return finalDir;
        }

        private string GenerateLogFile()
        {
            var finalLogFile = this.LogFile.Get(this.ActivityContext);
            if (string.IsNullOrEmpty(finalLogFile))
            {
                var projectFile = this.ProjectFile.Get(this.ActivityContext);
                var fileName = Path.GetFileName(projectFile);
                if (fileName != null)
                {
                    var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                    if (!string.IsNullOrEmpty(buildDetail.DropLocation))
                    {
                        string logDirectory = Path.Combine(buildDetail.DropLocation, @"logs");
                        if (!Directory.Exists(logDirectory))
                        {
                            Directory.CreateDirectory(logDirectory);
                        }

                        finalLogFile = Path.Combine(logDirectory, fileName.Replace(".vbp", ".log"));
                    }
                    else
                    {
                        finalLogFile = Path.Combine(Path.GetDirectoryName(projectFile) + string.Empty, fileName.Replace(".vbp", ".log"));
                    }
                }

                this.LogBuildMessage(string.Format("Generated log file {0}.", finalLogFile));
            }

            return finalLogFile;
        }

        private void ChangeProjectProperties()
        {
            string changeVbProperty = this.ChangeProperty.Get(this.ActivityContext);
            if (!string.IsNullOrEmpty(changeVbProperty))
            {
                this.LogBuildMessage("START - Changing Properties VBP");

                VBPProject project = new VBPProject(this.ProjectFile.Get(this.ActivityContext));
                if (project.Load())
                {
                    string[] linesProperty = changeVbProperty.Split(Separator);
                    string[] keyProperty = new string[linesProperty.Length];
                    string[] valueProperty = new string[linesProperty.Length];
                    int index;

                    for (index = 0; index <= linesProperty.Length - 1; index++)
                    {
                        if (linesProperty[index].IndexOf("=", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            keyProperty[index] = linesProperty[index].Substring(0, linesProperty[index].IndexOf("=", StringComparison.OrdinalIgnoreCase));
                            valueProperty[index] = linesProperty[index].Substring(linesProperty[index].IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1);
                        }

                        if (!string.IsNullOrEmpty(keyProperty[index]) && !string.IsNullOrEmpty(valueProperty[index]))
                        {
                            this.LogBuildMessage(keyProperty[index] + " -> New value: " + valueProperty[index]);
                            project.SetProjectProperty(keyProperty[index], valueProperty[index], false);
                        }
                    }

                    project.Save();
                }

                this.LogBuildMessage("END - Changing Properties VBP");
            }
        }

        private string GenerateCommandLineCommands(string projectFileName, string directoryName, string logFileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("/m \"{0}\" /outdir \"{1}\" /out \"{2}\" ", projectFileName, directoryName, logFileName);
            this.LogBuildMessage(string.Format("Command line: {0}", sb));
            return sb.ToString();
        }
    }
}
