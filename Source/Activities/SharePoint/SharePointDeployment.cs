//-----------------------------------------------------------------------
// <copyright file="SharePointDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SharePoint
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    ///  Possible action for the activity
    /// </summary>
    public enum SharePointAction
    {
        /// <summary>
        /// Adds a solution
        /// </summary>
        AddSolution,

        /// <summary>
        /// Installs a solution
        /// </summary>
        InstallSolution,

        /// <summary>
        /// Updates a solution
        /// </summary>
        UpdateSolution,

        /// <summary>
        /// Uninstalls a solution
        /// </summary>
        UninstallSolution,

        /// <summary>
        /// Removes a solution
        /// </summary>
        RemoveSolution,

        /// <summary>
        /// Enables a feature
        /// </summary>
        EnableFeature,

        /// <summary>
        /// Disables a feature
        /// </summary>
        DisableFeature,

        /// <summary>
        /// Gets the list of features
        /// </summary>
        GetFeature,

        /// <summary>
        /// Gets the list of solutions
        /// </summary>
        GetSolution,
    }

    /// <summary>
    ///  Possible action for the activity
    /// </summary>
    public enum SharePointVersion
    {
        /// <summary>
        /// SharePoint 2010
        /// </summary>
        SP2010,

        /// <summary>
        /// SharePoint 2013
        /// </summary>
        SP2013,

        /// <summary>
        /// SharePoint Online
        /// </summary>
        SPOnline,
    }

    /// <summary>
    ///  Possible action for the activity
    /// </summary>
    public enum SharePointCompatabilityLevel
    {
        /// <summary>
        /// Installs solution to both 14 and 15 directories
        /// </summary>
        AllVersions,

        /// <summary>
        /// Installs solution to 14 hive directory
        /// </summary>
        OldVersions,

        /// <summary>
        /// Installs solution to 15 hive directory
        /// </summary>
        NewVersions,
    }

    /// <summary>
    /// An activity that builds and executes PowerShell commands to deploy SharePoint Solutions to different versions of SharePoint
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SharePointDeployment : BaseCodeActivity
    {
        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public SharePointAction Action
        {
            get;
            set;
        }

        /// <summary>
        /// The values for the CompatabilityLevel parameter.
        /// </summary>
        [RequiredArgument]
        [Description("The values for the Version parameter.")]
        public SharePointVersion Version
        {
            get;
            set;
        }

        /// <summary>
        /// The values for the CompatabilityLevel parameter.
        /// </summary>
        [RequiredArgument]
        [Description("The values for the CompatabilityLevel parameter.")]
        public SharePointCompatabilityLevel CompatabilityLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the WSP e.g. mysolution.wsp.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wsp", Justification = "Wsp is the correct spelling"), Browsable(true)]
        public InArgument<string> WspName { get; set; }

        /// <summary>
        /// Gets or sets the name of the server to execute the PowerShell script on.
        /// </summary>
        [Browsable(true)]
        public InArgument<string> ServerName { get; set; }

        /// <summary>
        /// Gets or sets the fullpath to the WSP file e.g. c:\myfolder\mysolution.wsp.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wsp", Justification = "Wsp is the correct spelling"), Browsable(true)]
        public InArgument<string> WspLiteralPath { get; set; }

        /// <summary>
        /// Gets or sets the URL of the SharePoint web application to deploy a feature to.
        /// </summary>
        [Browsable(true)]
        public InArgument<string> SiteUrl { get; set; }

        /// <summary>
        /// Gets or sets if a solution should be deployed to the GAC.
        /// </summary>
        [Browsable(true)]
        public InArgument<bool> GacDeploy { get; set; }

        /// <summary>
        /// Gets or sets the timeout to use when running the powershell script in milliseconds.
        /// </summary>
        [Browsable(true)]
        public InArgument<int> ScriptTimeout { get; set; }

        /// <summary>
        /// Gets or sets if a solution should be force installed.
        /// </summary>
        [Browsable(true)]
        public InArgument<bool> Force { get; set; }

        /// <summary>
        /// Gets or sets if a any other PowerShell parameter needs to be passed
        /// </summary>
        [Browsable(true)]
        public InArgument<string> OtherParameters { get; set; }

        /// <summary>
        /// Gets or sets the name of the feature to be deployed
        /// </summary>
        [Browsable(true)]
        public InArgument<string> FeatureName { get; set; }

        /// <summary>
        /// Gets or sets the result of the underling powershell command
        /// </summary>
        [Browsable(true)]
        public OutArgument<SharePointDeploymentStatus[]> ScriptResult
        {
            get;
            set;
        }

        /// <summary>
        /// Generates the PowerShell
        /// </summary>
        /// <param name="serverName">The name of the server to run the command on</param>
        /// <param name="action">The action to perform</param>
        /// <param name="version">The version to target</param>
        /// <param name="compatabilityLevel">The compatabilityLevel to target</param>
        /// <param name="wspName">The name of the WSP file</param>
        /// <param name="siteUrl">The web application url</param>
        /// <param name="wspLiteralPath">The path to the WSP file</param>
        /// <param name="featureName">The name of a feature within a solution</param>
        /// <param name="gacDeployment">True if the wsp should be deployed to the GAC</param>
        /// <param name="force">True if the command should be forced event if a solution is partial deployed</param>
        /// <param name="otherParameters">A string of adition parameters to append</param>
        /// <returns>The PowerShell command to run</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "guid", Justification = "Using the guid to force an exception to chekc format"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The switch is the easiest to read")]
        internal static string GeneratePowerShellScript(
            string serverName,
            SharePointAction action,
            SharePointVersion version,
            SharePointCompatabilityLevel compatabilityLevel,
            string wspName,
            string siteUrl,
            string wspLiteralPath,
            string featureName,
            bool gacDeployment,
            bool force,
            string otherParameters)
        {
            string command;

            switch (version)
            {
                case SharePointVersion.SP2010:
                    command = "Add-PsSnapin Microsoft.SharePoint.PowerShell; ";

                    switch (action)
                    {
                        case SharePointAction.AddSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Add-SPSolution -LiteralPath '{0}'", wspLiteralPath);
                            break;
                        case SharePointAction.InstallSolution:

                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Install-SPSolution –Identity {0}", wspName);
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" –WebApplication {0}", siteUrl);
                            }

                            if (gacDeployment)
                            {
                                command += " -GACDeployment";
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.UpdateSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Update-SPSolution –Identity {0} –LiteralPath '{1}'", wspName, wspLiteralPath);
                            if (gacDeployment)
                            {
                                command += " -GACDeployment";
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.UninstallSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Uninstall-SPSolution –Identity {0} -Confirm:$false", wspName);
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" –WebApplication {0}", siteUrl);
                            }

                            break;
                        case SharePointAction.RemoveSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Remove-SPSolution –Identity {0} -Confirm:$false", wspName);
                            break;
                        case SharePointAction.EnableFeature:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "enable-spfeature –Identity {0}", featureName);
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" -Url {0}", siteUrl);
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.DisableFeature:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "disable-spfeature –Identity {0} -Confirm:$false", featureName.Replace(" ", "_"));
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" -Url {0}", siteUrl);
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.GetSolution:
                            if (string.IsNullOrEmpty(wspName))
                            {
                                command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spsolution");
                            }
                            else
                            {
                                try
                                {
                                    var guid = Guid.Parse(wspName);
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spsolution | where {{$_.id -eq '{0}'}}", wspName);
                                }
                                catch (FormatException)
                                {
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spsolution | where {{$_.name -eq '{0}'}}", wspName);
                                }
                            }

                            command += " | fl -property Displayname, Deployed, Id ;";
                            break;
                        case SharePointAction.GetFeature:
                            if (string.IsNullOrEmpty(featureName))
                            {
                                command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spfeature");
                            }
                            else
                            {
                                try
                                {
                                    var guid = Guid.Parse(featureName);
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spfeature | where {{$_.id -eq '{0}'}}", featureName);
                                }
                                catch (FormatException)
                                {
                                    // we need to replace spaces with underscore
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spfeature | where {{$_.displayname -eq '{0}'}}", featureName.Replace(' ', '_'));
                                }
                            }

                            // we now need to add the handling to make sure the format is consistant both locally and remotely
                            // using the plus operator to make sure no {0} confusion
                            command += " | fl -property Displayname, Id ;";

                            break;
                        default:
                            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Unknown SharePointAction [{0}] specified", action));
                    }

                    break;
                case SharePointVersion.SP2013:
                    command = "Add-PsSnapin Microsoft.SharePoint.PowerShell; ";

                    switch (action)
                    {
                        case SharePointAction.AddSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Add-SPSolution -LiteralPath '{0}'", wspLiteralPath);
                            break;
                        case SharePointAction.InstallSolution:

                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Install-SPSolution –Identity {0} -CompatibilityLevel {1}", wspName, compatabilityLevel);
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" –WebApplication {0}", siteUrl);
                            }

                            if (gacDeployment)
                            {
                                command += " -GACDeployment";
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.UpdateSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Update-SPSolution –Identity {0} –LiteralPath '{1}'", wspName, wspLiteralPath);
                            if (gacDeployment)
                            {
                                command += " -GACDeployment";
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.UninstallSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Uninstall-SPSolution –Identity {0} -CompatibilityLevel {1} -Confirm:$false", wspName, compatabilityLevel);
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" –WebApplication {0}", siteUrl);
                            }

                            break;
                        case SharePointAction.RemoveSolution:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Remove-SPSolution –Identity {0} -Confirm:$false", wspName);
                            break;
                        case SharePointAction.EnableFeature:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "enable-spfeature –Identity {0} -CompatibilityLevel {1}", featureName, compatabilityLevel);
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" -Url {0}", siteUrl);
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.DisableFeature:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "disable-spfeature –Identity {0} -Confirm:$false", featureName.Replace(" ", "_"));
                            if (string.IsNullOrEmpty(siteUrl) == false)
                            {
                                command = command.AppendFormat(" -Url {0}", siteUrl);
                            }

                            if (force)
                            {
                                command += " -Force";
                            }

                            break;
                        case SharePointAction.GetSolution:
                            if (string.IsNullOrEmpty(wspName))
                            {
                                command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spsolution");
                            }
                            else
                            {
                                try
                                {
                                    var guid = Guid.Parse(wspName);
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spsolution | where {{$_.id -eq '{0}'}}", wspName);
                                }
                                catch (FormatException)
                                {
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spsolution | where {{$_.name -eq '{0}'}}", wspName);
                                }
                            }

                            command += " | fl -property Displayname, Deployed, Id ;";
                            break;
                        case SharePointAction.GetFeature:
                            if (string.IsNullOrEmpty(featureName))
                            {
                                command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spfeature");
                            }
                            else
                            {
                                try
                                {
                                    var guid = Guid.Parse(featureName);
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spfeature | where {{$_.id -eq '{0}'}}", featureName);
                                }
                                catch (FormatException)
                                {
                                    // we need to replace spaces with underscore
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "get-spfeature | where {{$_.displayname -eq '{0}'}}", featureName.Replace(' ', '_'));
                                }
                            }

                            // we now need to add the handling to make sure the format is consistant both locally and remotely
                            // using the plus operator to make sure no {0} confusion
                            command += " | fl -property Displayname, Id ;";

                            break;
                        default:
                            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Unknown SharePointAction [{0}] specified", action));
                    }

                    break;
                case SharePointVersion.SPOnline:
                    command = "Add-PsSnapin Microsoft.Online.SharePoint.PowerShell; ";
                    break;
                default:
                    throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Unknown SharePointAction [{0}] specified", action));
            }

            if (string.IsNullOrEmpty(otherParameters) == false)
            {
                command = command.AppendFormat(" {0}", otherParameters);
            }

            if (string.IsNullOrEmpty(serverName))
            {
                return command;
            }

            return string.Format(CultureInfo.InvariantCulture, "invoke-command -computername {0} {{{1}}}", serverName, command);
        }

        /// <summary>
        /// Capture the return results for any commands that return lists
        /// </summary>
        /// <param name="action">The command passed</param>
        /// <param name="outputFromScript">The raw text from the powershell script</param>
        /// <returns>An array of processed lines</returns>
        internal static SharePointDeploymentStatus[] ProcessPowerShellOutput(SharePointAction action, string outputFromScript)
        {
            var results = new List<SharePointDeploymentStatus>();

            switch (action)
            {
                case SharePointAction.GetFeature:
                case SharePointAction.GetSolution:
                    var lines = outputFromScript.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    SharePointDeploymentStatus newItem = null;
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            var sections = line.Split(':');
                            if ((sections.Length == 2))
                            {
                                switch (sections[0].Trim())
                                {
                                    case "DisplayName":
                                        newItem = new SharePointDeploymentStatus();
                                        newItem.Name = sections[1].Trim();
                                        newItem.Deployed = true; // set a default as features don't pass this
                                        break;
                                    case "Id":
                                        newItem.Id = Guid.Parse(sections[1].Trim());
                                        results.Add(newItem); // we need to make sure the ID is the last in the list
                                        break;
                                    case "Deployed":
                                        newItem.Deployed = bool.Parse(sections[1].Trim());
                                        break;
                                }
                            }
                        }
                    }

                    break;
            }

            return results.ToArray();
        }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "We are trapping the context to be safe, it is noted it is not an argument")]
        protected override void InternalExecute()
        {
            if (this.ActivityContext == null)
            {
                throw new ArgumentNullException("context");
            }

            var script = GeneratePowerShellScript(this.ServerName.Get(this.ActivityContext), this.Action, this.Version, this.CompatabilityLevel, this.WspName.Get(this.ActivityContext), this.SiteUrl.Get(this.ActivityContext), this.WspLiteralPath.Get(this.ActivityContext), this.FeatureName.Get(this.ActivityContext), this.GacDeploy.Get(this.ActivityContext), this.Force.Get(this.ActivityContext), this.OtherParameters.Get(this.ActivityContext));

            this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Running command '{0}'", script), BuildMessageImportance.High);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "powershell";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = script;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                proc.Start();
                proc.WaitForExit(this.ScriptTimeout.Get(this.ActivityContext));

                var output = proc.StandardOutput.ReadToEnd();
                this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Script output [{0}]", output), BuildMessageImportance.Low);
                this.ScriptResult.Set(this.ActivityContext, ProcessPowerShellOutput(this.Action, output));

                if (proc.ExitCode != 0)
                {
                    if (this.FailBuildOnError.Get(this.ActivityContext))
                    {
                        this.LogBuildError(string.Format(CultureInfo.InvariantCulture, "Powershell script exit code {0}: {1}", proc.ExitCode, proc.StandardError.ReadToEnd()));
                    }
                    else
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Powershell script exit code {0}: {1}", proc.ExitCode, proc.StandardError.ReadToEnd()), BuildMessageImportance.High);
                    }
                }
            }

            // this would all be easier if we could run the powershell directly as below, but we see the error below due to SP2010 being built against .net 3.5
            // error: Microsoft SharePoint is not supported with version 4.0.30319.17020 of the Microsoft .Net Runtime.. Stack Trace:    at System.Management.Automation.Internal.PipelineProcessor.SynchronousExecuteEnumerate(Object input, Hashtable errorResults, Boolean enumerate)    at System.Management.Automation.PipelineNode.Execute(Array input, Pipe outputPipe, ArrayList& resultList, ExecutionContext context)    at System.Management.Automation.StatementListNode.ExecuteStatement(ParseTreeNode statement, Array input, Pipe outputPipe, ArrayList& resultList, ExecutionContext context). Inner Exception: Microsoft SharePoint is not supported with version 4.0.30319.17020 of the Microsoft .Net Runtime.
            /*
            using (var runspace = RunspaceFactory.CreateRunspace(new WorkflowPsHost(this.ActivityContext)))
            {
                runspace.Open();

                using (var pipeline = runspace.CreatePipeline(script))
                {
                    var output = pipeline.Invoke();
                    output.ToArray();
                   
                }
            }
             */
        }
    }
}
