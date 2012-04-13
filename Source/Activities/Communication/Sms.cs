//-----------------------------------------------------------------------
// <copyright file="Sms.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Communication
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// EmailAction
    /// </summary>
    public enum SmsAction
    {
        /// <summary>
        /// Send
        /// </summary>
        Send
    }

    /// <summary>
    /// <b>Valid Actions are:</b>
    /// <para><i>Send</i> (<b>Required: </b> From, To, Body, AccountSid, AuthToken </para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions Sms Sequence" sap:VirtualizedContainerService.HintSize="818,146">
    ///   <Sequence.Variables>
    ///     <Variable x:TypeArguments="x:String" Default="YOURFROMADDRESS" Name="From" />
    ///     <Variable x:TypeArguments="s:String[]" Default="[New String() {&quot;YOURTOADDRESSES&quot;}]" Name="To" />
    ///     <Variable x:TypeArguments="x:String" Name="variable1" />
    ///   </Sequence.Variables>
    ///   <tac2:Email Attachments="{x:Null}" Body="{x:Null}" FailBuildOnError="{x:Null}" Format="{x:Null}" LogExceptionStack="{x:Null}" Priority="{x:Null}" TreatWarningsAsErrors="{x:Null}" UseDefaultCredentials="{x:Null}" Action="Send" EnableSsl="True" sap:VirtualizedContainerService.HintSize="200,22" MailFrom="[EmailFrom]" MailTo="[EmailTo]" Port="0" SmtpServer="" Subject="YOURSUBJECT" UserName="" UserPassword="" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Sms : BaseCodeActivity
    {
        private SmsAction action = SmsAction.Send;
        private string apiversion = "2010-04-01";

        /// <summary>
        /// Specifies the action to perform. Default is Send.
        /// </summary>
        public SmsAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The phone number to send the SMS from.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> From { get; set; }

        /// <summary>
        /// Sets the Item Collection of phone numbers address to send the SMS to.
        /// </summary>
        [RequiredArgument]
        public InArgument<string[]> To { get; set; }

        /// <summary>
        /// The body of the SMS.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Body { get; set; }

        /// <summary>
        /// Sets the Twilio Account SID
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AccountSid { get; set; }

        /// <summary>
        /// Sets the Twilio AuthToken
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AuthToken { get; set; }

        /// <summary>
        /// Sets the Twilio AuthToken
        /// </summary>
        public InArgument<string> ApiVersion { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            if (!string.IsNullOrEmpty(this.ApiVersion.Get(this.ActivityContext)))
            {
                this.apiversion = this.ApiVersion.Get(this.ActivityContext);
            }

            switch (this.Action)
            {
                case SmsAction.Send:
                    this.Send();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private static void SetBasicAuthHeader(WebRequest req, string userName, string userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
        }

        private void Send()
        {
            string from = this.From.Get(this.ActivityContext);
            string body = this.Body.Get(this.ActivityContext);
            string accountsid = this.AccountSid.Get(this.ActivityContext);
            string authtoken = this.AuthToken.Get(this.ActivityContext);

            Uri uri = new Uri(string.Format("https://api.twilio.com/{0}/Accounts/{1}/SMS/Messages", this.apiversion, accountsid));

            foreach (string recipient in this.To.Get(this.ActivityContext))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Sending Sms to: {0}", recipient));

                string parameters = string.Format(
                        "From={0}&To={1}&Body={2}",
                        from,
                        recipient,
                        body);

                var request = (HttpWebRequest)WebRequest.Create(uri);

                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                SetBasicAuthHeader(request, accountsid, authtoken);

                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
                request.ContentLength = bytes.Length;

                var s = request.GetRequestStream();
                s.Write(bytes, 0, bytes.Length);
                s.Close();

                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
                        {
                            // Check to see if a <RestException> element was returned from Twilio, indicating an error
                            var document = XDocument.Parse(sr.ReadToEnd());
                            var ex = document.Element("TwilioResponse").Element("RestException");
                            if (ex != null)
                            {
                                LogBuildWarning(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        "Failed to send SMS message.  The following response was returned: '{0}: {1}'.",
                                        ex.Element("Status").Value.ToString(),
                                        ex.Element("Message").Value.ToString()));
                            }
                        }
                    }
                }
                catch (WebException wex)
                {
                    LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "Failed to send SMS message.  The following HTTP status code was returned: {0}.", wex.Status.ToString()));
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}