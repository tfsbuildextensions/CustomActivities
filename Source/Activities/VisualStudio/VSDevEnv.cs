//-----------------------------------------------------------------------
// <copyright file="VSDevEnv.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using TfsBuildExtensions.Activities.Internal;

    /// <summary>
    /// Activity for building Visual Studio solutions using devenv. This allows for building or deploying
    /// solutions/projects that can't be built using msbuild (C++ (pre VS2010), BTS 2004, 2006 and 2006R, 
    /// deploy reporting services projects,etc)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public sealed class VSDevEnv : BaseActivity
    {
        private VSDevEnvAction action = VSDevEnvAction.Build;
        private VSVersion version = VSVersion.Auto;
        private BuildMessageImportance loggingOutputImportance = BuildMessageImportance.Normal;

        /// <summary>
        /// The Path to the solution or Project to build
        /// </summary>
        [RequiredArgument]
        [Description("The complete path of the solution or project file to be built.")]
        public InArgument<string> FilePath { get; set; }

        /// <summary>
        /// The Configuration to Build.
        /// <example>
        /// <list type="">
        /// <item>Debug</item>
        /// <item>Release</item>
        /// </list>
        /// </example>
        /// </summary>
        [RequiredArgument]
        [Description("Configuration to be built (eg: Debug , Release,etc).")]
        public InArgument<string> Configuration { get; set; }

        /// <summary>
        /// The Platform to Build.
        /// </summary>
        [RequiredArgument]
        [Description("Configuration Platform to be built (eg: X86, Any CPU,etc).")]
        public InArgument<string> Platform { get; set; }

        /// <summary>
        /// Specifies the File to log all output to.
        /// <para></para>
        /// If ommited the file will be automatically placed on the logs path of DropsFolder\logs
        /// </summary>
        [Description("The path for the log file. You should only use these if you want to control the log file for a particular reason. Otherwise the system will take care of the log file details for you")]
        public InArgument<string> OutputFile { get; set; }

        /// <summary>
        /// The action to perform with devenv (build, rebuild, clean,...)
        /// </summary>
        [RequiredArgument]
        [Description("The action to be performed by dev (eg: Build, Clean,etc).")]
        public VSDevEnvAction Action
        {
            get 
            { 
                return this.action; 
            }

            set
            {
                this.action = value; 
            }
        }

        /// <summary>
        /// The Visual Studio's devenv version to use to build the FileName.
        /// <para></para>
        /// If Auto is specified the system will determine the most appropriate
        /// version to use based on the file version.
        /// </summary>
        [RequiredArgument]
        [Description("DevEnv version to be used. Set to Auto if you want the appropriate version to be used.")]
        public VSVersion Version
        {
            get
            {
                return this.version;
            }

            set
            {
                this.version = value;
            }
        }

        /// <summary>
        /// Logging level for the placing the standard output content directly on the build log
        /// </summary>
        [Description("Mininum log level to used to write dev env standard output to the build log. This can lead to a lot of noise in the build log.")]
        public BuildMessageImportance OutputLoggingLevel 
        {
            get
            {
                return this.loggingOutputImportance;
            }

            set
            {
                this.loggingOutputImportance = value;
            }
        }

        /// <summary>
        /// Were there any errors while executing devenv? (or even before that)
        /// </summary>
        [Description("Has devenv finished with errors? (or the execution failed even before that)")]
        public OutArgument<bool> HasErrors { get; set; }

        /// <summary>
        /// The error code returned by devenv
        /// </summary>
        [Description("Devenv error level")]
        public OutArgument<int> ErrorCode { get; set; }

        /// <summary>
        /// Creates the code for executing dev env activity
        /// </summary>
        /// <returns>The code of the activity to be executed</returns>
        protected override Activity CreateInternalBody()
        {
            var visualStudioVersionVariable = new Variable<VSVersionInternal> { Name = "VSVersion" };

            return new Sequence
            {
                Variables =
                {
                    visualStudioVersionVariable
                },

                Activities =
                {
                    this.CreateValidationParametersBody(),
                    
                    // Are the parameters valid? then proceed.
                    new @If
                    {
                        DisplayName = "If? Parameters are valid",
                        
                        Condition = new InArgument<bool>(env => this.HasErrors.GetLocation(env).Value),
                    
                        Else = new Sequence 
                        {
                            // Get VS DevEnv version to use
                            Activities = 
                            {
                                new Assign<VSVersionInternal>
                                {
                                    DisplayName = "AssignVSVersion",
                                    To = new OutArgument<VSVersionInternal>(visualStudioVersionVariable),
                                    Value = new InArgument<VSVersionInternal>(env => GetVersion(this.FilePath.Get(env), this.version))
                                },

                                // If VNext or previous specified issue an error. Otherwise build with devenv appropriate version
                                new @If
                                {
                                    DisplayName = "If? VNext",

                                    Condition = new InArgument<bool>(env => visualStudioVersionVariable.Get(env) == VSVersionInternal.VSNext || visualStudioVersionVariable.Get(env) == VSVersionInternal.Previous),
                                    
                                    Then = this.GetOperationFailedBody("Visual Studio version not supported"),

                                    Else = this.GetInvokeDevEnvBody(visualStudioVersionVariable)
                                }
                            }
                        }                
                    }
                }
            };
        }

        /// <summary>
        /// Gets the command line action string to be executed by devenv
        /// </summary>
        /// <param name="action">The action to be executed by devenv</param>
        /// <returns>The command line option for the corresponding action</returns>
        private static string GetActionCommandLineOption(VSDevEnvAction action)
        {
            switch (action)
            {
                case VSDevEnvAction.Build: return "/Build";
                case VSDevEnvAction.Rebuild: return "/Rebuild";
                case VSDevEnvAction.Clean: return "/Clean";
                case VSDevEnvAction.Deploy: return "/Deploy";

                default: throw new ArgumentException("Unknown action " + action);
            }
        }

        /// <summary>
        /// Removes invalid character from a file name.
        /// </summary>
        /// <param name="fileName">The FileName to sanitize</param>
        /// <returns>sanitized FileName without invalid characters</returns>
        private static string SanitizeFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, invalidChar) => current.Replace(invalidChar.ToString(), string.Empty));
        }

        /// <summary>
        /// Gets the Visual Studio version to be used.
        /// <para>
        /// If the user has specified the version than that version will be used.
        /// </para>
        /// If the user has specified Auto then we will determine VS's appropriate
        /// version based on the version of the file (.sln) we are going to build
        /// </summary>
        /// <param name="fileName">The name of the solution file to get the version (full path). Only used if <value>TfsBuildExtensions.Activities.VisualStudio.VSVersion.Auto</value> is specified</param>
        /// <param name="version">The version</param>
        /// <returns>The devenv version that will be used</returns>
        private static VSVersionInternal GetVersion(string fileName, VSVersion version)
        {
            if (version != VSVersion.Auto)
            {
                return (VSVersionInternal)(int)version;
            }

            var solution = new VSSolution(fileName);
            return solution.FriendlyVersion;
        }

        /// <summary>
        /// Gets the code for the activity that validates the main activity parameters
        /// </summary>
        /// <returns>The activity that validates the parameters</returns>
        private ValidateActivityInternal CreateValidationParametersBody()
        {
            return new ValidateActivityInternal
            {
                DisplayName = "Valida Parameters",

                FilePath = new InArgument<string>(env => this.FilePath.Get(env)),
                Version = new InArgument<VSVersion>(this.version),
                HasErrors = new OutArgument<bool>(env => this.HasErrors.GetLocation(env).Value),

                IgnoreExceptions = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),
                FailBuildOnError = new InArgument<bool>(env => this.FailBuildOnError.Get(env)),
                LogExceptionStack = new InArgument<bool>(env => this.LogExceptionStack.Get(env)),
                TreatWarningsAsErrors = new InArgument<bool>(env => this.TreatWarningsAsErrors.Get(env))
            };
        }

        /// <summary>
        /// Gets the activity that sets the HasErrors to true and logs a build error
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <returns>The activity</returns>
        private Activity GetOperationFailedBody(string message)
        {
            return new Sequence
            {
                Activities =
                {
                    new Assign<bool>
                    {
                        To = new OutArgument<bool>(env => this.HasErrors.GetLocation(env).Value),
                        Value = new InArgument<bool>(true)
                    },
                    new LogBuildError
                    {
                        Message = message,
                        FailBuildOnError = new InArgument<bool>(env => this.FailBuildOnError.Get(env)),
                        LogExceptionStack = new InArgument<bool>(env => this.LogExceptionStack.Get(env))
                    }
                }
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "further simplification would have marginal returns on reability")]
        private Activity GetInvokeDevEnvBody(Variable<VSVersionInternal> visualStudioVersionVariable)
        {
            var devEnvPathVariable = new Variable<string> { Name = "VS DevEnv Path" };
            var logFileLocationVariable = new Variable<string> { Name = "Log File Location" };
            var devEnvCommandPathVariable = new Variable<string> { Name = "DevEnv Command Path" };
            var devEnvArgumentsVariable = new Variable<string> { Name = "DevEnv Arguments" };

            var errorHandlerArgument = new DelegateInArgument<string>();
            var stdOutputHandlerArgument = new DelegateInArgument<string>();

            // Invoke DevEnv after determing VS version
            return new Sequence
            {
                Variables = { devEnvPathVariable },
                Activities =
                {
                    new Assign<string>
                    {
                        To = new OutArgument<string>(devEnvPathVariable),
                        Value = new InArgument<string>(env => VSHelper.GetVisualStudioInstallationDir(visualStudioVersionVariable.Get(env)))
                    },

                    new @If
                    {
                        Condition = new InArgument<bool>(env => devEnvPathVariable.Get(env) == null),

                        Then = this.GetOperationFailedBody("DevEnv Visual Studio not installed."),

                        Else = new Sequence
                        {
                            Variables =
                            {
                                logFileLocationVariable, devEnvCommandPathVariable, devEnvArgumentsVariable
                            },
                            Activities =
                            {
                                this.GetDevEnvLogFileBody(logFileLocationVariable),
                                this.GetDevEnvCallingParametersBody(devEnvPathVariable, logFileLocationVariable, devEnvCommandPathVariable, devEnvArgumentsVariable),

                                new LogBuildMessage
                                {
                                    Message = new InArgument<string>(env => string.Format("Calling {0} {1}", devEnvCommandPathVariable.Get(env), devEnvArgumentsVariable.Get(env)))
                                },

                                new InvokeProcess
                                {
                                    DisplayName = "Invoke DevEnv",
                                    FileName = new InArgument<string>(devEnvCommandPathVariable),
                                    Arguments = new InArgument<string>(devEnvArgumentsVariable),
                                    Result = new OutArgument<int>(env => this.ErrorCode.GetLocation(env).Value),

                                    OutputDataReceived = new ActivityAction<string>
                                    {
                                        Argument = stdOutputHandlerArgument,
                                        Handler = new LogBuildMessage
                                        {
                                            Message = stdOutputHandlerArgument,
                                            Importance = new InArgument<BuildMessageImportance>(this.OutputLoggingLevel)
                                        }
                                    },
                                    ErrorDataReceived = new ActivityAction<string>
                                    {
                                        Argument = errorHandlerArgument,
                                        Handler = new LogBuildError
                                        {
                                            Message = errorHandlerArgument,
                                            FailBuildOnError = new InArgument<bool>(false)
                                        }
                                    }
                                },

                                // IF output log file exists then log it
                                new @If
                                {
                                    Condition = new InArgument<bool>(env => File.Exists(logFileLocationVariable.Get(env))),

                                    Then = new AddLinkToLogFile
                                    {
                                        FileName = new InArgument<string>(env => logFileLocationVariable.Get(env))
                                    }
                                },
                                new @If
                                {
                                    Condition = new InArgument<bool>(env => this.ErrorCode.GetLocation(env).Value == 0),
                                    Else = this.GetOperationFailedBody("Calling DevEnv failed.")                                    
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Gets the code for the activty that is responsable to determine the log file file path
        /// </summary>
        /// <param name="logFileLocationVariable">The variable where the complete log file path will be stored</param>
        /// <returns>The activity</returns>
        private GetDevEnvLogFile GetDevEnvLogFileBody(Variable<string> logFileLocationVariable)
        {
            return new GetDevEnvLogFile
            {
                FilePath = new InArgument<string>(env => this.FilePath.Get(env)),
                Configuration = new InArgument<string>(env => this.Configuration.Get(env)),
                Platform = new InArgument<string>(env => this.Platform.Get(env)),
                OutputFile = new InArgument<string>(env => this.OutputFile.Get(env)),
                LogFile = new OutArgument<string>(logFileLocationVariable)
            };
        }

        /// <summary>
        /// Gets the code for the activity that is responsable for getting the location and executable name
        /// of the devenv version we need
        /// </summary>
        /// <param name="devEnvPathVariable">the variable that holds the dev env path</param>
        /// <param name="logFileLocationVariable">the variable that holds the complete path of the log filePath</param>
        /// <param name="devEnvCommandPathVariable">the variable that holds the path+exec name to be executed</param>
        /// <param name="devEnvArgumentsVariable">the arguments that holds the arguments to be passed to devenv</param>
        /// <returns>the activity</returns>
        private GetDevEnvCallingParameters GetDevEnvCallingParametersBody(Variable<string> devEnvPathVariable, Variable<string> logFileLocationVariable, Variable<string> devEnvCommandPathVariable, Variable<string> devEnvArgumentsVariable)
        {
            return new GetDevEnvCallingParameters
            {
                DevEnvPath = new InArgument<string>(env => devEnvPathVariable.Get(env)),

                FilePath = new InArgument<string>(env => this.FilePath.Get(env)),
                Configuration = new InArgument<string>(env => this.Configuration.Get(env)),
                Platform = new InArgument<string>(env => this.Platform.Get(env)),
                Action = new InArgument<VSDevEnvAction>(this.Action),
                OutputFile = new InArgument<string>(logFileLocationVariable),

                DevEnvCommandPath = new OutArgument<string>(devEnvCommandPathVariable),
                Arguments = new OutArgument<string>(devEnvArgumentsVariable)
            };
        }

        /// <summary>
        /// Validates the parameters of the main activity.
        /// The filename has to be mandatory and has to exist on disk.
        /// If the user has selection auto selection of devenv version than it verifies
        /// if the version type can be auto detected for the specified file type (only
        /// solution files support auto detection)
        /// <para></para>
        /// If the parameters are invalid logs a build error message
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
        private class ValidateActivityInternal : BaseCodeActivity
        {
            /// <summary>
            /// The Path to the solution or Project to build
            /// </summary>
            [RequiredArgument]
            [Description("The complete path of the solution or project file to be built.")]
            public InArgument<string> FilePath { get; set; }

            /// <summary>
            /// The Visual Studio's devenv version to use to build the FileName.
            /// <para></para>
            /// If Auto is specified the system will determine the most appropriate
            /// version to use based on the file version.
            /// </summary>
            [RequiredArgument]
            [Description("DevEnv version to be used. Set to Auto if you want the appropriate version to be used.")]
            [Editor("Microsoft.TeamFoundation.Build.Controls.EnumPropertyEditor, Microsoft.TeamFoundation.Build.Controls", typeof(System.Drawing.Design.UITypeEditor))]
            public InArgument<VSVersion> Version { get; set; }

            /// <summary>
            /// Predicate that indicates if there were errors found while validating.
            /// <para></para>
            /// If there are errors they are logged as errors.
            /// </summary>
            public OutArgument<bool> HasErrors { get; set; }

            /// <summary>
            /// Validates the main activity parameters
            /// </summary>
            protected override void InternalExecute()
            {
                var filePath = this.FilePath.Get(this.ActivityContext);
                var version = this.Version.Get(this.ActivityContext);

                if (string.IsNullOrWhiteSpace(filePath) || File.Exists(filePath) == false)
                {
                    this.LogBuildError(string.Format("File {0} not found.", filePath));

                    this.HasErrors.Set(this.ActivityContext, true);

                    return;
                }

                if (version == VSVersion.Auto && VSSolution.CanAutoDetectVersion(filePath) == false)
                {
                    this.LogBuildError("Can only auto detect DevEnv version for solution files. For project files you need to explicitily set the version");

                    this.HasErrors.Set(this.ActivityContext, true);

                    return;
                }
            }
        }

        /// <summary>
        /// Gets the parameters to call devenv (the exec name + arguments)
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class GetDevEnvCallingParameters : CodeActivity
        {
            /// <summary>
            /// The path (only the directory) where devenv is stored.
            /// <example>c:\program files(x86)\Microsoft Visual Studio 10\common7\ide</example>
            /// </summary>
            [RequiredArgument]
            public InArgument<string> DevEnvPath { get; set; }

            /// <summary>
            /// The complete path (directory + executable name) to be executed.
            /// </summary>
            public OutArgument<string> DevEnvCommandPath { get; set; }

            /// <summary>
            /// The arguments to be passed to devenv
            /// </summary>
            public OutArgument<string> Arguments { get; set; }

            /// <summary>
            /// The Path to the solution or Project to build
            /// </summary>
            [RequiredArgument]
            [Description("The complete path of the solution or project file to be built.")]
            public InArgument<string> FilePath { get; set; }

            /// <summary>
            /// The Configuration to Build.
            /// <example>
            /// <list type="">
            /// <item>Debug</item>
            /// <item>Release</item>
            /// </list>
            /// </example>
            /// </summary>
            [RequiredArgument]
            [Description("Configuration to be built (eg: Debug , Release,etc).")]
            public InArgument<string> Configuration { get; set; }

            /// <summary>
            /// The Platform to Build.
            /// </summary>
            [RequiredArgument]
            [Description("Configuration Platform to be built (eg: X86, Any CPU,etc).")]
            public InArgument<string> Platform { get; set; }

            /// <summary>
            /// Specifies the File to log all output to.
            /// <para></para>
            /// If ommited the file will be automatically placed on the logs path of DropsFolder\logs
            /// </summary>
            [Description("The path for the log file. You should only use these if you want to control the log file for a particular reason. Otherwise the system will take care of the log file details for you")]
            public InArgument<string> OutputFile { get; set; }

            /// <summary>
            /// The action to perform with devenv (build, rebuild, clean,...)
            /// </summary>
            [RequiredArgument]
            [Description("The action to be performed by dev (eg: Build, Clean,etc).")]
            [Editor("Microsoft.TeamFoundation.Build.Controls.EnumPropertyEditor, Microsoft.TeamFoundation.Build.Controls", typeof(System.Drawing.Design.UITypeEditor))]
            public InArgument<VSDevEnvAction> Action { get; set; }

            /// <summary>
            /// Executes the activity and returns the data necessary to execute devenv (executable + arguments)
            /// </summary>
            /// <param name="context">The context</param>
            protected override void Execute(CodeActivityContext context)
            {
                this.DevEnvCommandPath.Set(context, Path.Combine(this.DevEnvPath.Get(context), "devenv.com"));
                this.Arguments.Set(context, this.GenerateCommandLineCommands(context));
            }

            /// <summary>
            /// Generate the command line arguments to be passed to devenv
            /// <para></para>
            /// If the user hasn't specified an output FileName we will generate a log
            /// file name with the project name (using the appropriate configuration/platform settings)
            /// and store it on the logs folder in the drops directory
            /// </summary>
            /// <param name="context">Activity context</param>
            /// <returns>The command line arguments</returns>
            private string GenerateCommandLineCommands(ActivityContext context)
            {
                return string.Format(
                    "\"{0}\" {1} \"{2}|{3}\" /Out \"{4}\" ",
                    this.FilePath.Get(context),
                    VSDevEnv.GetActionCommandLineOption(this.Action.Get(context)),
                    this.Configuration.Get(context),
                    this.Platform.Get(context),
                    this.OutputFile.Get(context));
            }
        }

        /// <summary>
        /// Adds a link to the log file in the build logs.
        /// <para></para>
        /// The link is only added if the log file is stored in an UNC path
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class AddLinkToLogFile : BaseCodeActivity
        {
            /// <summary>
            /// The filename (complete path) of the file to linked
            /// </summary>
            public InArgument<string> FileName { get; set; }

            /// <summary>
            /// If the filename is stored in an UNC path then create the link.
            /// <para></para>
            /// The link is created with the message "Visual Studio DevEnv log file"
            /// </summary>
            protected override void InternalExecute()
            {
                Uri fileUri;
                string fileName = this.FileName.Get(this.ActivityContext);

                if (Uri.TryCreate(fileName, UriKind.Absolute, out fileUri))
                {
                    if (fileUri.IsUnc)
                    {
                        this.LogBuildLink(string.Format("Visual Studio DevEnv log file for {0}", Path.GetFileName(fileName)), new Uri(fileName));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the location where devenv output will be stored.
        /// <para></para>
        /// If the caller explicitily set the output log file use it, otherwise
        /// create a new one on the logs drops folder. The name will be base on the
        /// project to build, the configuration and the platform.
        /// <para></para>
        /// The log filename will be in the format $project$configuration$platform.log
        /// </summary>
        [BuildActivity(HostEnvironmentOption.All)]
        private sealed class GetDevEnvLogFile : BaseCodeActivity
        {
            /// <summary>
            /// The Path to the solution or Project to build
            /// </summary>
            [RequiredArgument]
            [Description("The complete path of the solution or project file to be built.")]
            public InArgument<string> FilePath { get; set; }

            /// <summary>
            /// The Configuration to Build.
            /// <example>
            /// <list type="">
            /// <item>Debug</item>
            /// <item>Release</item>
            /// </list>
            /// </example>
            /// </summary>
            [RequiredArgument]
            [Description("Configuration to be built (eg: Debug , Release,etc).")]
            public InArgument<string> Configuration { get; set; }

            /// <summary>
            /// The Platform to Build.
            /// </summary>
            [RequiredArgument]
            [Description("Configuration Platform to be built (eg: X86, Any CPU,etc).")]
            public InArgument<string> Platform { get; set; }

            /// <summary>
            /// Specifies the File to log all output to.
            /// <para></para>
            /// If ommited the file will be automatically placed on the logs path of DropsFolder\logs
            /// </summary>
            [Description("The path for the log file. You should only use these if you want to control the log file for a particular reason. Otherwise the system will take care of the log file details for you")]
            public InArgument<string> OutputFile { get; set; }

            public OutArgument<string> LogFile { get; set; }

            /// <summary>
            /// Executes the logic for this workflow activity
            /// </summary>
            protected override void InternalExecute()
            {
                var outputLogFile = this.OutputFile.Get(this.ActivityContext);

                if (string.IsNullOrWhiteSpace(outputLogFile))
                {
                    outputLogFile = this.GenerateLogFile(this.ActivityContext);
                }

                this.LogFile.Set(this.ActivityContext, outputLogFile);
            }

            /// <summary>
            /// Removes invalid character from a file name.
            /// </summary>
            /// <param name="fileName">The filename to sanitize</param>
            /// <returns>sanitized filename without invalid characters</returns>
            private static string SanitizeFileName(string fileName)
            {
                return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, invalidChar) => current.Replace(invalidChar.ToString(), string.Empty));
            }

            /// <summary>
            /// Make sure we have a log file in place.
            /// <para></para>
            /// If the user hasn't specified one we will create one for him on
            /// the logs drops folder path.
            /// <para></para>
            /// The file will have the same name of the solution/project being built, plus the
            /// context for configurationa and platform with the .log file extension
            /// </summary>
            /// <param name="context">Activity Context</param>
            /// <returns>The location of the log file</returns>
            private string GenerateLogFile(ActivityContext context)
            {
                string finalLogFile = null;

                var buildDetail = context.GetExtension<IBuildDetail>();
                var filePath = this.FilePath.Get(this.ActivityContext);
                var configuration = this.Configuration.Get(this.ActivityContext);
                var platform = this.Platform.Get(this.ActivityContext);

                var fileName = Path.GetFileName(filePath);

                if (fileName != null)
                {
                    string logDirectory = Path.Combine(buildDetail.DropLocation, @"logs");

                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    finalLogFile = Path.GetFileNameWithoutExtension(fileName);

                    if (string.IsNullOrWhiteSpace(configuration) == false)
                    {
                        finalLogFile += configuration;
                    }

                    if (string.IsNullOrWhiteSpace(platform) == false)
                    {
                        finalLogFile += platform;
                    }

                    finalLogFile += ".log";

                    finalLogFile = Path.Combine(logDirectory, SanitizeFileName(finalLogFile));
                }

                this.LogBuildMessage(string.Format("Log file {0} will be used.", finalLogFile));

                return finalLogFile;
            }
        }
    }
}