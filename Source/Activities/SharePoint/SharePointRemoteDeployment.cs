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
    using System.Management.Automation.Runspaces;
    using TfsBuildExtensions.Activities.Scripting;

    /// <summary>
    ///  Possible action for the activity
    /// </summary>
    public enum SharePointRemoteAction
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
    /// An activity that builds and executes PowerShell commands to deploy SharePoint Solutions to different versions of SharePoint
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SharePointRemoteDeployment : BaseCodeActivity
    {
        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public SharePointRemoteAction Action
        {
            get;
            set;
        }        

        /// <summary>
        /// The values for the CompatabilityLevel parameter.
        /// </summary>
        [Browsable(true), Description("The values for the CompatabilityLevel parameter.")]
        public InArgument<string> CompatabilityLevel
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
        /// Gets or sets if a solution should be force installed.
        /// </summary>
        [Browsable(true), Description("Use CredSSP parameter for PS Remoting.")]
        public InArgument<bool> UseCredSSP { get; set; }

        /// <summary>
        /// Gets or sets if a solution should be force installed.
        /// </summary>
        [Browsable(true), Description("Domain of the account to access the server.")]
        public InArgument<string> Domain { get; set; }

        /// <summary>
        /// Gets or sets if a solution should be force installed.
        /// </summary>
        [Browsable(true), Description("UserName of the account to access the server.")]
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// Gets or sets if a solution should be force installed.
        /// </summary>
        [Browsable(true), Description("Password of the account to access the server.")]
        public InArgument<string> Password { get; set; }

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
        /// <param name="compatabilityLevel">The compatabilityLevel to target</param>
        /// <param name="wspName">The name of the WSP file</param>
        /// <param name="siteUrl">The web application url</param>
        /// <param name="wspLiteralPath">The path to the WSP file</param>
        /// <param name="featureName">The name of a feature within a solution</param>
        /// <param name="gacDeployment">True if the wsp should be deployed to the GAC</param>
        /// <param name="force">True if the command should be forced event if a solution is partial deployed</param>
        /// <param name="otherParameters">A string of adition parameters to append</param>
        /// <param name="useCredSSP">True if want to use CredSSP on Powershell script</param>
        /// <param name="domain">Domain of the account to access the server</param>
        /// <param name="userName">UserName of the account to access the server</param>
        /// <param name="password">Password of the account to access the server</param>
        /// <returns>The PowerShell command to run</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "guid", Justification = "Using the guid to force an exception to chekc format"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The switch is the easiest to read")]
        internal static string GeneratePowerShellScript(
            string serverName,
            SharePointRemoteAction action,
            string compatabilityLevel,
            string wspName,
            string siteUrl,
            string wspLiteralPath,
            string featureName,
            bool gacDeployment,
            bool force,
            string otherParameters,
            bool useCredSSP,
            string domain,
            string userName,
            string password)
        {   
            string command = " Add-PsSnapin Microsoft.SharePoint.PowerShell; ";

            switch (action)
            {
                case SharePointRemoteAction.AddSolution:
                    command = command.AppendFormat(CultureInfo.InvariantCulture, "Add-SPSolution -LiteralPath '{0}'", wspLiteralPath);
                    break;
                case SharePointRemoteAction.InstallSolution:

                    command = command.AppendFormat(CultureInfo.InvariantCulture, "Install-SPSolution –Identity {0} ", wspName);
                    if (string.IsNullOrEmpty(compatabilityLevel) == false)
                    {
                        command = command.AppendFormat(" -CompatibilityLevel {0}", compatabilityLevel);
                    }

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
                case SharePointRemoteAction.UpdateSolution:
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
                case SharePointRemoteAction.UninstallSolution:
                    command = command.AppendFormat(CultureInfo.InvariantCulture, "Uninstall-SPSolution –Identity {0} -Confirm:$false", wspName);
                    if (string.IsNullOrEmpty(compatabilityLevel) == false)
                    {
                        command = command.AppendFormat(" -CompatibilityLevel {0}", compatabilityLevel);
                    }

                    if (string.IsNullOrEmpty(siteUrl) == false)
                    {
                        command = command.AppendFormat(" –WebApplication {0}", siteUrl);
                    }

                    break;
                case SharePointRemoteAction.RemoveSolution:
                    command = command.AppendFormat(CultureInfo.InvariantCulture, "Remove-SPSolution –Identity {0} -Confirm:$false", wspName);
                    break;
                case SharePointRemoteAction.EnableFeature:
                    command = command.AppendFormat(CultureInfo.InvariantCulture, "enable-spfeature –Identity {0} ", featureName);
                    if (string.IsNullOrEmpty(compatabilityLevel) == false)
                    {
                        command = command.AppendFormat(" -CompatibilityLevel {0}", compatabilityLevel);
                    }

                    if (string.IsNullOrEmpty(siteUrl) == false)
                    {
                        command = command.AppendFormat(" -Url {0}", siteUrl);
                    }

                    if (force)
                    {
                        command += " -Force";
                    }

                    break;
                case SharePointRemoteAction.DisableFeature:
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
                case SharePointRemoteAction.GetSolution:
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
                case SharePointRemoteAction.GetFeature:
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


            if (string.IsNullOrEmpty(otherParameters) == false)
            {
                command = command.AppendFormat(" {0}", otherParameters);
            }                     

            if (useCredSSP)
            {
                return string.Format(CultureInfo.InvariantCulture, " invoke-command -computername {0} -Authentication CredSSP -credential $pp {{{1}}}", serverName, command);
            }

            if (!string.IsNullOrEmpty(domain) &&
                !string.IsNullOrEmpty(userName))
            {
                return string.Format(CultureInfo.InvariantCulture, " invoke-command -computername {0} -credential $pp {{{1}}}", serverName, command);
            }

            return string.Format(CultureInfo.InvariantCulture, " invoke-command -computername {0} {{{1}}}", serverName, command);
        }

        /// <summary>
        /// Capture the return results for any commands that return lists
        /// </summary>
        /// <param name="action">The command passed</param>
        /// <param name="outputFromScript">The raw text from the powershell script</param>
        /// <returns>An array of processed lines</returns>
        internal static SharePointDeploymentStatus[] ProcessPowerShellOutput(SharePointRemoteAction action, string outputFromScript)
        {
            var results = new List<SharePointDeploymentStatus>();

            switch (action)
            {
                case SharePointRemoteAction.GetFeature:
                case SharePointRemoteAction.GetSolution:
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

            string creds = string.Empty;

            if (!string.IsNullOrEmpty(this.Domain.Get(this.ActivityContext)) &&
                !string.IsNullOrEmpty(this.UserName.Get(this.ActivityContext)))
            {
                creds = creds.AppendFormat(@"$pw= convertto-securestring '{0}' -asplaintext –force; $pp = new-object -typename System.Management.Automation.PSCredential -argumentlist '{1}\{2}',$pw; ", this.Password.Get(this.ActivityContext), this.Domain.Get(this.ActivityContext), this.UserName.Get(this.ActivityContext));
            }

            var script = GeneratePowerShellScript(this.ServerName.Get(this.ActivityContext), this.Action, this.CompatabilityLevel.Get(this.ActivityContext), this.WspName.Get(this.ActivityContext), this.SiteUrl.Get(this.ActivityContext), this.WspLiteralPath.Get(this.ActivityContext), this.FeatureName.Get(this.ActivityContext), this.GacDeploy.Get(this.ActivityContext), this.Force.Get(this.ActivityContext), this.OtherParameters.Get(this.ActivityContext), this.UseCredSSP.Get(this.ActivityContext), this.Domain.Get(this.ActivityContext), this.UserName.Get(this.ActivityContext), this.Password.Get(this.ActivityContext));


            this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Running command '{0}'", script), BuildMessageImportance.High);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "powershell";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = creds += script;
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
