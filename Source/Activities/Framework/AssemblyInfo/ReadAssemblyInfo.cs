//-----------------------------------------------------------------------
// <copyright file="ReadAssemblyInfo.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Framework
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation.Build.Client;

    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ReadAssemblyInfo : BaseCodeActivity
    {
        #region Fields

        // AssemblyInfo file access helper.
        private AssemblyInfoFile file;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo"/> class.
        /// </summary>
        public ReadAssemblyInfo()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Sets the AssemblyInfo file to read.
        /// </summary>
        /// <remarks>
        /// This property is <b>required.</b>
        /// </remarks>
        [RequiredArgument]
        [Description("Specify the AssemblyInfo file path.")]
        public InArgument<string> File { get; set; }

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        [Description("Specify the assembly version.")]
        public OutArgument<string> AssemblyVersion { get; set; }

        /// <summary>
        /// Gets the assembly file version.
        /// </summary>
        [Description("Specify the assembly file version.")]
        public OutArgument<string> AssemblyFileVersion { get; set; }

        /// <summary>
        /// Gets the assembly informational version.
        /// </summary>
        [Description("Gets the assembly informational version.")]
        public OutArgument<string> AssemblyInformationalVersion { get; set; }

        /// <summary>
        /// Gets the company name.
        /// </summary>
        [Description("Gets the company name.")]
        public OutArgument<string> AssemblyCompany { get; set; }

        /// <summary>
        /// Gets the assembly configuration.
        /// </summary>
        [Description("Gets the assembly configuration.")]
        public OutArgument<string> AssemblyConfiguration { get; set; }

        /// <summary>
        /// Gets the assembly copyright.
        /// </summary>
        [Description("Gets the assembly copyright.")]
        public OutArgument<string> AssemblyCopyright { get; set; }

        /// <summary>
        /// Gets the assembly culture.
        /// </summary>
        [Description("Gets the assembly culture.")]
        public OutArgument<string> AssemblyCulture { get; set; }

        /// <summary>
        /// Specifiec whether the assembly is marked for delay signing.
        /// </summary>
        [Description("Gets whether to delay sign the assembly.")]
        public OutArgument<bool?> AssemblyDelaySign { get; set; }

        /// <summary>
        /// Gets the assembly description.
        /// </summary>
        [Description("Gets the assembly description.")]
        public OutArgument<string> AssemblyDescription { get; set; }

        /// <summary>
        /// Gets the assembly GUID.
        /// </summary>
        [Description("Gets the assembly GUID.")]
        public OutArgument<System.Guid?> Guid { get; set; }

        /// <summary>
        /// Gets the key file to use to sign the assembly.
        /// </summary>
        [Description("Gets the key file to use to sign the assembly.")]
        public OutArgument<string> AssemblyKeyFile { get; set; }

        /// <summary>
        /// Gets the name of a key container within the CSP containing the key pair used to generate a strong name.
        /// </summary>
        [Description("Gets the name of the key conteiner to use to generate a strong name.")]
        public OutArgument<string> AssemblyKeyName { get; set; }

        /// <summary>
        /// Gets the assembly product.
        /// </summary>
        [Description("Gets the assembly product.")]
        public OutArgument<string> AssemblyProduct { get; set; }

        /// <summary>
        /// Gets the assembly title.
        /// </summary>
        [Description("Gets the assembly title.")]
        public OutArgument<string> AssemblyTitle { get; set; }

        /// <summary>
        /// Gets the assembly trademark.
        /// </summary>
        [Description("Gets the assembly trademark.")]
        public OutArgument<string> AssemblyTrademark { get; set; }

        /// <summary>
        /// Specifies whether the assemblyis marked CLS compliant.
        /// </summary>
        [Description("Gets whether the assembly is CLS compliant.")]
        public OutArgument<bool?> CLSCompliant { get; set; }

        /// <summary>
        /// Specifies whether the assembly is marked visible to COM.
        /// </summary>
        [Description("Gets whether the assembly is COM visible.")]
        public OutArgument<bool?> ComVisible { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the logic for this workflow activity.
        /// </summary>
        protected override void InternalExecute()
        {
            // validate file
            var path = this.File.Get(this.ActivityContext);
            if (path == null)
            {
                this.LogBuildWarning("No AssemblyInfo file specified.");

                return;
            }

            // load file
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException("AssemblyInfo file not found.", path);
            }

            this.file = new AssemblyInfoFile(path);

            // read all loaded attribute values
            this.ReadStringAttribute("AssemblyCompany", this.AssemblyCompany);
            this.ReadStringAttribute("AssemblyConfiguration", this.AssemblyConfiguration);
            this.ReadStringAttribute("AssemblyCopyright", this.AssemblyCopyright);
            this.ReadStringAttribute("AssemblyDescription", this.AssemblyDescription);
            this.ReadStringAttribute("AssemblyProduct", this.AssemblyProduct);
            this.ReadStringAttribute("AssemblyTitle", this.AssemblyTitle);
            this.ReadStringAttribute("AssemblyTrademark", this.AssemblyTrademark);
            this.ReadStringAttribute("AssemblyCulture", this.AssemblyCulture);
            this.ReadBoolAttribute("AssemblyDelaySign", this.AssemblyDelaySign);
            this.ReadGuidAttribute("Guid", this.Guid);
            this.ReadStringAttribute("AssemblyKeyFile", this.AssemblyKeyFile);
            this.ReadStringAttribute("AssemblyKeyName", this.AssemblyKeyName);
            this.ReadBoolAttribute("CLSCompliant", this.CLSCompliant);
            this.ReadBoolAttribute("ComVisible", this.ComVisible);
            this.ReadStringAttribute("AssemblyVersion", this.AssemblyVersion);
            this.ReadStringAttribute("AssemblyFileVersion", this.AssemblyFileVersion);
            this.ReadStringAttribute("AssemblyInformationalVersion", this.AssemblyInformationalVersion);
        }

        #endregion

        #region Private Helpers

        // Copies the specified attribute string value to the specified argument if present.
        private void ReadStringAttribute(string attributeName, OutArgument argument)
        {
            var value = this.file[attributeName];

            if (value == null)
            {
                // do nothing
                return;
            }

            argument.Set(this.ActivityContext, value);
        }

        // Copies the specified attribute boolean value to the specified argument if present.
        private void ReadBoolAttribute(string attributeName, OutArgument argument)
        {
            var value = this.file[attributeName];

            if (value == null)
            {
                argument.Set(this.ActivityContext, null);

                return;
            }

            argument.Set(this.ActivityContext, Convert.ToBoolean(value));
        }

        // Copies the specified attribute GUID value to the specified argument if present.
        private void ReadGuidAttribute(string attributeName, OutArgument argument)
        {
            var value = this.file[attributeName];

            if (value == null)
            {
                argument.Set(this.ActivityContext, null);

                return;
            }

            argument.Set(this.ActivityContext, new System.Guid(value));
        }

        #endregion
    }
}
