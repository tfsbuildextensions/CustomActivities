//-----------------------------------------------------------------------
// <copyright file="ExpandVariables.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Framework
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// Expands variables of the form $(variable) in the specified inputs to their corresponding values.
    /// </summary>
    /// <remarks>
    /// Variables names are case incensitive and user variables specifed using the <see cref="Variables"/> have precedence over environment and build 
    /// variables.
    /// </remarks>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ExpandVariables : BaseCodeActivity<IEnumerable<string>>
    {
        #region Fields

        // variable match regex
        private static readonly Regex VariableRegex = new Regex(@"\$\(([^)]+)\)", RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        #region Properties

        /// <summary>
        /// Sets the input strings to expand.
        /// </summary>
        /// <remarks>
        /// This property is <b>required.</b>
        /// </remarks>
        [RequiredArgument]
        [Description("The input strings to expand.")]
        public InArgument<IEnumerable<string>> Inputs { get; set; }

        /// <summary>
        /// Set to <b>true</b> to add build variables to expand.
        /// </summary>
        /// <remarks>
        /// Setting the value to true will enable the following build variables:
        /// <list type="table">
        /// <listheader>
        /// <item>
        /// <term>Variable</term>
        /// <description>Description</description>
        /// </item>
        /// </listheader>
        /// <item>
        /// <term>$(BuildNumer)</term>
        /// <description>The build number.</description>
        /// </item>
        /// <item>
        /// <term>$(BuildId)</term>
        /// <description>The current build identifier.</description>
        /// </item>
        /// <item>
        /// <term>$(BuildDefinitionName)</term>
        /// <description>The current build definition name.</description>
        /// </item>
        /// <item>
        /// <term>$(BuildDefinitionId)</term>
        /// <description>The current build definition identifier.</description>
        /// </item>
        /// <item>
        /// <term>$(TeamProject)</term>
        /// <description>The current build team project.</description>
        /// </item>
        /// <item>
        /// <term>$(DropLocation)</term>
        /// <description>The current build drop location.</description>
        /// </item>
        /// <item>
        /// <term>$(BuildAgent)</term>
        /// <description>The current build agent name. This variable is only available if the activity is inside a <see cref="AgentScope"/> activity.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [Description("Specify whether build variables are available to expand. Default is false")]
        public InArgument<bool> IncludeBuildVariables { get; set; }

        /// <summary>
        /// Set to <b>true</b> to add environment variables to expand.
        /// </summary>
        [Description("Specify whether environment variables are available to expand. Default is false")]
        public InArgument<bool> IncludeEnvironmentVariables { get; set; }

        /// <summary>
        /// Set variables and their value to expand.
        /// </summary>
        /// <remarks>
        /// User variables have precedence over environment variables and build variables.
        /// </remarks>
        [Description("Variables and their values that you would like to expand.")]
        public InArgument<IDictionary<string, string>> Variables { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>
        /// The activity result.
        /// </returns>
        protected override IEnumerable<string> InternalExecute()
        {
            // get inputs
            var inputs = this.Inputs.Get(this.ActivityContext);
            if (inputs == null || !inputs.Any())
            {
                this.LogBuildWarning("No variable expanded, input strings is null or empty.");

                return inputs;
            }

            // get variables (copy userVariables to put them inside a case insensitive dictionary)
            var userVariables = this.Variables.Get(this.ActivityContext) != null ? new Dictionary<string, string>(this.Variables.Get(this.ActivityContext), StringComparer.OrdinalIgnoreCase) : default(Dictionary<string, string>);

            var buildVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (this.IncludeBuildVariables.Get(this.ActivityContext))
            {
                var buildAgent = this.ActivityContext.GetExtension<IBuildAgent>();
                var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();

                buildVariables["BuildNumber"] = buildDetail.BuildNumber;
                buildVariables["BuildId"] = LinkingUtilities.DecodeUri(buildDetail.Uri.ToString()).ToolSpecificId;
                buildVariables["BuildDefinitionName"] = buildDetail.BuildDefinition.Name;
                buildVariables["BuildDefinitionId"] = buildDetail.BuildDefinition.Id;
                buildVariables["TeamProject"] = buildDetail.BuildDefinition.TeamProject;
                buildVariables["DropLocation"] = buildDetail.DropLocation;
                buildVariables["BuildAgent"] = buildAgent != null ? buildAgent.Name : string.Empty;
                buildVariables["BuildAgentName"] = buildAgent != null ? buildAgent.Name : string.Empty; // Same as BuildAgent but more consistent with TFS naming convention
                buildVariables["BuildAgentId"] = buildAgent != null ? LinkingUtilities.DecodeUri(buildAgent.Uri.AbsoluteUri).ToolSpecificId : string.Empty;

                buildVariables["BuildControllerName"] = buildAgent != null ? buildAgent.Controller.Name : string.Empty;
                buildVariables["BuildControllerId"] = buildAgent != null ? LinkingUtilities.DecodeUri(buildAgent.Controller.Uri.AbsoluteUri).ToolSpecificId : string.Empty;
                buildVariables["BuildControllerCustomAssemblyPath"] = buildAgent != null ? buildAgent.Controller.CustomAssemblyPath : string.Empty;
            }

            var envVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (this.IncludeEnvironmentVariables.Get(this.ActivityContext))
            {
                foreach (var entry in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>())
                {
                    envVariables[entry.Key.ToString()] = entry.Value.ToString();
                }
            }

            // find and replace variables
            var outputs = new List<string>();

            foreach (var input in inputs)
            {
                var output = new StringBuilder(input);

                if (input != null)
                {
                    var matches = VariableRegex.Matches(input);
                    for (var i = matches.Count - 1; i >= 0; --i)
                    {
                        if (matches[i].Success)
                        {
                            var value = default(string);
                            if ((userVariables != null && userVariables.TryGetValue(matches[i].Groups[1].Value, out value)) || buildVariables.TryGetValue(matches[i].Groups[1].Value, out value) || envVariables.TryGetValue(matches[i].Groups[1].Value, out value))
                            {
                                output.Replace(matches[i].Value, value, matches[i].Index, matches[i].Length);

                                this.LogBuildMessage("Expanded variable " + matches[i].Value + " to '" + value + "'.");
                            }
                        }
                    }
                }

                outputs.Add(output.ToString());
            }

            return outputs;
        }

        #endregion
    }
}
