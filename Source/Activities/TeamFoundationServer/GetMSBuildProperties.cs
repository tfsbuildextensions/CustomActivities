//-----------------------------------------------------------------------
// <copyright file="GetMSBuildProperties.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    ///  Returns Dictionary string, string  of key-value pairs of msbuild arguments that were passed in from another process
    ///  Please use: '/p:Property=Value /p:Property2=Value2 explicit notation (Not /p:Property=Value;Property2=Value2 which poses greater risk because of complexity.  There's actually internal msbuild engine issues handling these scenarios also.)
    ///  Note:  GetMSBuildProperties will eventually be replaced by ConvertProperties which converts msbuild, powershell, and ntshell.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetMSBuildProperties : BaseCodeActivity<Dictionary<string, string>>
    {
        private Dictionary<string, string> msbuildProperties;

        /// <summary>
        /// MSBuildArguments
        /// </summary>
        public InArgument<string> MSBuildArguments { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>Dictionary(string, string)</returns>
        protected override Dictionary<string, string> InternalExecute()
        {
            this.msbuildProperties = new Dictionary<string, string>();

            if (this.MSBuildArguments.Expression != null)
            {
                this.GetValue();
            }

            this.ActivityContext.SetValue(Result, this.msbuildProperties);
            return this.msbuildProperties;
        }

        private void GetValue()
        {
            try
            {
                List<string> rawArgs = new List<string>();
                string argsWithLowerCaseDelimiters = this.MSBuildArguments.Get(this.ActivityContext).Replace("/P:", "/p:");

                int indexOfParameterDelimiters = -1;
                while ((indexOfParameterDelimiters = argsWithLowerCaseDelimiters.IndexOf("/p:", StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    string argsWithoutFirstDelimiters = argsWithLowerCaseDelimiters.TrimStart('/').TrimStart('p').TrimStart(':');
                    int indexOfNextDelimiterString = argsWithoutFirstDelimiters.IndexOf("/p:", StringComparison.OrdinalIgnoreCase);

                    string rawArg = argsWithoutFirstDelimiters;
                    if (indexOfNextDelimiterString == -1)
                    {
                        rawArgs.RemoveAll(a => a.Contains(rawArg.Substring(0, rawArg.IndexOf('='))));
                        rawArgs.Add(rawArg);
                    }
                    else
                    {
                        rawArg = argsWithoutFirstDelimiters.Substring(0, indexOfNextDelimiterString);
                        rawArgs.RemoveAll(a => a.Contains(rawArg.Substring(0, rawArg.IndexOf('='))));
                        rawArgs.Add(rawArg);
                    }

                    argsWithLowerCaseDelimiters = rawArg == argsWithoutFirstDelimiters ? string.Empty : Regex.Replace(argsWithLowerCaseDelimiters, @"^[^\s]*\s", string.Empty); 
                }

                foreach (string s in rawArgs)
                {
                    this.msbuildProperties.Add(s.Split('=')[0].Trim().Replace("\"", string.Empty), s.Split(new[] { '=' }, 2)[1].Trim().Replace("\"", string.Empty));
                }
            }
            catch (Exception)
            {
                this.LogBuildError("The parameters were passed in a way the activity doesn't accept.  Please use: '/p:Property=Value /p:Property2=Value2 explicit notation (Not /p:Property=Value;Property2=Value2).  The parameters passed are: \"" + this.MSBuildArguments.Get(this.ActivityContext) + "\"");
                throw new Exception();
            }
        }
    }
}
