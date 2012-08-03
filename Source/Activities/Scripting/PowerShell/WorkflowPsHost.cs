//-----------------------------------------------------------------------
// <copyright file="WorkflowPsHost.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Scripting
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Host;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;

    internal class WorkflowPsHost : PSHost
    {
        private readonly CodeActivityContext activityContext;
        private readonly WorkflowPsHostUi hostUI;
        private readonly Guid instanceId;

        public WorkflowPsHost(CodeActivityContext activityContext)
        {
            this.activityContext = activityContext;
            this.instanceId = Guid.NewGuid();
            this.hostUI = new WorkflowPsHostUi(activityContext);
        }

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return CultureInfo.CurrentCulture; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return CultureInfo.CurrentUICulture; }
        }

        public override Guid InstanceId
        {
            get { return this.instanceId; }
        }

        public override string Name
        {
            get { return "Workflow Powershell Host"; }
        }

        public override PSHostUserInterface UI
        {
            get { return this.hostUI; }
        }

        public override Version Version
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                return new Version(versionInfo.ProductVersion);
            }
        }

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        public override void NotifyBeginApplication()
        {
            this.activityContext.TrackBuildMessage(string.Format(this.CurrentCulture, ActivityResources.BeginActionMessage, "PSHost"), BuildMessageImportance.Low);
        }

        public override void NotifyEndApplication()
        {
            this.activityContext.TrackBuildMessage(string.Format(this.CurrentCulture, ActivityResources.CompleteActionMessage, "PSHost"), BuildMessageImportance.Low);
        }

        public override void SetShouldExit(int exitCode)
        {
            this.activityContext.TrackBuildMessage(string.Format(this.CurrentCulture, "Should Exit {0}", exitCode), BuildMessageImportance.Low);
        }
    }
}
