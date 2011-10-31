//-----------------------------------------------------------------------
// <copyright file="Twitter.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Communication
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// <b>Tweet on Twitter</b>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions Twitter Sequence" sap:VirtualizedContainerService.HintSize="818,146">
    /// <tac2:Twitter FailBuildOnError="{x:Null}" IgnoreExceptions="{x:Null}" TreatWarningsAsErrors="{x:Null}" AccessToken="yourAccessToken" AccessTokenSecret="yourAccessTokenSecret" ConsumerKey="yourConsumerKey" ConsumerSecret="yourConsumerSecret" sap:VirtualizedContainerService.HintSize="504,22" LogExceptionStack="True" Message="Hello from your twitter app" TwitterUrl="http://api.twitter.com/1/statuses/update.json" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Twitter : BaseCodeActivity
    {
        private InArgument<string> twitterUrl = "http://api.twitter.com/1/statuses/update.json";

        /// <summary>
        /// Sets the Twitter URL to post to. Defaults to http://api.twitter.com/1/statuses/update.json
        /// </summary>
        public InArgument<string> TwitterUrl
        {
            get { return this.twitterUrl; }
            set { this.twitterUrl = value; }
        }

        /// <summary>
        /// Sets the message to send to Twitter
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// Sets the ConsumerKey
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ConsumerKey { get; set; }

        /// <summary>
        /// Sets the AccessToken (oauth_token)
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AccessToken { get; set; }

        /// <summary>
        /// Sets the ConsumerSecret
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ConsumerSecret { get; set; }

        /// <summary>
        /// Sets the AccessTokenSecret
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AccessTokenSecret { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            string oauth_version = "1.0";
            string oauth_signature_method = "HMAC-SHA1";
            if (this.Message.Get(this.ActivityContext).Length > 140)
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Message too long: {0}. Maximum length is 140 characters.", this.Message.Get(this.ActivityContext).Length));
                return;
            }

            // TODO: figure out encoding to support sending apostrophes
            string userMessage = this.Message.Get(this.ActivityContext).Replace("'", " ");
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Tweeting: {0}", userMessage));
            string postBody = "status=" + Uri.EscapeDataString(userMessage);
            string oauth_consumer_key = this.ConsumerKey.Get(this.ActivityContext);
            string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            string oauth_timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();
            SortedDictionary<string, string> sd = new SortedDictionary<string, string> { { "status", Uri.EscapeDataString(userMessage) }, { "oauth_version", oauth_version }, { "oauth_consumer_key", oauth_consumer_key }, { "oauth_nonce", oauth_nonce }, { "oauth_signature_method", oauth_signature_method }, { "oauth_timestamp", oauth_timestamp }, { "oauth_token", this.AccessToken.Get(this.ActivityContext) } };

            string baseString = string.Empty;
            baseString += "POST" + "&";
            baseString += Uri.EscapeDataString(this.TwitterUrl.Get(this.ActivityContext)) + "&";
            baseString = sd.Aggregate(baseString, (current, entry) => current + Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&"));
            baseString = baseString.Substring(0, baseString.Length - 3);

            string signingKey = Uri.EscapeDataString(this.ConsumerSecret.Get(this.ActivityContext)) + "&" + Uri.EscapeDataString(this.AccessTokenSecret.Get(this.ActivityContext));
            string signatureString;
            using (HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey)))
            {
                signatureString = Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(baseString)));
            }

            ServicePointManager.Expect100Continue = false;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(this.TwitterUrl.Get(this.ActivityContext)));

            StringBuilder authorizationHeaderParams = new StringBuilder();
            authorizationHeaderParams.Append("OAuth ");
            authorizationHeaderParams.Append("oauth_nonce=" + "\"" + Uri.EscapeDataString(oauth_nonce) + "\",");
            authorizationHeaderParams.Append("oauth_signature_method=" + "\"" + Uri.EscapeDataString(oauth_signature_method) + "\",");
            authorizationHeaderParams.Append("oauth_timestamp=" + "\"" + Uri.EscapeDataString(oauth_timestamp) + "\",");
            authorizationHeaderParams.Append("oauth_consumer_key=" + "\"" + Uri.EscapeDataString(oauth_consumer_key) + "\",");
            authorizationHeaderParams.Append("oauth_token=" + "\"" + Uri.EscapeDataString(this.AccessToken.Get(this.ActivityContext)) + "\",");
            authorizationHeaderParams.Append("oauth_signature=" + "\"" + Uri.EscapeDataString(signatureString) + "\",");
            authorizationHeaderParams.Append("oauth_version=" + "\"" + Uri.EscapeDataString(oauth_version) + "\"");
            webRequest.Headers.Add("Authorization", authorizationHeaderParams.ToString());
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            using (Stream stream = webRequest.GetRequestStream())
            {
                byte[] bodyBytes = new ASCIIEncoding().GetBytes(postBody);
                stream.Write(bodyBytes, 0, bodyBytes.Length);
                stream.Flush();
            }

            webRequest.Timeout = 3 * 60 * 1000;
            try
            {
                HttpWebResponse rsp = webRequest.GetResponse() as HttpWebResponse;
                Stream responseStream = rsp.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string content = reader.ReadToEnd();
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Response: {0}", content), BuildMessageImportance.Low);
                }
            }
            catch (Exception e)
            {
                this.LogBuildError(e.ToString());
            }
        }
    }
}