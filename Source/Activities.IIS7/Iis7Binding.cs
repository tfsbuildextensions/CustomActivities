//-----------------------------------------------------------------------
// <copyright file="Iis7Binding.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Web
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.Web.Administration;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// IIS7BindingAction
    /// </summary>
    public enum IIS7BindingAction
    {
        /// <summary>
        /// Add
        /// </summary>
        Add,

        /// <summary>
        /// CheckExists
        /// </summary>
        CheckExists,

        /// <summary>
        /// Modify
        /// </summary>
        Modify,

        /// <summary>
        /// Remove
        /// </summary>
        Remove
    }

    /// <summary>
    /// Iis7Binding
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Iis7Binding : BaseRemoteCodeActivity
    {
        private ServerManager iisServerManager;
        private Site website;
        private string bindingProtocol = "http";
        private IIS7BindingAction action = IIS7BindingAction.Add;

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public IIS7BindingAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Sets the name of the Website
        /// </summary>
        [RequiredArgument]
        public string Name { get; set; }

        /// <summary>
        /// Sets the port of the Binding to Modify
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// String containing binding information.
        /// <para/>
        /// Format: ip address:port:hostheader
        /// <para/>
        /// Example: *:80:sample.example.com or : *:443:
        /// </summary>
        public string BindingInformation { get; set; }

        /// <summary>
        /// Sets the PreviousBindingInformation to use when calling Modify
        /// </summary>
        public string PreviousBindingInformation { get; set; }

        /// <summary>
        /// Sets the PreviousBindingProtocol to use when calling Modify
        /// </summary>
        public string PreviousBindingProtocol { get; set; }

        /// <summary>
        /// If HTTPS is used, this is the certificate hash. This is the value of "thumbprint" value of the certificate you want to use.
        /// <para/>
        /// Format: hash encoded string. Hex symbols can be space or dash separated.
        /// <para/>
        /// Example: 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a 0a
        /// </summary>
        public string CertificateHash { get; set; }

        /// <summary>
        /// The name of the certificate store. Default is "MY" for the personal store
        /// </summary>
        public string CertificateStoreName { get; set; }

        /// <summary>
        /// Gets whether the binding exists
        /// </summary>
        public OutArgument<bool> Exists { get; set; }

        /// <summary>
        /// Binding protocol. Example: "http", "https", "ftp". Default is http.
        /// </summary>
        public string BindingProtocol
        {
            get { return this.bindingProtocol; }
            set { this.bindingProtocol = value; }
        }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            try
            {
                this.iisServerManager = System.Environment.MachineName != this.MachineName.Get(this.ActivityContext) ? ServerManager.OpenRemote(this.MachineName.Get(this.ActivityContext)) : new ServerManager();
                switch (this.Action)
                {
                    case IIS7BindingAction.Add:
                        this.Add();
                        break;
                    case IIS7BindingAction.CheckExists:
                        this.CheckExists();
                        break;
                    case IIS7BindingAction.Modify:
                        this.Modify();
                        break;
                    case IIS7BindingAction.Remove:
                        this.Remove();
                        break;
                    default:
                        throw new ArgumentException("Action not supported");
                }
            }
            finally
            {
                if (this.iisServerManager != null)
                {
                    this.iisServerManager.Dispose();
                }
            }
        }

        /// <summary>
        /// Parse certificate hash from a string.
        /// </summary>
        /// <remarks>Based on code from: http://www.codeproject.com/KB/recipes/hexencoding.aspx</remarks>
        /// <param name="hexValue">hex values, can be space, dash or not-delimited</param>
        /// <returns>byte[] encoded value</returns>
        private static byte[] HexToData(string hexValue)
        {
            if (hexValue == null)
            {
                return null;
            }

            hexValue = hexValue.Replace(" ", string.Empty);
            hexValue = hexValue.Replace("-", string.Empty);
            if (hexValue.Length % 2 == 1)
            {
                // Up to you whether to pad the first or last byte
                hexValue = '0' + hexValue;
            }

            byte[] data = new byte[hexValue.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(hexValue.Substring(i * 2, 2), 16);
            }

            return data;
        }

        private void CheckExists()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            if (string.IsNullOrEmpty(this.BindingInformation))
            {
                this.LogBuildError("BindingInformation is required.");
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Looking for Binding: [{0}] {1} for: {2} on: {3}", this.BindingProtocol, this.BindingInformation, this.Name, this.MachineName));
            if (this.website.Bindings.Any(binding => binding.Protocol.Equals(this.BindingProtocol, StringComparison.OrdinalIgnoreCase) && (binding.BindingInformation == this.BindingInformation)))
            {
                this.Exists.Set(this.ActivityContext, true);
                return;
            }
        }

        private void Remove()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting BindingInformation: [{0}] {1} from {2} on: {3}", this.BindingProtocol, this.BindingInformation, this.Name, this.MachineName));
            foreach (Binding binding in this.website.Bindings.Where(binding => binding.Protocol.Equals(this.BindingProtocol, StringComparison.OrdinalIgnoreCase) && binding.BindingInformation == this.BindingInformation))
            {
                this.website.Bindings.Remove(binding);
                break;
            }

            this.iisServerManager.CommitChanges();
        }

        private void Add()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            if (string.IsNullOrEmpty(this.BindingInformation))
            {
                this.LogBuildError("BindingInformation is required.");
                return;
            }

            if (!string.IsNullOrEmpty(this.CertificateHash))
            {
                if (string.IsNullOrEmpty(this.CertificateStoreName))
                {
                    this.CertificateStoreName = "MY";
                }

                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Creating binding with certificate: thumb print '{0}' in store '{1}'", this.CertificateHash, this.CertificateStoreName));
                this.website.Bindings.Add(this.BindingInformation, HexToData(this.CertificateHash), this.CertificateStoreName);
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding BindingInformation: [{0}] {1} to: {2} on: {3}", this.BindingProtocol, this.BindingInformation, this.Name, this.MachineName));
                if (this.website.Bindings.Any(binding => binding.Protocol.Equals(this.BindingProtocol, StringComparison.OrdinalIgnoreCase) && binding.BindingInformation == this.BindingInformation))
                {
                    this.LogBuildError("A binding with the same ip, port and host header already exists.");
                    return;
                }

                this.website.Bindings.Add(this.BindingInformation, this.BindingProtocol);
            }

            this.iisServerManager.CommitChanges();
        }

        private void Modify()
        {
            if (!this.SiteExists())
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The website: {0} does not exists on: {1}", this.Name, this.MachineName));
                return;
            }

            if (string.IsNullOrEmpty(this.BindingInformation))
            {
                this.LogBuildError("BindingInformation is required.");
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Modifying BindingInformation, setting: {0} for: {1} on: {2}", this.BindingInformation, this.Name, this.MachineName));
            foreach (Binding binding in this.website.Bindings.Where(binding => binding.Protocol.Equals(this.PreviousBindingProtocol, StringComparison.OrdinalIgnoreCase) && binding.BindingInformation == this.PreviousBindingInformation))
            {
                binding.BindingInformation = this.BindingInformation;
                binding.Protocol = this.BindingProtocol;
                break;
            }

            this.iisServerManager.CommitChanges();
        }

        private bool SiteExists()
        {
            this.website = this.iisServerManager.Sites[this.Name];
            return this.website != null;
        }
    }
}