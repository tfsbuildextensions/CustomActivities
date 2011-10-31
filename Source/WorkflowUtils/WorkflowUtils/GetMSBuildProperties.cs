using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.IO;

using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WorkflowUtils
{
    /// <summary>
    /// Activity based on CodeActivity
    /// </summary>
    [BuildActivity (HostEnvironmentOption.All)]
    [BuildExtension (HostEnvironmentOption.All)]
    public sealed class GetMSBuildProperties : CodeActivity<Dictionary<String, String>>
    {
        public InArgument<String> MSBuildArguments { get; set; }

        private string sMSBuildArguments;
        private Dictionary<String, String> dMSbuildProperties;
        
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">WF context</param>
        protected override Dictionary<String, String> Execute(CodeActivityContext context)
        {
            dMSbuildProperties = new Dictionary<String, String>();
            sMSBuildArguments = context.GetValue(this.MSBuildArguments);

            if (sMSBuildArguments != null)
                GetValue();
            context.SetValue(Result, dMSbuildProperties);
            return dMSbuildProperties;
        }

        private void GetValue()
        {
            try
            {
                List<String> rawArgs = new List<string>();

                String argsWithLowerCaseDelimiters = sMSBuildArguments.Replace("/P:", "/p:");

                int indexOfParameterDelimiters = -1;
                while ((indexOfParameterDelimiters = argsWithLowerCaseDelimiters.IndexOf("/p:")) != -1)
                {
                    String argsWithoutFirstDelimiters = argsWithLowerCaseDelimiters.TrimStart('/').TrimStart('p').TrimStart(':');
                    int indexOfNextDelimiterString = argsWithoutFirstDelimiters.IndexOf("/p:");

                    String rawArg = argsWithoutFirstDelimiters;
                    if (indexOfNextDelimiterString == -1)
                    {
                        rawArgs.RemoveAll(a => a.Contains(rawArg.Substring(0, rawArg.IndexOf('='))));
                        rawArgs.Add(rawArg);
                    }
                    else
                    {
                        rawArg = argsWithoutFirstDelimiters.Substring(0, indexOfNextDelimiterString);
                        rawArgs.RemoveAll(a => a.Contains(rawArg.Substring(0,rawArg.IndexOf('='))));
                        rawArgs.Add(rawArg);
                    }

                    argsWithLowerCaseDelimiters = rawArg == argsWithoutFirstDelimiters ? "" : argsWithoutFirstDelimiters.TrimStart(rawArg.ToCharArray()).Trim();
                }

                foreach (String s in rawArgs)
                    dMSbuildProperties.Add(s.Split('=')[0].Trim().Replace("\"",""), s.Split(new Char[] { '=' }, 2)[1].Trim().Replace("\"",""));
            }
            catch (Exception ex)
            {
                throw new Exception("The parameters were passed in a way the activity doesn't accept.  Please use: '/p:Property=Value /p:Property2=Value2 explicit notation (Not /p:Property=Value;Property2=Value2).  The parameters passed are: \"" + sMSBuildArguments + "\"");
            }

            //This code was intended to be for the non-standard short hand processing of msbuild arguments (behaves strangely so this code would be more complicated and pose risks)

            //foreach (String arg in rawArgs)
            //{
            //    String argTrimmed = arg.Trim();

            //    Char[] characterString = argTrimmed.ToArray<Char>();

            //    bool equalsFound = false;
            //    int indexOfEquals = -1;
            //    for (int i = characterString.Length - 1; i >= 0; i--)
            //    {
            //        if (characterString[i] == '=')
            //        {
            //            equalsFound = true;
            //            indexOfEquals = i;
            //        }
            //        if (equalsFound && characterString[i] == ';')
            //        {

            //        }
            //    }
            //}
        }
    }
}
