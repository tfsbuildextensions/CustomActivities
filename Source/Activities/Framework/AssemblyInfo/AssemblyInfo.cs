//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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

    /// <summary>
    /// Updates content of AssemblyInfo files.
    /// </summary>
    /// <remarks>
    ///     <para>The <see cref="Files"/> property is <b>required</b>.</para>
    ///     <para>Setting a property to null will disable updating the associated attribute.</para>
    ///     <para>
    ///     The following tokens are supported for replacement (all tokens are not supported by every property, see properties remarks for the list of supported tokens):
    ///     <list type="table">
    ///         <listheader>
    ///             <term>Token</term>
    ///             <description>Description</description>
    ///         </listheader>
    ///         <item>
    ///             <term>$(current)</term>
    ///             <description>Uses the current value.</description>
    ///         </item>
    ///         <item>
    ///             <term>$(increment)</term>
    ///             <description>Increments the current value.</description>
    ///         </item>
    ///         <item>
    ///             <term>$(date:&lt;format&gt;)</term>
    ///             <description>Uses the current date formatted with the specified &lt;format&gt;.</description>
    ///         </item>
    ///         <item>
    ///             <term>$(version)</term>
    ///             <description>The updated AssemblyVersion value.</description>
    ///         </item>
    ///         <item>
    ///             <term>$(fileversion)</term>
    ///             <description>The updated AssemblyFileVersion value.</description>
    ///         </item>
    ///     </list>
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="xml"><![CDATA[
    ///     <Sequence DisplayName="TFSBuildExtensions AssemblyInfo Sequence" sap:VirtualizedContainerService.HintSize="818,146">
    ///         <Sequence.Variables>
    ///             <Variable x:TypeArguments="s:String[]" Name="AssemblyInfoFiles" />
    ///         </Sequence.Variables>
    ///         <taf:AssemblyInfo FailBuildOnError="{x:Null}" IgnoreExceptions="{x:Null}" TreatWarningsAsErrors="{x:Null}" AssemblyCompany="{x:Null}" AssemblyConfiguration="{x:Null}" AssemblyCopyright="{x:Null}" AssemblyCulture="{x:Null}" AssemblyDelaySign="{x:Null}" AssemblyDescription="{x:Null}" AssemblyFileVersion="$(current).$(current).$(increment).0" AssemblyInformationalVersion="$(fileversion)" AssemblyKeyFile="{x:Null}" AssemblyKeyName="{x:Null}" AssemblyProduct="{x:Null}" AssemblyTitle="{x:Null}" AssemblyTrademark="{x:Null}" AssemblyVersion="$(current).$(current).0.0" CLSCompliant="[False]" ComVisible="{x:Null}" Files="[AssemblyInfoFiles]" Guid="{x:Null}" LogExceptionStack="True" MaxAssemblyFileVersion="{x:Null}" MaxAssemblyInformationalVersion="{x:Null}" MaxAssemblyVersion="{x:Null}" />
    ///     </Sequence>
    ///     ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class AssemblyInfo : BaseCodeActivity
    {
        #region Fields

        // token parser.
        private static readonly Regex tokenParser = new Regex(
            @"\$\((?<token>[^:\)]*)(:(?<param>[^\)]+))?\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // version parser.
        private static readonly Regex versionParser = new Regex(
            @"\d+\.\d+\.\d+\.\d+",
            RegexOptions.Compiled);

        // AssemblyInfo file access helper.
        private AssemblyInfoFile file;

        // token values.
        private IDictionary<string, Func<string, string>> tokenEvaluators;

        #endregion

        #region Properties

        /// <summary>
        /// Sets the AssemblyInfo files to update.
        /// </summary>
        /// <remarks>
        /// This property is <b>required.</b>
        /// </remarks>
        [RequiredArgument]
        [Description("Specify the AssemblyInfo files path.")]
        public InArgument<IEnumerable<string>> Files { get; set; }

        /// <summary>
        /// Sets the assembly version.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(current)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(increment)</description>
        ///        </item>
        ///        <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///        </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly version. (null to disable update)")]
        public InArgument<string> AssemblyVersion { get; set; }

        /// <summary>
        /// Sets the assembly file version.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///        <item>
        ///             <description>$(current)</description>
        ///        </item>
        ///        <item>
        ///            <description>$(increment)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///        </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly file version. (null to disable update)")]
        public InArgument<string> AssemblyFileVersion { get; set; }

        /// <summary>
        /// Sets the assembly informational version.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///         <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly informational version. (null to disable update)")]
        public InArgument<string> AssemblyInformationalVersion { get; set; }

        /// <summary>
        /// Sets the company name.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the company name. (null to disable update)")]
        public InArgument<string> AssemblyCompany { get; set; }

        /// <summary>
        /// Sets the assembly configuration.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly configuration. (null to disable update)")]
        public InArgument<string> AssemblyConfiguration { get; set; }

        /// <summary>
        /// Sets the assembly copyright.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly copyright. (null to disable update)")]
        public InArgument<string> AssemblyCopyright { get; set; }

        /// <summary>
        /// Sets the assembly culture.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify the assembly culture. (null to disable update)")]
        public InArgument<string> AssemblyCulture { get; set; }

        /// <summary>
        /// Set to <b>true</b> to mark the assembly for delay signing.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify whether to delay sign the assembly. (null to disable update)")]
        public InArgument<bool?> AssemblyDelaySign { get; set; }

        /// <summary>
        /// Sets the assembly description.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly description. (null to disable update)")]
        public InArgument<string> AssemblyDescription { get; set; }

        /// <summary>
        /// Sets the assembly GUID.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify the assembly GUID. (null to disable update)")]
        public InArgument<System.Guid?> Guid { get; set; }

        /// <summary>
        /// Sets the key file to use to sign the assembly.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify the key file to use to sign the assembly. (null to disable update)")]
        public InArgument<string> AssemblyKeyFile { get; set; }

        /// <summary>
        /// Sets the name of a key container within the CSP containing the key pair used to generate a strong name.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify the name of the key conteiner to use to generate a strong name. (null to disable update)")]
        public InArgument<string> AssemblyKeyName { get; set; }

        /// <summary>
        /// Sets the assembly product.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly product. (null to disable update)")]
        public InArgument<string> AssemblyProduct { get; set; }

        /// <summary>
        /// Sets the assembly title.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly title. (null to disable update)")]
        public InArgument<string> AssemblyTitle { get; set; }

        /// <summary>
        /// Sets the assembly trademark.
        /// </summary>
        /// <remarks>
        ///     <para>Setting the value to null will disable updating this attribute.</para>
        ///     <para>
        ///     The following tokens are supported (see <see cref="AssemblyInfo"/> remarks for a description of those tokens):
        ///     <list type="bullet">
        ///         <item>
        ///             <description>$(version)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(fileversion)</description>
        ///         </item>
        ///         <item>
        ///             <description>$(date:&lt;format&gt;)</description>
        ///         </item>
        ///     </list>
        ///     </para>
        /// </remarks>
        [Description("Specify the assembly trademark. (null to disable update)")]
        public InArgument<string> AssemblyTrademark { get; set; }

        /// <summary>
        /// Set to <b>true</b> to mark the assembly CLS compliant.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify whether the assembly is CLS compliant. (null to disable update)")]
        public InArgument<bool?> CLSCompliant { get; set; }

        /// <summary>
        /// Set to <b>true</b> to make the assembly visible to COM.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will disable updating this attribute.
        /// </remarks>
        [Description("Specify whether the assembly is COM visible. (null to disable update)")]
        public InArgument<bool?> ComVisible { get; set; }

        /// <summary>
        /// Gets the max updated assembly version.
        /// </summary>
        [Description("Gets the max computed assembly version.")]
        public OutArgument<Version> MaxAssemblyVersion { get; set; }

        /// <summary>
        /// Gets the max updated assembly file version.
        /// </summary>
        [Description("Gets the max computed assembly file version.")]
        public OutArgument<Version> MaxAssemblyFileVersion { get; set; }

        /// <summary>
        /// Gets the max updated assembly informational version.
        /// </summary>
        [Description("Gets the max computed assembly informational version.")]
        public OutArgument<string> MaxAssemblyInformationalVersion { get; set; }

        /// <summary>
        /// Gets the updated assembly versions.
        /// </summary>
        [Description("Gets the updated assembly versions.")]
        public OutArgument<IEnumerable<Version>> AssemblyVersions { get; set; }

        /// <summary>
        /// Gets the max updated assembly file versions.
        /// </summary>
        [Description("Gets the updated assembly file versions.")]
        public OutArgument<IEnumerable<Version>> AssemblyFileVersions { get; set; }

        /// <summary>
        /// Gets the updated assembly informational versions.
        /// </summary>
        [Description("Gets the updated assembly informational versions.")]
        public OutArgument<IEnumerable<string>> AssemblyInformationalVersions { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the logic for this workflow activity.
        /// </summary>
        protected override void InternalExecute()
        {
            // initialize values
            var now = DateTime.Now;
            var version = string.Empty;
            var fileVersion = string.Empty;
            var versions = new List<Version>();
            var fileVersions = new List<Version>();
            var infoVersions = new List<string>();
            this.tokenEvaluators = new Dictionary<string, Func<string, string>>
            {
                { "current", p => "-1" },
                { "increment", p => "-1" },
                { "date", p => now.ToString(p, System.Globalization.CultureInfo.InvariantCulture) },
                { "version", p => version },
                { "fileversion", p => fileVersion }
            };

            this.MaxAssemblyVersion.Set(this.ActivityContext, new Version(0, 0, 0, 0));
            this.MaxAssemblyFileVersion.Set(this.ActivityContext, new Version(0, 0, 0, 0));
            this.MaxAssemblyInformationalVersion.Set(this.ActivityContext, string.Empty);
            this.AssemblyVersions.Set(this.ActivityContext, new List<Version>());
            this.AssemblyFileVersions.Set(this.ActivityContext, new List<Version>());
            this.AssemblyInformationalVersions.Set(this.ActivityContext, new List<string>());

            // update all files
            var files = this.Files.Get(this.ActivityContext);
            if (files != null && files.Any())
            {
                foreach (var path in files)
                {
                    // load file
                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException("AssemblyInfo file not found.", path);
                    }

                    this.file = new AssemblyInfoFile(path);

                    // update version attributes
                    version = this.UpdateVersion(
                        "AssemblyVersion",
                        this.AssemblyVersion.Get(this.ActivityContext),
                        this.MaxAssemblyVersion);

                    var parsedVersion = default(Version);
                    if (Version.TryParse(version, out parsedVersion))
                    {
                        versions.Add(parsedVersion);
                    }

                    fileVersion = this.UpdateVersion(
                        "AssemblyFileVersion",
                        this.AssemblyFileVersion.Get(this.ActivityContext),
                        this.MaxAssemblyFileVersion);

                    if (Version.TryParse(fileVersion, out parsedVersion))
                    {
                        fileVersions.Add(parsedVersion);
                    }

                    var infoVersion = this.UpdateAttribute("AssemblyInformationalVersion", this.AssemblyInformationalVersion.Get(this.ActivityContext), true);
                    if (string.Compare(infoVersion, this.MaxAssemblyInformationalVersion.Get(this.ActivityContext), StringComparison.Ordinal) > 0)
                    {
                        this.MaxAssemblyInformationalVersion.Set(this.ActivityContext, infoVersion);
                    }

                    infoVersions.Add(infoVersion);

                    // update other attributes
                    this.UpdateAttribute("AssemblyCompany", this.AssemblyCompany.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyConfiguration", this.AssemblyConfiguration.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyCopyright", this.AssemblyCopyright.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyDescription", this.AssemblyDescription.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyProduct", this.AssemblyProduct.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyTitle", this.AssemblyTitle.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyTrademark", this.AssemblyTrademark.Get(this.ActivityContext), true);
                    this.UpdateAttribute("AssemblyCulture", this.AssemblyCulture.Get(this.ActivityContext), false);
                    this.UpdateAttribute("AssemblyDelaySign", this.AssemblyDelaySign.Get(this.ActivityContext).HasValue ? BooleanToString(path, this.AssemblyDelaySign.Get(this.ActivityContext).Value) : null, false);
                    this.UpdateAttribute("Guid", this.Guid.Get(this.ActivityContext).HasValue ? this.Guid.Get(this.ActivityContext).Value.ToString() : null, false);
                    this.UpdateAttribute("AssemblyKeyFile", this.AssemblyKeyFile.Get(this.ActivityContext), false);
                    this.UpdateAttribute("AssemblyKeyName", this.AssemblyKeyName.Get(this.ActivityContext), false);
                    this.UpdateAttribute("CLSCompliant", this.CLSCompliant.Get(this.ActivityContext).HasValue ? BooleanToString(path, this.CLSCompliant.Get(this.ActivityContext).Value) : null, false);
                    this.UpdateAttribute("ComVisible", this.ComVisible.Get(this.ActivityContext).HasValue ? BooleanToString(path, this.ComVisible.Get(this.ActivityContext).Value) : null, false);

                    // write to file (unset and set back ReadOnly attribute if present).
                    var fileAttributes = File.GetAttributes(path);
                    var attributesChanged = false;

                    if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(path, fileAttributes ^ FileAttributes.ReadOnly);
                        attributesChanged = true;
                    }

                    using (var sw = new StreamWriter(path))
                    {
                        this.file.Write(sw);
                    }

                    if (attributesChanged)
                    {
                        File.SetAttributes(path, FileAttributes.ReadOnly);
                    }

                    // log successful update
                    this.LogBuildMessage("AssemblyInfo file '" + path + "' was successfully updated.", BuildMessageImportance.High);
                }

                this.AssemblyVersions.Set(this.ActivityContext, versions);
                this.AssemblyFileVersions.Set(this.ActivityContext, fileVersions);
                this.AssemblyInformationalVersions.Set(this.ActivityContext, infoVersions);
            }
            else
            {
                this.LogBuildWarning("No AssemblyInfo files specified.");
            }
        }

        #endregion

        #region Private Helpers

        // Returns the specified value as a string with the correct case based on the file extension.
        private static string BooleanToString(string path, bool value)
        {
            switch (Path.GetExtension(path))
            {
                case ".cs":
                case ".fs":
                    return value.ToString().ToLower();

                case ".vb":
                    return value.ToString();
            }

            return null;
        }

        // Updates and returns the version of the specified attribute.
        private string UpdateVersion(string attributeName, string format, OutArgument<Version> maxVersion)
        {
            var oldValue = this.file[attributeName];
            if (oldValue == null || string.IsNullOrWhiteSpace(format))
            {
                // do nothing
                return oldValue;
            }

            // parse old version (handle * character)
            bool containsWildcard = oldValue.Contains('*');
            string versionPattern = "{0}.{1}.{2}.{3}";

            if (containsWildcard)
            {
                if (oldValue.Split('.').Length == 3)
                {
                    oldValue = oldValue.Replace("*", "0.0");
                    versionPattern = "{0}.{1}.*";
                }
                else
                {
                    oldValue = oldValue.Replace("*", "0");
                    versionPattern = "{0}.{1}.{2}.*";
                }                
            }

            if (!versionParser.IsMatch(oldValue))
            {
                throw new FormatException("Current value for attribute '" + attributeName + "' is not in a correct version format.");
            }

            var version = new Version(oldValue);

            // update version
            var tokens = format.Split('.');
            if (tokens.Length != 4)
            {
                throw new FormatException("Specified value for attribute '" + attributeName + "'  is not a correct version format.");
            }

            version = new Version(
                Convert.ToInt32(this.ReplaceTokens(tokens[0], version.Major)),
                Convert.ToInt32(this.ReplaceTokens(tokens[1], version.Minor)),
                Convert.ToInt32(this.ReplaceTokens(tokens[2], version.Build)),
                Convert.ToInt32(this.ReplaceTokens(tokens[3], version.Revision)));

            this.file[attributeName] = string.Format(versionPattern, version.Major, version.Minor, version.Build, version.Revision);

            if (version > maxVersion.Get(this.ActivityContext))
            {
                maxVersion.Set(this.ActivityContext, version);
            }

            return version.ToString();
        }

        // Updates and returns the value of the specified attribute.
        private string UpdateAttribute(string attributeName, string attributeValue, bool replaceTokens)
        {
            if (attributeValue == null || this.file[attributeName] == null)
            {
                // do nothing
                return string.Empty;
            }

            this.file[attributeName] = replaceTokens ? this.ReplaceTokens(attributeValue, default(int)) : attributeValue;

            return this.file[attributeName];
        }

        // Expands the specified token.
        private string ReplaceTokens(string value, int current)
        {
            // define replace functions
            this.tokenEvaluators["current"] = p => current.ToString();
            this.tokenEvaluators["increment"] = p => (current + 1).ToString();

            // replace tokens
            return tokenParser.Replace(
                value,
                m =>
                {
                    var evaluator = default(Func<string, string>);
                    if (!this.tokenEvaluators.TryGetValue(m.Groups["token"].Value, out evaluator))
                    {
                        throw new FormatException("Unknown token '" + m.Groups["token"].Value + "'.");
                    }

                    return evaluator(m.Groups["param"].Value);
                });
        }

        #endregion
    }
}
