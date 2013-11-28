//-----------------------------------------------------------------------
// <copyright file="ConvertToProperties.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    ///  Returns Dictionary string, string of key-value pairs of a string of arguments that were passed in from another process
    ///  MSBuild: Please use: '/p:Property=Value /p:Property2=Value2 explicit notation (Not /p:Property=Value;Property2=Value2 which poses greater risk because of complexity.  There's actually internal msbuild engine issues handling these scenarios also.)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ConvertToProperties : BaseCodeActivity<Dictionary<string, string>>
    {
        private Dictionary<string, string> properties;
        private PropertiesType inputType;
        private string inputProperties;

        /// <summary>
        /// Properties you wish to convert
        /// </summary>
        public InArgument<string> Properties { get; set; }

        /// <summary>
        /// Type of properties passed in (Accepts: MSBuild, Powershell)
        /// </summary>
        public InArgument<PropertiesType> InputType { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>String of properties converted to the desired type</returns>
        protected override Dictionary<string, string> InternalExecute()
        {
            this.properties = new Dictionary<string, string>();
            this.inputProperties = this.Properties.Get(this.ActivityContext);
            this.inputType = this.InputType.Get(this.ActivityContext);

            if (this.Properties.Expression != null)
            {
                switch (this.inputType)
                {
                    case PropertiesType.MSBuild:
                        this.InputMSBuild();
                        break;
                    case PropertiesType.NTShell:
                        this.InputNtshell();
                        break;
                    case PropertiesType.PowerShell:
                        this.InputPowershell();
                        break;
                }
            }

            this.ActivityContext.SetValue(this.Result, this.properties);
            return this.properties;
        }

        private void InputMSBuild()
        {
            try
            {
                const string Pattern = "/[pP]:[\\w]+=(\"[^\"]*\"|[^\"\\s]*)";
                MatchCollection matches = Regex.Matches(this.inputProperties, Pattern);
                foreach (Match match in matches)
                {
                    string matchWithoutPrefix = match.Value.Remove(0, 3);
                    string[] pair = matchWithoutPrefix.Split(new[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (this.properties.ContainsKey(pair[0]))
                    {
                        this.properties.Remove(pair[0]);
                    }

                    this.properties.Add(pair[0].Trim().Replace("\"", string.Empty), pair[1].Trim().Replace("\"", string.Empty));
                }
            }
            catch
            {
                this.LogBuildError("The parameters could not be processed.");
                this.LogBuildError("Passed parameters were: " + this.inputProperties);
                this.LogBuildError("For MSBuild: Please use: '/p:Property1=Value1 /p:Property2=Value2 explicit notation (Not /p:Property1=Value1;Property2=Value2).");
                throw;
            }
        }

        private void InputNtshell()
        {
            try
            {
                const string Pattern = "(\"[^\"]*\"|[^\"\\s]+)(\\s+|$)";
                MatchCollection matches = Regex.Matches(this.inputProperties, Pattern);

                if (matches.Count % 2 != 0)
                {
                    throw new FailingBuildException("There should be an even number of parameters to represent key/value pairs.  Current number of arguments: " + matches.Count + "  Example:  Key1 Value1 Key2 Value2");
                }

                for (int i = 0; i < matches.Count; i = i + 2)
                {
                    if (Regex.IsMatch(matches[i].Value.Trim(), "\\W"))
                    {
                        throw new FailingBuildException("Key names should be alphanumeric strings without quotes.  Invalid key name detected: " + matches[i].Value.Trim());
                    }

                    this.properties.Add(matches[i].Value.Trim(), matches[i + 1].Value.Trim());
                }
            }
            catch (FailingBuildException ke)
            {
                Console.WriteLine(ke.Message);
            }
            catch
            {
                this.LogBuildError("The parameters could not be processed.");
                this.LogBuildError("Passed parameters were: " + this.inputProperties);
                this.LogBuildError("For Batchshell: Please use: 'Property1 Value1 Property2 Value2'");
                throw;
            }
        }

        private void InputPowershell()
        {
            try
            {
                const string Pattern = "(\"[^\"]*\"|[^\"\\s]+)(\\s+|$)";

                MatchCollection matches = Regex.Matches(this.inputProperties, Pattern);

                if (matches.Count % 2 != 0)
                {
                    throw new FailingBuildException("There should be an even number of parameters to represent key/value pairs.  Current number of arguments: " + matches.Count + "  Example:  -Key1 Value1 -Key2 Value2");
                }

                for (int i = 0; i < matches.Count; i = i + 2)
                {
                    if (!Regex.IsMatch(matches[i].Value.Trim(), "^-[A-Za-z][\\w]*$") || this.properties.ContainsKey(matches[i].Value.Trim()))
                    {
                        throw new FailingBuildException("Key names should be alphanumeric strings without quotes, preceded by a dash, and with the first character not a number or special character.  Also, the key name cannot be duplicated.  [Example:  -Key1 Value1 -Key2 Value2].  Invalid key name detected: " + matches[i].Value.Trim());
                    }

                    this.properties.Add(matches[i].Value.Trim().TrimStart(new[] { '-' }), matches[i + 1].Value.Trim());
                }

                for (int i = 0; i < matches.Count; i = i + 2)
                {
                    if (this.properties.ContainsKey(matches[i + 1].Value.Trim()))
                    {
                        throw new FailingBuildException("The key name cannot be duplicated.  [Example:  -Key1 Value1 -Key2 Value2].  Key name duplicated: " + matches[i + 1].Value.Trim());
                    }
                }
            }
            catch (FailingBuildException ke)
            {
                Console.WriteLine(ke.Message);
            }
            catch
            {
                this.LogBuildError("The parameters could not be processed.");
                this.LogBuildError("Passed parameters were: " + this.inputProperties);
                this.LogBuildError("For Powershell: Please use: '-Property1 Value1 -Property2 Value2'");
                throw;
            }
        }
    }
}
