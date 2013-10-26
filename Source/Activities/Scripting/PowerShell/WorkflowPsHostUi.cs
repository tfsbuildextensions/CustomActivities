//-----------------------------------------------------------------------
// <copyright file="WorkflowPsHostUi.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Scripting
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;

    internal class WorkflowPsHostUi : PSHostUserInterface
    {
        private readonly CodeActivityContext activityContext;
        private readonly WorkflowRawPsHostUi rawUI;

        public WorkflowPsHostUi(CodeActivityContext activityContext)
        {
            this.activityContext = activityContext;
            this.rawUI = new WorkflowRawPsHostUi();
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return this.rawUI; }
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return new Dictionary<string, PSObject>();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return 0;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return null;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return null;
        }

        public override string ReadLine()
        {
            return string.Empty;
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            return default(System.Security.SecureString);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            this.activityContext.TrackBuildMessage(value, BuildMessageImportance.Normal);
        }

        public override void Write(string value)
        {
            this.activityContext.TrackBuildMessage(value, BuildMessageImportance.Normal);
        }

        public override void WriteDebugLine(string message)
        {
            this.activityContext.TrackBuildMessage(message, BuildMessageImportance.Low);
        }

        public override void WriteErrorLine(string value)
        {
            this.activityContext.TrackBuildError(value);
        }

        public override void WriteLine(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                this.activityContext.TrackBuildMessage(value, BuildMessageImportance.Normal);
            }
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            this.activityContext.TrackBuildMessage(string.Format(CultureInfo.CurrentCulture, "{0} Progress {1}% Complete", record.CurrentOperation, record.PercentComplete));
        }

        public override void WriteVerboseLine(string message)
        {
            this.activityContext.TrackBuildMessage(message, BuildMessageImportance.Low);
        }

        public override void WriteWarningLine(string message)
        {
            this.activityContext.TrackBuildWarning(message);
        }
    }
}
