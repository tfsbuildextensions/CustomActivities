//-----------------------------------------------------------------------
// <copyright file="SharePointAppDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    public enum SharePointAppAction
    {
        /// <summary>
        /// Imports a SPAppPackage
        /// </summary>
        Import_SPAppPackage,

        /// <summary>
        /// Installs a SPApp
        /// </summary>
        Install_SPApp,

        /// <summary>
        /// Updates a SPAppInstance
        /// </summary>
        Update_SPAppInstance,

        /// <summary>
        /// Uninstalls a SPAppInstance
        /// </summary>
        Uninstall_SPAppInstance,        
        
        /// <summary>
        /// Gets the list of SPAppInstances
        /// </summary>
        Get_SPAppInstance,
    }    

    /// <summary>
    /// An activity that builds and executes PowerShell commands to deploy SharePoint AppPackages to different versions of SharePoint
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SharePointAppDeployment : BaseCodeActivity
    {
        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public SharePointAppAction Action
        {
            get;
            set;
        }

        /// <summary>
        /// The values for the CompatabilityLevel parameter.
        /// </summary>
        [RequiredArgument]
        [Description("The values for the Version parameter.")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Defines the source of the app.
        /// </summary>
        [RequiredArgument]
        [Description("Defines the source of the app.")]
        public string AppSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the App e.g. myapplication.spapp.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "App", Justification = "App is the correct spelling"), Browsable(true)]
        public InArgument<string> AppName { get; set; }        

        /// <summary>
        /// Gets or sets the name of the server to execute the PowerShell script on.
        /// </summary>
        [Browsable(true)]
        public InArgument<string> ServerName { get; set; }

        /// <summary>
        /// Gets or sets the fullpath to the App file e.g. c:\myfolder\myapplication.spapp.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "App", Justification = "App is the correct spelling"), Browsable(true)]
        public InArgument<string> AppLiteralPath { get; set; }

        /// <summary>
        /// Gets or sets the URL of the SharePoint web application to deploy a feature to.
        /// </summary>
        [Browsable(true)]
        public InArgument<string> SiteUrl { get; set; }        

        /// <summary>
        /// Gets or sets the timeout to use when running the powershell script in milliseconds.
        /// </summary>
        [Browsable(true)]
        public InArgument<int> ScriptTimeout { get; set; }       

        /// <summary>
        /// Gets or sets if a any other PowerShell parameter needs to be passed
        /// </summary>
        [Browsable(true)]
        public InArgument<string> OtherParameters { get; set; }        

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
        /// <param name="appSource">The appSource to deploy to</param>
        /// <param name="appName">The name of the SpAppPackage file</param>
        /// <param name="siteUrl">The web application url</param>
        /// <param name="appLiteralPath">The path to the SpAppPackage file</param>        
        /// <param name="otherParameters">A string of adition parameters to append</param>
        /// <returns>The PowerShell command to run</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "guid", Justification = "Using the guid to force an exception to chekc format"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The switch is the easiest to read")]
        internal static string GeneratePowerShellScript(
            string serverName,
            SharePointAppAction action,
            string version,
            string appSource,
            string appName,
            string siteUrl,
            string appLiteralPath,                   
            string otherParameters)
        {
            var command = string.Empty;

            switch (version)
            {
                case "2013":
                    command = "Add-PsSnapin Microsoft.SharePoint.PowerShell; ";
                    switch (action)
                    {
                        case SharePointAppAction.Import_SPAppPackage:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Import-SPAppPackage -Path '{0}' –Site {1} -Source '{2}' -Confirm:$false", appLiteralPath, siteUrl, appSource);
                            break;
                        case SharePointAppAction.Install_SPApp:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Install-SPApp –Identity {0} –WebUrl {1}", appName, siteUrl);
                            break;
                        case SharePointAppAction.Update_SPAppInstance:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Update-SPAppInstance –Identity {0} –App '{1}'", appLiteralPath, appName);
                            break;
                        case SharePointAppAction.Uninstall_SPAppInstance:
                            command = command.AppendFormat(CultureInfo.InvariantCulture, "Uninstall-SPAppInstance –Identity {0}", appName);
                            break;
                        case SharePointAppAction.Get_SPAppInstance:
                            if (string.IsNullOrEmpty(appName))
                            {
                                command = command.AppendFormat(CultureInfo.InvariantCulture, "Get-SPAppInstance");
                            }
                            else
                            {
                                try
                                {
                                    var guid = Guid.Parse(appName);
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "Get-SPAppInstance | where {{$_.id -eq '{0}'}}", appName);
                                }
                                catch (FormatException)
                                {
                                    command = command.AppendFormat(CultureInfo.InvariantCulture, "Get-SPAppInstance | where {{$_.name -eq '{0}'}}", appName);
                                }
                            }

                            command += " | fl -property Displayname, Deployed, Id ;";
                            break;
                        default:
                            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Unknown SharePointAction [{0}] specified", action));
                    }

                    break;
                case "Online":
                    command = "Add-PsSnapin Microsoft.Online.SharePoint.PowerShell; ";
                    break;
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
        internal static SharePointDeploymentStatus[] ProcessPowerShellOutput(SharePointAppAction action, string outputFromScript)
        {
            var results = new List<SharePointDeploymentStatus>();

            switch (action)
            {
                case SharePointAppAction.Get_SPAppInstance:
                    var lines = outputFromScript.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    SharePointDeploymentStatus newItem = null;
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            var sections = line.Split(':');
                            if (sections.Length == 2)
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

            var script = GeneratePowerShellScript(this.ServerName.Get(this.ActivityContext), this.Action, this.Version, this.AppSource, this.AppName.Get(this.ActivityContext), this.SiteUrl.Get(this.ActivityContext), this.AppLiteralPath.Get(this.ActivityContext), this.OtherParameters.Get(this.ActivityContext));

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
