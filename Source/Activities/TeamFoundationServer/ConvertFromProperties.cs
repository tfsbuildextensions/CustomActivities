//-----------------------------------------------------------------------
// <copyright file="ConvertFromProperties.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    ///  Returns Dictionary string, string of key-value pairs of a string of arguments that were passed in from another process
    ///  MSBuild: Please use: '/p:Property=Value /p:Property2=Value2 explicit notation (Not /p:Property=Value;Property2=Value2 which poses greater risk because of complexity.  There's actually internal msbuild engine issues handling these scenarios also.)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class ConvertFromProperties : BaseCodeActivity<string>
    {
        private Dictionary<string, string> properties;
        private string outputProperties = string.Empty;
        private TfsBuildExtensions.Activities.PropertiesType outputType;

        /// <summary>
        /// Properties you wish to convert
        /// </summary>
        public InArgument<Dictionary<string, string>> Properties { get; set; }

        /// <summary>
        /// Type of properties passed in (Accepts: MSBuild, Powershell)
        /// </summary>
        public InArgument<PropertiesType> OutputType { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>String of properties converted to the desired type</returns>
        protected override string InternalExecute()
        {
            this.properties = this.Properties.Get(this.ActivityContext);
            this.outputType = this.OutputType.Get(this.ActivityContext);

            try
            {
                if (this.Properties.Expression != null)
                {
                    switch (this.outputType)
                    {
                        case PropertiesType.MSBuild:
                            this.outputProperties = this.OutputMSBuild();
                            break;
                        case PropertiesType.NTShell:
                            this.outputProperties = this.OutputNtshell();
                            break;
                        case PropertiesType.PowerShell:
                            this.outputProperties = this.OutputPowershell();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new FailingBuildException(e.Message);
            }

            this.ActivityContext.SetValue(this.Result, this.outputProperties);
            return this.outputProperties;
        }

        private string OutputMSBuild()
        {
            string output = this.properties.Aggregate(string.Empty, (current, keyValue) => current + (" /p:" + keyValue.Key + "=" + keyValue.Value));
            return output.Trim();
        }

        private string OutputNtshell()
        {
            string output = this.properties.Aggregate(string.Empty, (current, keyValue) => current + (" " + keyValue.Key + " " + keyValue.Value));
            return output.Trim();
        }
        
        private string OutputPowershell()
        {
            string output = this.properties.Aggregate(string.Empty, (current, keyValue) => current + (" -" + keyValue.Key + " " + keyValue.Value));
            return output.Trim();
        }
    }
}
