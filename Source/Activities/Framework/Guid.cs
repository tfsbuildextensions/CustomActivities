//-----------------------------------------------------------------------
// <copyright file="Guid.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Framework
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Security.Cryptography;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// GuidAction
    /// </summary>
    public enum GuidAction
    {
        /// <summary>
        /// Send
        /// </summary>
        Create,

        /// <summary>
        /// CreateCrypto
        /// </summary>
        CreateCrypto
    }

    /// <summary>
    /// <b>Valid Action values are:</b>
    /// <para><i>Create</i> - <b>Output: </b> GuidString, FormattedGuidString</para>
    /// <para><i>CreateCrypto</i> - <b>Output: </b> GuidString, FormattedGuidString</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions Guid Sequence" sap:VirtualizedContainerService.HintSize="633,208">
    /// <Sequence.Variables>
    ///   <Variable x:TypeArguments="x:String" Name="MyGuid" />
    ///   <Variable x:TypeArguments="x:String" Name="MyFormattedGuid" />
    /// </Sequence.Variables>
    /// <taf:Guid FailBuildOnError="{x:Null}" LogExceptionStack="{x:Null}" TreatWarningsAsErrors="{x:Null}" Action="Create" DisplayName="Guid (Create)" FormattedGuidString="[MyFormattedGuid]" GuidString="[MyGuid]" sap:VirtualizedContainerService.HintSize="200,22" />
    /// <mtbwa:WriteBuildMessage DisplayName="Write Guid Result" sap:VirtualizedContainerService.HintSize="200,22" Importance="[Microsoft.TeamFoundation.Build.Client.BuildMessageImportance.High]" Message="[&quot;MyGuid: &quot; + MyGuid + &quot;. MyFormattedGuid: &quot; + MyFormattedGuid]" mva:VisualBasic.Settings="Assembly references and imported namespaces serialized as XML namespaces" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Guid : BaseCodeActivity
    {
        private GuidAction action = GuidAction.Create;

        /// <summary>
        /// Specifies the action to perform. Default is Create.
        /// </summary>
        public GuidAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The Guid result:  32 digits: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /// </summary>
        public OutArgument<string> GuidString { get; set; }

        /// <summary>
        /// The Formatted Guid result: 32 digits separated by hyphens: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        /// </summary>
        public OutArgument<string> FormattedGuidString { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case GuidAction.Create:
                    this.Get();
                    break;
                case GuidAction.CreateCrypto:
                    this.GetCrypto();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        private void Get()
        {
            this.LogBuildMessage("Getting random GUID");
            System.Guid internalGuid = System.Guid.NewGuid();
            this.ActivityContext.SetValue(this.GuidString, internalGuid.ToString("N", CultureInfo.CurrentCulture));
            this.ActivityContext.SetValue(this.FormattedGuidString, internalGuid.ToString("D", CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Gets the crypto.
        /// </summary>
        private void GetCrypto()
        {
            this.LogBuildMessage("Getting Cryptographically Secure GUID");
            byte[] data = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
                System.Guid internalGuid = new System.Guid(data);
                this.ActivityContext.SetValue(this.GuidString, internalGuid.ToString("N", CultureInfo.CurrentCulture));
                this.ActivityContext.SetValue(this.FormattedGuidString, internalGuid.ToString("D", CultureInfo.CurrentCulture));
            }
        }
    }
}