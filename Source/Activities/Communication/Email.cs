//-----------------------------------------------------------------------
// <copyright file="Email.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Communication
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Mail;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// EmailAction
    /// </summary>
    public enum EmailAction
    {
        /// <summary>
        /// Send
        /// </summary>
        Send
    }

    /// <summary>
    /// <b>Valid Actions are:</b>
    /// <para><i>Send</i> (<b>Required: </b> SmtpServer, MailFrom, MailTo, Subject  <b>Optional: </b> Priority, Body, Format, Attachments, UseDefaultCredentials, UserName, UserPassword, Port, EnableSsl)</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions Email Sequence" sap:VirtualizedContainerService.HintSize="818,146">
    ///   <Sequence.Variables>
    ///     <Variable x:TypeArguments="x:String" Default="YOUREMAILFROMADDRESS" Name="EmailFrom" />
    ///     <Variable x:TypeArguments="s:String[]" Default="[New String() {&quot;YOUREMAILTOADDRESSES&quot;}]" Name="EmailTo" />
    ///     <Variable x:TypeArguments="x:String" Name="variable1" />
    ///   </Sequence.Variables>
    ///   <tac2:Email Attachments="{x:Null}" Body="{x:Null}" FailBuildOnError="{x:Null}" Format="{x:Null}" LogExceptionStack="{x:Null}" Priority="{x:Null}" TreatWarningsAsErrors="{x:Null}" UseDefaultCredentials="{x:Null}" Action="Send" EnableSsl="True" sap:VirtualizedContainerService.HintSize="200,22" MailFrom="[EmailFrom]" MailTo="[EmailTo]" Port="0" SmtpServer="" Subject="YOURSUBJECT" UserName="" UserPassword="" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Email : BaseCodeActivity
    {
        private EmailAction action = EmailAction.Send;

        /// <summary>
        /// Specifies the action to perform. Default is Send.
        /// </summary>
        public EmailAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The SMTP server to use to send the email.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> SmtpServer { get; set; }

        /// <summary>
        /// Sets the port to use. Ignored if not specified.
        /// </summary>
        public InArgument<int> Port { get; set; }

        /// <summary>
        /// Sets whether to EnableSsl
        /// </summary>
        public InArgument<bool> EnableSsl { get; set; }

        /// <summary>
        /// The email address to send the email from.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> MailFrom { get; set; }

        /// <summary>
        /// Sets the Item Collection of email address to send the email to.
        /// </summary>
        [RequiredArgument]
        public InArgument<string[]> MailTo { get; set; }

        /// <summary>
        /// The subject of the email.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Subject { get; set; }

        /// <summary>
        /// The priority of the email. Default is Normal
        /// </summary>
        public InArgument<string> Priority { get; set; }

        /// <summary>
        /// The body of the email.
        /// </summary>
        public InArgument<string> Body { get; set; }

        /// <summary>
        /// Sets the format of the email. Default is HTML
        /// </summary>
        public InArgument<string> Format { get; set; }

        /// <summary>
        /// Sets the UserName if SmtpClient requires credentials
        /// </summary>
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// Sets the UserPassword if SmtpClient requires credentials
        /// </summary>
        public InArgument<string> UserPassword { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that controls whether the DefaultCredentials are sent with requests. DefaultCredentials represents the system credentials for the current security context in which the application is running. Default is true.
        /// <para>If UserName and UserPassword is supplied, this is set to false. If UserName and UserPassword are not supplied and this is set to false then mail is sent to the server anonymously.</para>
        /// <para><b>If you provide credentials for basic authentication, they are sent to the server in clear text. This can present a security issue because your credentials can be seen, and then used by others.</b></para>
        /// </summary>
        public InArgument<bool> UseDefaultCredentials { get; set; }

        /// <summary>
        /// An Item Collection of full paths of files to attach to the email.
        /// </summary>
        public InArgument<IEnumerable<string>> Attachments { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            // Initialise defaults
            if (this.UseDefaultCredentials.Expression == null)
            {
                this.UseDefaultCredentials.Set(this.ActivityContext, true);
            }

            if (this.Format.Expression == null)
            {
                this.Format.Set(this.ActivityContext, "HTML");
            }

            if (this.Priority.Expression == null)
            {
                this.Priority.Set(this.ActivityContext, "Normal");
            }

            switch (this.Action)
            {
                case EmailAction.Send:
                    this.Send();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }
        
        private void Send()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Sending email: {0}", this.Subject.Get(this.ActivityContext)));
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(this.MailFrom.Get(this.ActivityContext));
                foreach (string recipient in this.MailTo.Get(this.ActivityContext))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding recipient: {0}", recipient), BuildMessageImportance.Low);
                    msg.To.Add(new MailAddress(recipient));
                }

                if (this.Attachments.Get(this.ActivityContext) != null)
                {
                    foreach (string file in this.Attachments.Get(this.ActivityContext))
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding attachment: {0}", file), BuildMessageImportance.Low);
                        Attachment attachment = new Attachment(file);
                        msg.Attachments.Add(attachment);
                    }
                }

                msg.Subject = this.Subject.Get(this.ActivityContext);
                msg.Body = this.Body.Get(this.ActivityContext) ?? string.Empty;
                if (this.Format.Get(this.ActivityContext).ToUpperInvariant() == "HTML")
                {
                    msg.IsBodyHtml = true;
                }

                using (SmtpClient client = new SmtpClient(this.SmtpServer.Get(this.ActivityContext)))
                {
                    if (Convert.ToInt32(this.Port.Get(this.ActivityContext)) > 0)
                    {
                        client.Port = Convert.ToInt32(this.Port.Get(this.ActivityContext));
                    }

                    client.EnableSsl = Convert.ToBoolean(this.EnableSsl.Get(this.ActivityContext));
                    client.UseDefaultCredentials = this.UseDefaultCredentials.Get(this.ActivityContext);
                    if (!string.IsNullOrEmpty(this.UserName.Get(this.ActivityContext)))
                    {
                        client.Credentials = new System.Net.NetworkCredential(this.UserName.Get(this.ActivityContext), this.UserPassword.Get(this.ActivityContext));
                    }

                    client.Send(msg);
                }
            }
        }
    }
}