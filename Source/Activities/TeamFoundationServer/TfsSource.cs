//-----------------------------------------------------------------------
// <copyright file="TFSSource.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// TF action to perform
    /// </summary>
    public enum TfsAction
    {
        /// <summary>
        /// Add item(s)
        /// </summary>
        Add,

        /// <summary>
        /// Checkin item(s)
        /// </summary>
        Checkin,

        /// <summary>
        /// Checkout item(s)
        /// </summary>
        Checkout,

        /// <summary>
        /// Delete item(s)
        /// </summary>
        Delete,

        /// <summary>
        /// Undelete item(s)
        /// </summary>
        Undelete,

        /// <summary>
        /// Undo checked out item(s)
        /// </summary>
        UndoCheckout
    }

    /// <summary>
    /// Activity to wrap TF.exe commands for simple integration in the build process.
    /// <para />
    /// <b>Valid Action values are:</b>
    /// <para><i>Add</i> - Add an item</para>
    /// <para><i>Checkin</i> - Checkin pending changes</para>
    /// <para><i>Checkout</i> - Checkout an item</para>
    /// <para><i>Delete</i> - Delete an item</para>
    /// <para><i>Undelete</i> - Undelete a deleted item</para>
    /// <para><i>UndoCheckout</i> - Undo a checked out item</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions TfsSource Sequence">
    /// <Sequence.Variables>
    ///     <Variable x:TypeArguments="x:String" Default="C:\Builds\1\Tailspin Toys\Tailspin Toys - Iteration 2 - Extension\Sources\Development\Iteration 2\VB6\Class1.cls" Name="ItemSpec" />
    ///     <Variable x:TypeArguments="x:String" Default="C:\Builds\1\Tailspin Toys\Tailspin Toys - Iteration 2 - Extension\Sources\Development\Iteration 2\VB6" Name="WorkingDirectory" />
    ///     <Variable x:TypeArguments="x:String" Default="A checkin comment" Name="CheckinComment" />
    ///     <Variable x:TypeArguments="x:String" Default="A policy override reason" Name="OverrideReason" />
    ///     <Variable x:TypeArguments="x:String" Default="DefaultCollection" Name="ProjectCollection" />
    /// </Sequence.Variables>
    /// <!-- Add item to version control -->
    /// <tat:TfsSource 
    ///     DisplayName="Add file" 
    ///     Action="[TfsAction.Add]" 
    ///     Itemspec="[ItemSpec]" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// <!-- Checkin pending changes with comment -->
    /// <tat:TfsSource 
    ///     DisplayName="Checkin" 
    ///     Action="[TfsAction.Checkin]" 
    ///     Collection="[ProjectCollection]" 
    ///     Comments="[CheckinComment]" 
    ///     OverrideReason="{x:Null}" 
    ///     Notes="{x:Null}" 
    ///     Recursive="{x:Null}" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// <!-- Checkin pending changes with policy override -->
    /// <tat:TfsSource 
    ///     DisplayName="Checkin" 
    ///     Action="[TfsAction.Checkin]" 
    ///     Collection="[ProjectCollection]" 
    ///     OverrideReason="[OverrideReason]" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// <!-- Checkout item from version control -->
    /// <tat:TfsSource 
    ///     DisplayName="Checkout" 
    ///     Action="[TfsAction.Checkout]" 
    ///     Itemspec="[ItemSpec]" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// <!-- Delete item from version control -->
    /// <tat:TfsSource 
    ///     DisplayName="Delete file" 
    ///     Action="[TfsAction.Delete]" 
    ///     Collection="[ProjectCollection]" 
    ///     Itemspec="[ItemSpec]" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// <!-- Undelete delete from version control -->
    /// <tat:TfsSource 
    ///     DisplayName="Undelete file" 
    ///     Action="[TfsAction.Undelete]" 
    ///     Itemspec="[ItemSpec]" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// <!-- Undo Checkout -->
    /// <tat:TfsSource 
    ///     DisplayName="Undo Checkout" 
    ///     Action="[TfsAction.UndoCheckout]" 
    ///     Collection="[ProjectCollection]" 
    ///     Itemspec="[ItemSpec]" 
    ///     WorkingDirectory="[WorkingDirectory]" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    [Description("Activity to wrap TF.exe commands for simple integration in the build process")]
    public class TfsSource : BaseCodeActivity
    {
        private InArgument<string> toolPath = string.Empty;
        
        /// <summary>
        /// The action to perform
        /// </summary>
        public InArgument<TfsAction> Action { get; set; }

        /// <summary>
        /// Item to operate on
        /// </summary>
        [Browsable(true)]
        public InArgument<string> Itemspec { get; set; }

        /// <summary>
        /// Path to TF.exe. Defaults to %VS100COMNTOOLS%\..\IDE\tf.exe
        /// </summary>
        [Browsable(true)]
        public InArgument<string> ToolPath
        {
            get { return this.toolPath; }
            set { this.toolPath = value; }
        }

        /// <summary>
        /// Working directory used when executing command
        /// </summary>
        [Browsable(true)]
        public InArgument<string> WorkingDirectory { get; set; }

        /// <summary>
        /// Optional checkin comment
        /// </summary>
        [Browsable(true)]
        public InArgument<string> Comments { get; set; }

        /// <summary>
        /// Optional checkin notes
        /// </summary>
        [Browsable(true)]
        public InArgument<string> Notes { get; set; }

        /// <summary>
        /// Optional checkin policy override reason
        /// </summary>
        [Browsable(true)]
        public InArgument<string> OverrideReason { get; set; }

        /// <summary>
        /// TFS collection to perform action on
        /// </summary>
        [Browsable(true)]
        public InArgument<string> Collection { get; set; }

        /// <summary>
        /// Optional login information, format: username [, password]
        /// </summary>
        [Browsable(true)]
        public InArgument<string> Login { get; set; }

        /// <summary>
        /// Performs the operation recursively. Defaults to true
        /// </summary>
        [Browsable(true)]
        public InArgument<bool> Recursive { get; set; }

        /// <summary>
        /// If true, ignores warning about items that already exists in source control during add. Defaults to false
        /// </summary>
        [Browsable(true)]
        public InArgument<bool> IgnoreItemAlreadyHasPendingChangesWarning { get; set; }

        /// <summary>
        /// Gets the Return Code from TF checkout
        /// </summary>
        public OutArgument<int> ReturnCode { get; set; }

        /// <summary> 
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            TfsAction action = this.Action.Get(this.ActivityContext);
            switch (action)
            {
                case TfsAction.Add:
                    this.Add();
                    break;
                case TfsAction.Checkin:
                    this.Checkin();
                    break;
                case TfsAction.Checkout:
                    this.Checkout();
                    break;
                case TfsAction.Delete:
                    this.Delete();
                    break;
                case TfsAction.Undelete:
                    this.Undelete();
                    break;
                case TfsAction.UndoCheckout:
                    this.UndoCheckout();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private void UndoCheckout()
        {
            this.ExecuteCommand("undo", string.Empty, "/noprompt /recursive");
        }

        private void Undelete()
        {
            this.ExecuteCommand("undelete", string.Empty, "/noprompt /recursive");
        }

        private void Delete()
        {
            this.ExecuteCommand("delete", string.Empty, "/recursive");
        }

        private void Checkout()
        {
            this.ExecuteCommand("checkout", string.Empty, "/noprompt /recursive");
        }

        private void Checkin()
        {
            string comment = string.Empty;
            if (!string.IsNullOrEmpty(this.Comments.Get(this.ActivityContext)))
            {
                comment = string.Format("/comment:\"{0}\"", this.Comments.Get(this.ActivityContext));
            }

            string note = string.Empty;
            if (!string.IsNullOrEmpty(this.Notes.Get(this.ActivityContext)))
            {
                note = string.Format("/notes:\"{0}\"", this.Notes.Get(this.ActivityContext));
            }

            string overrideReason = string.Empty;
            if (string.IsNullOrEmpty(this.OverrideReason.Get(this.ActivityContext)) == false)
            {
                overrideReason = string.Format(" /override:\"{0}\"", this.OverrideReason.Get(this.ActivityContext));
            }

            this.ExecuteCommand("checkin", string.Format("{0} {1} {2}", comment, note, overrideReason), "/noprompt /recursive");
        }

        private void Add()
        {
            this.ExecuteCommand("add", string.Empty, "/noprompt /recursive /noignore");
        }

        private void ExecuteCommand(string action, string options, string lastOptions)
        {
            using (Process proc = new Process())
            {
                string fileName = this.toolPath.Get(this.ActivityContext);
                if (string.IsNullOrEmpty(fileName))
                {
                    string visualStudioTools = Environment.GetEnvironmentVariable("VS100COMNTOOLS");
                    if (visualStudioTools != null)
                    {
                        fileName = Path.Combine(visualStudioTools, @"..\IDE\tf.exe");
                    }
                }

                if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                {
                    this.LogBuildError(
                        string.Format(
                            "TF.exe was not found in the default location. Use ToolPath to specify it. Searched at: {0}",
                            fileName));
                    return;
                }

                proc.StartInfo.FileName = fileName;

                string arguments = string.Format("{0} \"{1}\" {2}", action, this.Itemspec.Get(this.ActivityContext), options);

                if (string.IsNullOrEmpty(this.Collection.Get(this.ActivityContext)) == false)
                {
                    arguments += " /collection:" + this.Collection.Get(this.ActivityContext);
                }

                if (string.IsNullOrEmpty(this.Login.Get(this.ActivityContext)) == false)
                {
                    arguments += " /login:" + this.Login.Get(this.ActivityContext);
                }

                if (!this.Recursive.Get(this.ActivityContext))
                {
                    lastOptions = lastOptions.Replace("/recursive", string.Empty);
                }

                arguments += " " + lastOptions;

                if (this.WorkingDirectory.Expression != null)
                {
                    proc.StartInfo.WorkingDirectory = this.WorkingDirectory.Get(this.ActivityContext);
                }

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = arguments;
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);

                proc.OutputDataReceived += (o, e) =>
                    {
                        var message = e.Data;
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            this.LogBuildMessage(e.Data);
                        }
                    };
                proc.ErrorDataReceived += (o, e) =>
                    {
                        var message = e.Data;
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            if (!IgnoreItemAlreadyHasPendingChangesWarning.Get(this.ActivityContext) || !message.Contains("already has pending changes"))
                            {
                                this.LogBuildWarning(e.Data);
                            }
                        }
                    };

                proc.Start();

                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.WaitForExit();
                this.ReturnCode.Set(this.ActivityContext, proc.ExitCode);
            }
        }
    }
}
