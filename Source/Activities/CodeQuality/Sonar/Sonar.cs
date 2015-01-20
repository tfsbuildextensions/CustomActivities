//-----------------------------------------------------------------------
// <copyright file="Sonar.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// The Sonar activity allows a build to call the sonar-runner after compilation.
    /// It also provides a way to handle sonar properties required for the sonar-runner automatically.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class Sonar : BaseCodeActivity
    {
        private InArgument<bool> generatePropertiesIfMissing = true;
        private InArgument<string> sonarPropertiesFileName = "sonar-project.properties";

        /// <summary>
        /// Projects (solutions) list to analyze, typically pass the ProjectsToBuild variable.
        /// </summary>
        [Description("Projects to Analyze")]
        [Browsable(true)]
        [RequiredArgument]
        public InArgument<StringList> ProjectsToAnalyze { get; set; }

        /// <summary>
        /// Sonar runner cmd (or bat) file path.
        /// </summary>
        [Description("Sonar Runner command file Path")]
        [Browsable(true)]
        [RequiredArgument]
        public InArgument<string> SonarRunnerPath { get; set; }

        /// <summary>
        /// Gets or sets the build workspace. This is used to obtain the project folder in the build workspace.
        /// </summary>
        /// <value>The workspace used by the current build</value>
        [Browsable(true)]
        [RequiredArgument]
        public InArgument<Workspace> BuildWorkspace { get; set; }

        /// <summary>
        /// Gets or sets the binaries directory. This is the path where Sonar may find all the binaries to analyze.
        /// It is set up by the build process template.
        /// </summary>
        /// <value>The binaries directory as defined by the current build template</value>
        [Browsable(true)]
        [RequiredArgument]
        public InArgument<string> BinariesDirectory { get; set; }

        /// <summary>
        /// The path to a Sonar properties template file, that will be used for generating a customized version every build.
        /// </summary>
        [Description("Path to a template sonar properties file")]
        [Browsable(true)]
        public InArgument<string> SonarPropertiesTemplatePath { get; set; }

        /// <summary>
        /// Gets or sets the option to fail the build if alerts are raised by Sonar.
        /// </summary>
        [Description("Set to true to fail the build if alerts are raised by Sonar")]
        public InArgument<bool> FailBuildOnAlert { get; set; }

        /// <summary>
        /// Gets or sets the option to fail the build if alerts are raised by Sonar.
        /// </summary>
        [Description("If the sonar properties file for the projects to analyze are missing, they can be generated from a template file. Default value is true.")]
        [Browsable(true)]
        public InArgument<bool> GeneratePropertiesIfMissing
        {
            get { return this.generatePropertiesIfMissing; }
            set { this.generatePropertiesIfMissing = value; }
        }

        /// <summary>
        /// Gets or sets the name of the file that will hold the Sonar properties during the build.
        /// </summary>
        [Description("Name of the Sonar for c# properties file. Default value is sonar-project.properties")]
        [Browsable(true)]
        public InArgument<string> SonarPropertiesFileName
        {
            get { return this.sonarPropertiesFileName; }
            set { this.sonarPropertiesFileName = value; }
        }

        /// <summary>
        /// Gets or sets the list of properties names and values that are overriden for a build definition.
        /// </summary>
        /// <example>
        /// sonar.fxcop.mode=skip
        /// sonar.ndeps.mode=active
        /// </example>
        [Description("Sonar properties to be added (or overwritten if found) in the properties file")]
        [Browsable(true)]
        public InArgument<StringList> SonarProperties { get; set; }

        public string TransformSonarProperties(string templatePath, string solutionPath)
        {
            StringList properties = this.SonarProperties.Get(this.ActivityContext);
            IBuildDetail build = this.ActivityContext.GetExtension<IBuildDetail>();
            string buildDefinition = build.BuildDefinition.Name;
            string buildNumber = build.BuildNumber;

            string content = File.ReadAllText(templatePath);
            if (properties != null)
            {
                Regex nameRegex = new Regex("^\\s*(?<name>[^#=\\s]+)\\s*=*.");
                foreach (string propertyLine in properties)
                {
                    MatchCollection matches = nameRegex.Matches(propertyLine);
                    if (matches.Count > 0)
                    {
                        string propertyName = matches[0].Groups["name"].Value;
                        Regex lineRegex = new Regex("[\\x020\\t]*" + propertyName.Replace(".", "\\.") + "[\\x020\\t]*=.*\\r?\\n");
                        if (lineRegex.IsMatch(content))
                        {
                            content = lineRegex.Replace(content, propertyLine + "\r\n");
                        }
                        else
                        {
                            content += "\r\n" + propertyLine;
                        }
                    }
                }
            }

            string binariesDirectory = Path.GetFullPath(this.BinariesDirectory.Get(this.ActivityContext));

            // remove trailing \ if found
            if (binariesDirectory.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                binariesDirectory = binariesDirectory.Remove(binariesDirectory.Length - 1);
            }

            StringBuilder sb = new StringBuilder(content);

            sb.Replace("%BUILD_DEFINITION%", buildDefinition);
            sb.Replace("%BUILD_DEFINITION_UNDERSCORE%", buildDefinition.Replace(' ', '_'));
            sb.Replace("%BUILD_NUMBER%", buildNumber);
            sb.Replace("%BUILD_NUMBER_UNDERSCORE%", buildNumber.Replace(' ', '_'));
            sb.Replace("%BUILD_NUMBER_DEFAULT_SUFFIX%", buildNumber.Replace(buildDefinition + "_", string.Empty));
            sb.Replace("%SOLUTION_FILE%", Path.GetFileName(solutionPath));
            sb.Replace("%SOLUTION_FILE_PATH%", solutionPath);
            sb.Replace("%SOLUTION_DIRECTORY_PATH%", Path.GetDirectoryName(solutionPath));
            sb.Replace("%BINARIES_DIRECTORY_PATH%", binariesDirectory);
            sb.Replace("%BINARIES_DIRECTORY_PATH_SLASH%", binariesDirectory.Replace('\\', '/'));

            return sb.ToString();
        }

        /// <summary>
        /// Executes the logic for this custom activity
        /// </summary>
        protected override void InternalExecute()
        {
            Workspace workspace = this.BuildWorkspace.Get(this.ActivityContext);
            string sonarRunnerPath = this.SonarRunnerPath.Get(this.ActivityContext);
            if (!File.Exists(sonarRunnerPath))
            {
                this.LogBuildError("Sonar runner file not found: " + sonarRunnerPath);
                return;
            }

            foreach (string projectToAnalyze in this.ProjectsToAnalyze.Get(this.ActivityContext))
            {
                if (!string.IsNullOrWhiteSpace(projectToAnalyze))
                {
                    string localProjectPath = workspace.GetLocalItemForServerItem(projectToAnalyze);
                    string localFolderPath = System.IO.Path.GetDirectoryName(localProjectPath);

                    if (this.GeneratePropertiesIfMissing.Get(this.ActivityContext))
                    {
                        string sonarPropertiesPath = Path.Combine(localFolderPath, this.SonarPropertiesFileName.Get(this.ActivityContext));
                        string templatePropertiesPath = this.SonarPropertiesTemplatePath.Get(this.ActivityContext);
                        if (!File.Exists(sonarPropertiesPath))
                        {
                            this.LogBuildMessage("sonar.properties file not found in working folder.");
                            if (!string.IsNullOrWhiteSpace(templatePropertiesPath))
                            {
                                if (File.Exists(templatePropertiesPath))
                                {
                                    this.LogBuildMessage("Generating sonar properties file from " + templatePropertiesPath);
                                    string properties = this.TransformSonarProperties(
                                        templatePropertiesPath,
                                        localProjectPath);
                                    this.LogBuildMessage(properties, BuildMessageImportance.Low);
                                    File.WriteAllText(sonarPropertiesPath, properties, Encoding.UTF8);
                                }
                                else
                                {
                                    this.LogBuildError("Sonar properties template file not found " + templatePropertiesPath);
                                }
                            }
                            else
                            {
                                this.LogBuildError("No sonar properties file found and no template path set up.");
                            }
                        }
                    }

                    string arguments = string.Format("/c \"{0}\"", this.SonarRunnerPath.Get(this.ActivityContext));

                    SonarRunResult result = this.RunProcess("cmd.exe", localFolderPath, arguments);

                    if (result.HasBreakingAlerts)
                    {
                        this.LogBuildError("Sonar analysis has raised critical alerts.");
                        if (this.FailBuildOnAlert.Get(this.ActivityContext))
                        {
                            IBuildDetail buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                            buildDetail.Status = BuildStatus.Failed;
                            buildDetail.Save();
                        }
                    }
                    else
                    {
                        if (result.ReturnCode == 0)
                        {
                            this.LogBuildMessage("Sonar analysis successful.", BuildMessageImportance.High);
                        }
                        else
                        {
                            this.LogBuildError("Sonar analysis has failed.");
                        }
                    }
                }
            }
        }

        private SonarRunResult RunProcess(string fullPath, string workingDirectory, string arguments)
        {
            if (!Directory.Exists(workingDirectory))
            {
                this.LogBuildError(string.Format("Working directory for sonar analysis {0} not found", workingDirectory));
            }

            using (Process proc = new Process())
            {
                var result = new SonarRunResult();

                proc.StartInfo.FileName = fullPath;

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = arguments;
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments, BuildMessageImportance.High);

                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    proc.StartInfo.WorkingDirectory = workingDirectory;
                }

                proc.Start();

                string outputStream = proc.StandardOutput.ReadToEnd();
                string errorStream = proc.StandardError.ReadToEnd();

                IBuildDetail build = this.ActivityContext.GetExtension<IBuildDetail>();
                if (!string.IsNullOrEmpty(build.LogLocation))
                {
                    // log the full output of sonar to the build logs folder
                    string logFolder = Path.GetDirectoryName(build.LogLocation);
                    string sonarLogFile = Path.Combine(logFolder, "Sonar.log");
                    File.WriteAllText(sonarLogFile, outputStream);
                    File.AppendAllText(sonarLogFile, "\n");
                    File.AppendAllText(sonarLogFile, errorStream);
                }
                else
                {
                    this.LogBuildWarning("Drop location not defined. Will not write to log file");
                    return null;
                }
                
                result.HasBreakingAlerts = outputStream.Contains("[BUILD BREAKER]");

                if (outputStream.Length > 0)
                {
                    this.LogBuildMessage(outputStream);
                }

                if (errorStream.Length > 0)
                {
                    this.LogBuildWarning(errorStream);
                }

                proc.WaitForExit();
                result.ReturnCode = proc.ExitCode;
                return result;
            }
        }

        /// <summary>
        /// Internal class for handling a Sonar analysis result
        /// </summary>
        private class SonarRunResult
        {
            public bool HasBreakingAlerts { get; set; }

            public int ReturnCode { get; set; }
        }
    }
}
