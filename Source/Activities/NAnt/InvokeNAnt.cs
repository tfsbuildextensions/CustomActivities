//-----------------------------------------------------------------------
// <copyright file="InvokeNAnt.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//---------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// InvokeNAnt
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public class InvokeNAnt : BaseActivity
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<ExecutionParameters> Parameters { get; set; }

        /// <summary>
        /// Verbose
        /// </summary>
        [Category("Configuration")]
        [DefaultValue(false)]
        public InArgument<bool> Verbose { get; set; }

        /// <summary>
        /// IgnoreExitCode
        /// </summary>
        [Category("Configuration")]
        [DefaultValue(false)]
        public InArgument<bool> IgnoreExitCode { get; set; }

        /// <summary>
        /// NAntDirectory
        /// </summary>
        [Category("Configuration")]
        [RequiredArgument]
        public InArgument<string> NAntDirectory { get; set; }

        /// <summary>
        /// WorkingDirectory
        /// </summary>
        [Category("Configuration")]
        public InArgument<string> WorkingDirectory { get; set; }

        /// <summary>
        /// LogFilePath
        /// </summary>
        [Category("Result")]
        public InArgument<string> LogFilePath { get; set; }

        /// <summary>
        /// Returns the implementation for the derived activity
        /// </summary>
        /// <returns>the code to execute the activity</returns>
        protected override System.Activities.Activity CreateInternalBody()
        {
            var commandLine = new Variable<string>();
            var nantPath = new Variable<string>();
            var parameters = new Variable<ExecutionParameters>();
            var exitCode = new Variable<int>();
            var configError = new Variable<bool>();
            var result = new Sequence
            {
                Variables = { commandLine, parameters, nantPath, exitCode, configError }
            };
            result.Append(new List<Activity>
            {
                new Assign<bool>
                {
                    To = configError,
                    Value = new InArgument<bool>(context => false)
                },
                new Assign<ExecutionParameters>
                {
                     To = parameters,
                     Value = new InArgument<ExecutionParameters>(context => context.GetValue(this.Parameters))
                },
                new If(context => parameters.Get(context) == null)                   
                {
                    Then = new Sequence().Append(new List<Activity>
                    {
                        new Assign<bool>
                        {
                            To = configError,
                            Value = new InArgument<bool>(context => true)
                        },
                        new WriteBuildError
                        {
                            Message = "parameters argument is null"
                        }
                    }),        
                    Else = new Sequence().Append(new List<Activity>
                    {
                        new If(context => !parameters.Get(context).BuildFilePathExists)
                        {
                            Then = new Sequence().Append(new List<Activity>
                            {
                                 new Assign<bool>
                                {
                                    To = configError,
                                    Value = new InArgument<bool>(context => true)
                                },
                                new WriteBuildError
                                {
                                    Message = "Build file does not exist"
                                }
                            })
                        }
                    })                
                },
                new Assign<string>
                {
                    To = nantPath,
                    Value = new InArgument<string>(context => Path.Combine(ProcessNantPath(context.GetValue(this.NAntDirectory)), "Nant.exe"))
                },
                new If(context => !File.Exists(nantPath.Get(context)))
                {
                    Then = new Sequence().Append(new List<Activity>
                    {
                        new Assign<bool>
                        {
                            To = configError,
                            Value = new InArgument<bool>(context => true)
                        },
                        new WriteBuildError
                        {
                            Message = new InArgument<string>(context => string.Format("Nant not found: '{0}'", nantPath.Get(context))),
                        }
                    })
                },
                new Assign<string>
                {
                    To = commandLine,
                    Value = new InArgument<string>(context => 
                            context
                                .GetValue(this.Parameters)
                                .CreateCommandLine(context.GetValue(this.LogFilePath)))
                },      
                new If(context => !configError.Get(context))
                {                         
                    Then = new Sequence().Append(new List<Activity>
                    {
                         new WriteBuildMessage
                        { 
                            Message = new InArgument<string>(context => string.Format("Command line: '{0}'", commandLine.Get(context))),
                            Importance = new InArgument<BuildMessageImportance>(context => BuildMessageImportance.Normal)
                        },
                        new InvokeProcess
                        {
                             Arguments = commandLine,
                             FileName = nantPath,
                             Result = exitCode,
                             WorkingDirectory = this.WorkingDirectory,                       
                        },                                   
                        new If(context => !this.IgnoreExitCode.Get(context) && exitCode.Get(context) != 0)
                        {
                            Then = new Sequence().Append(new List<Activity>
                            {
                                new WriteBuildError
                                {
                                    Message = new InArgument<string>(context => string.Format("Nant execution failed with exitcode '{0}'", exitCode.Get(context))),
                                },
                                new Throw
                                {
                                    Exception = new InArgument<Exception>(context => new NAntException())
                                }
                            })
                        }
                    }),
                    Else = new Throw
                    {
                        Exception = new InArgument<Exception>(context => new NAntException())
                    }                    
                }              
            });
            return result;
        }

        /// <summary>
        /// Replaces %env% placeholders by environment variable values
        /// </summary>
        /// <param name="input">string containing placeholders to replace</param>
        /// <returns>the input script with placeholder removed</returns>
        private static string ProcessNantPath(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return Environment.ExpandEnvironmentVariables(input);
        }
    }
}