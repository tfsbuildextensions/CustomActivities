//-----------------------------------------------------------------------
// <copyright file="BasePuttyActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
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
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.VersionControl.Common;
    using TfsBuildExtensions.Activities.FileSystem;
    using TfsBuildExtensions.Activities.Internal;
    using TfsBuildExtensions.TfsUtilities;

    /// <summary>
    /// Base activity for performing operations on a remote host using PuTTy
    /// This is achieved by invoking available command from PuTTy (http://www.putty.org/)
    /// <para></para>
    /// To use this activity PuTTY needs to be installed on the build agent(s) machine.
    /// We assume build is installed on %ProgramFiles%\PuTTY or 
    /// in %ProgramFiles(x86)%\PuTTY on 64 bit Operating systems.
    /// <para>
    /// In order to PuTTY to remotely contact a host, the hosts needs to be known, this 
    /// has to be done interactively by logging on the machine with the build agent user
    /// and connect to the host with PuTTY and accepting the host key
    /// This operation needs to be done only once (per each different host) since
    /// the key will be stored on the registry and afterwards PuTTY will use it.
    /// </para>
    /// <para>
    /// In order to make this operation more scalable (or even possible since we can't
    /// interactively login with NETWORK SERVICES account which is the default account
    /// for builds) we can optionally pass a parameter with a file (.reg format)
    /// containing the known hosts.
    /// This will allows us to have as many hosts as we want, and everytime we add a
    /// host we can manage it centrally instead of having to log on manually on all
    /// our agents. (the file can be either a file in the filesystem or stored in 
    /// source control).
    /// We can do this by connecting to the hosts on a client machine, export a registry
    /// key which holds the know hosts and the activity upon execution will add the hosts
    /// to the agent.
    /// You can export the hosts list with the reg.exe command
    /// <code>reg.exe export HKEY_CURRENT_USER\Software\SimonTatham\PuTTY</code>
    /// </para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public abstract class BasePuttyActivity : BaseActivity
    {
        private BuildMessageImportance loggingOutputImportance = BuildMessageImportance.Normal;

        /// <summary>
        /// The authentication information that we will use to authenticate the 
        /// user against the remote machine
        /// </summary>
        [RequiredArgument]
        [Description("authentication information")]
        [Category("Authentication")]
        public InArgument<SSHAuthentication> Authentication { get; set; }

        /// <summary>
        /// The registry (.reg file exported with reg.exe or regedit.exe) file 
        /// that contains the know hosts. Using this file to registry the hosts 
        /// which we know will allows us to conect to them without having
        /// to manually login on the build agent(s) machine once to interactively 
        /// call PuTTY to register the host on the registry.
        /// This file can be either a file in the local file system or stored on 
        /// source control
        /// </summary>
        [Description("The known host files (.reg format). Allows you to load on the agent the know hosts")]
        [Category("Authentication")]
        public InArgument<string> KnownHostsFile { get; set; }

        /// <summary>
        /// Logging level for the placing the standard output content directly on 
        /// the build log
        /// </summary>
        [Description("Mininum log level to used to write command standard output to the build log. This can lead to a lot of noise in the build log.")]
        [Category("Logging")]
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
        /// Specifies the File to log all output to.
        /// <para></para>
        /// </summary>
        [Description("The path for the log file. You should only use these if you want to control the log file for a particular reason. Otherwise the system will take care of the log file details for you")]
        [Category("Logging")]
        public InArgument<string> LogFileName { get; set; }

        /// <summary>
        /// Should we log to the log file errors that appear on stderr?
        /// If not the errors will be logged into team build public
        /// </summary>
        [Description("Should we log errors sent to stderr to the log file instead of the build log output as errors?")]
        [Category("Logging")]
        public InArgument<bool> LogErrorToFile { get; set; }

        /// <summary>
        /// The path for the putty commands. If missing we will try to determine the path
        /// ourselves or ultimately rely on path
        /// </summary>
        [Description("The path for putty command tools. If ommited we will try to determine the path on our own")]
        [Category("Configuration")]
        public InArgument<string> ToolsPath { get; set; }

        /// <summary>
        /// The port used for communications. Only to be used if the non standard port
        /// is not going to be used
        /// </summary>
        [Description("the port (optional). Only use if you to use a non standard port")]
        [Category("Host")]
        public InArgument<int> Port { get; set; }                

        /// <summary>
        /// Were there any errors while executing the operation? (or even before that)
        /// </summary>
        [Description("Has the command finished with errors? (or the execution failed even before that)")]
        [Category("Result")]
        public OutArgument<bool> HasErrors { get; set; }

        /// <summary>
        /// The error code returned by remote command
        /// </summary>
        [Description("command execution return code")]
        [Category("Result")]
        public OutArgument<int> ErrorCode { get; set; }

        /// <summary>
        /// Holds the string that will be shown on the link for the log file (if the file exists)
        /// </summary>
        protected string LinkMessage { get; set; }

        /// <summary>
        /// This method is responsable for returning an activity that upon the received parameters and the putty command
        /// being implemented will return the executable plus parameters to be invoked
        /// </summary>
        /// <param name="toolsPathVariable">The workflow variable that will hold the putty executable path</param>
        /// <param name="toolArgumentsVariable">The workflow variable that will hold the parameters to be passed to the executable</param>
        /// <returns>the activity code that will return the variables used to call the 
        /// putty executable that will perform the required action</returns>
        protected abstract Activity CreateCallingParametersBody(Variable<string> toolsPathVariable, Variable<string> toolArgumentsVariable);

        /// <summary>
        /// This method is responsable for returning an activity that will validate if the
        /// main activity has received all the necessary parameters and if their
        /// semantic is valid.
        /// </summary>
        /// <returns>The activity code that will validate the parameters</returns>
        protected abstract Activity CreateParametersValidationBody();

        /// <summary>
        /// Creates the code for executing putty activity
        /// </summary>
        /// <returns>The code of the activity to be executed</returns>
        protected override Activity CreateInternalBody()
        {
            return new Sequence
            {
                Activities =
                {
                    this.CreateBaseParametersValidationParametersBody(),
                    this.CreateParametersValidationBody(),
                    
                    // Are the parameters valid? then proceed.
                    new @If
                    {
                        DisplayName = "If? Parameters are valid",
                        
                        Condition = new InArgument<bool>(env => this.HasErrors.GetLocation(env).Value),
                    
                        Else = new Sequence 
                        {
                            DisplayName = "Main  Body",
                            
                            Activities = 
                            {
                                this.CreateInvokeRemoteCommandBody()
                            }
                        }                
                    }
                }
            };
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
        /// Gets the code for the activity that validates the main activity parameters
        /// </summary>
        /// <returns>The activity that validates the parameters</returns>
        private ValidateBaseParametersActivityInternal CreateBaseParametersValidationParametersBody()
        {
            return new ValidateBaseParametersActivityInternal
            {
                DisplayName = "Validate Base Parameters",

                Authentication = new InArgument<SSHAuthentication>(env => this.Authentication.Get(env)),

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
        private Activity CreateOperationFailedBody(string message)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Extracting methods would be no gain and make the core more unreadable")]
        private Activity CreateInvokeRemoteCommandBody()
        {
            var logFileLocationVariable = new Variable<string> { Name = "Log File Location" };
            var puttyCommandPathVariable = new Variable<string> { Name = "Putty Command Path" };
            var puttyArgumentsVariable = new Variable<string> { Name = "Putty Arguments" };
            var privateKeyVariable = new Variable<string> { Name = "Putty Private Key File Location" };

            var errorHandlerArgument = new DelegateInArgument<string>();
            var stdOutputHandlerArgument = new DelegateInArgument<string>();

            // gets the location of the logfile (if any), gets the parameters to execute the appropriate putty command and invokes it
            return new Sequence
            {
                Variables = { puttyCommandPathVariable, puttyArgumentsVariable, logFileLocationVariable, privateKeyVariable },

                Activities =
                {             
                    this.CreateLogFileBody(logFileLocationVariable),

                    new DownloadKeyFileIfNecessary 
                    {
                        Authentication = new InArgument<SSHAuthentication>(env => this.Authentication.Get(env)),
                                                
                        IgnoreExceptions = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),
                        FailBuildOnError = new InArgument<bool>(env => this.FailBuildOnError.Get(env)),
                        LogExceptionStack = new InArgument<bool>(env => this.LogExceptionStack.Get(env)),
                        TreatWarningsAsErrors = new InArgument<bool>(env => this.TreatWarningsAsErrors.Get(env))
                    },

                    this.CreateCallingParametersBody(puttyCommandPathVariable, puttyArgumentsVariable),

                    new RegisterKnownHosts 
                    {
                        RegistryHostFileName = new InArgument<string>(env => this.KnownHostsFile.Get(env)),

                        IgnoreExceptions = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),
                        FailBuildOnError = new InArgument<bool>(env => this.FailBuildOnError.Get(env)),
                        LogExceptionStack = new InArgument<bool>(env => this.LogExceptionStack.Get(env)),
                        TreatWarningsAsErrors = new InArgument<bool>(env => this.TreatWarningsAsErrors.Get(env))
                    },
                    
                    new LogBuildMessage
                    {
                        Message = new InArgument<string>(env => string.Format("Calling {0} {1}", puttyCommandPathVariable.Get(env), puttyArgumentsVariable.Get(env)))                     
                    },

                    new InvokeProcess
                    {
                        DisplayName = "Invoke Putty command",
                        FileName = new InArgument<string>(puttyCommandPathVariable),
                        Arguments = new InArgument<string>(puttyArgumentsVariable),
                        Result = new OutArgument<int>(env => this.ErrorCode.GetLocation(env).Value),

                        OutputDataReceived = new ActivityAction<string>
                        {
                            Argument = stdOutputHandlerArgument,
                            Handler = new Sequence
                            {
                                Activities = 
                                {
                                    new LogBuildMessage 
                                    {
                                        Message = stdOutputHandlerArgument,
                                        Importance = new InArgument<BuildMessageImportance>(this.OutputLoggingLevel)
                                    },

                                    new @If 
                                    {
                                        Condition = new InArgument<bool>(env => logFileLocationVariable.Get(env) != null),

                                        Then = new WriteToFile
                                        {
                                            FileName = new InArgument<string>(env => logFileLocationVariable.Get(env)),
                                            Content = stdOutputHandlerArgument,
                                            AutoNewLine = true,
                                            Create = false
                                        }
                                    }
                                }
                            }
                        },
                        ErrorDataReceived = new ActivityAction<string>
                        {
                            Argument = errorHandlerArgument,
                            Handler = new @If 
                            {                                           
                                Condition = new InArgument<bool>(env => this.LogErrorToFile.Get(env) && logFileLocationVariable.Get(env) != null),

                                Then = new WriteToFile
                                {
                                    FileName = new InArgument<string>(env => logFileLocationVariable.Get(env)),
                                    Content = errorHandlerArgument,
                                    AutoNewLine = true,
                                    Create = false
                                },
                            
                                Else = new LogBuildError
                                {
                                    Message = errorHandlerArgument,
                                    FailBuildOnError = new InArgument<bool>(false)                                
                                }                            
                            }
                        }
                    },

                    new DeletePrivateKeyTemporaryFile
                    {
                        Authentication = new InArgument<SSHAuthentication>(env => this.Authentication.Get(env))
                    },

                    // IF output log file exists then create the link on the build detail report
                    new @If
                    {
                        Condition = new InArgument<bool>(env => logFileLocationVariable.Get(env) != null && System.IO.File.Exists(logFileLocationVariable.Get(env))),

                        Then = new AddLinkToLogFile
                        {
                            FileName = new InArgument<string>(env => logFileLocationVariable.Get(env)),
                            LinkMessage = this.LinkMessage
                        }
                    },
                    new @If
                    {
                        Condition = new InArgument<bool>(env => this.ErrorCode.GetLocation(env) != null && this.ErrorCode.GetLocation(env).Value != 0),
                        Then = this.CreateOperationFailedBody("Calling putty failed.")                                    
                    }
                }
            };
        }

        /// <summary>
        /// Gets the code for the activity that is responsable to determine the log file file path
        /// </summary>
        /// <param name="logFileLocationVariable">The variable where the complete log file path will be stored</param>
        /// <returns>The activity</returns>
        private GetLogFile CreateLogFileBody(Variable<string> logFileLocationVariable)
        {
            return new GetLogFile
            {
                LogFilename = new InArgument<string>(env => this.LogFileName.Get(env)),
                LogFile = new OutArgument<string>(logFileLocationVariable)
            };
        }

        /// <summary>
        /// Validates the parameters of the main activity.
        /// The hosts is mandatory        
        /// Username/password(key) also mandatory(if using usr/pwd authentication)
        /// key is mandatory (if using private key authentication)
        /// <para></para>
        /// If the parameters are invalid logs a build error message
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
        private class ValidateBaseParametersActivityInternal : BaseCodeActivity
        {
            /// <summary>
            /// The authentication information that we will use to authenticate on the remote machine
            /// </summary>
            [RequiredArgument]
            [Description("authentication information")]
            public InArgument<SSHAuthentication> Authentication { get; set; }

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
                var auth = this.Authentication.Get(this.ActivityContext);
                bool error = false;

                switch (auth.AuthType)
                {
                    case SSHAuthenticationType.UserNamePassword:
                        if (string.IsNullOrWhiteSpace(auth.User) || string.IsNullOrWhiteSpace(auth.Key))
                        {
                            this.LogBuildError("You have to specify the username/password (key) to authenticate on the remote host");
                            error = true;
                        }

                        break;
                    case SSHAuthenticationType.PrivateKey:
                        if (string.IsNullOrWhiteSpace(auth.User) || string.IsNullOrWhiteSpace(auth.Key))
                        {
                            this.LogBuildError("You have to specify the username and private key file (key) to authenticate on the remote host");
                            error = true;
                        }

                        break;
                    default:
                        throw new NotImplementedException("Unknown authentication type");
                }

                this.HasErrors.Set(this.ActivityContext, error);
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

            public InArgument<string> LinkMessage { get; set; }

            /// <summary>
            /// If the filename is stored in an UNC path then create the link.
            /// <para></para>
            /// The link is created with the message in the member LinkMesage
            /// </summary>
            protected override void InternalExecute()
            {
                Uri fileUri;
                string fileName = this.FileName.Get(this.ActivityContext);

                if (Uri.TryCreate(fileName, UriKind.Absolute, out fileUri))
                {
                    if (fileUri.IsUnc)
                    {
                        this.LogBuildLink(string.Format(this.LinkMessage.Get(this.ActivityContext), Path.GetFileName(fileName)), new Uri(fileName));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the complete path of the log filename. The file will be automatically stored in the drops folder.
        /// The name will be based on the user LogFilename parameter. 
        /// <para></para>
        /// </summary>
        [BuildActivity(HostEnvironmentOption.All)]
        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class GetLogFile : BaseCodeActivity
        {
            /// <summary>
            /// Specifies the File to log all output to.
            /// <para></para>
            /// If ommited the file will be automatically placed on the logs path of DropsFolder\logs
            /// </summary>
            [Description("The name for the log file")]
            public InArgument<string> LogFilename { get; set; }

            public OutArgument<string> LogFile { get; set; }

            /// <summary>
            /// Executes the logic for this workflow activity
            /// </summary>
            protected override void InternalExecute()
            {
                var outputLogFile = this.LogFilename.Get(this.ActivityContext);

                if (string.IsNullOrWhiteSpace(outputLogFile) == false)
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
            /// context for configuration and platform with the .log file extension
            /// </summary>
            /// <param name="context">Activity Context</param>
            /// <returns>The location of the log file</returns>
            private string GenerateLogFile(ActivityContext context)
            {
                string finalLogFile = null;
                string logDirectory;

                var buildDetail = context.GetExtension<IBuildDetail>();

                var fileName = this.LogFilename.Get(context);

                if (fileName == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(buildDetail.LogLocation) == false)
                {
                    logDirectory = buildDetail.LogLocation;
                }
                else if (string.IsNullOrEmpty(buildDetail.DropLocation) == false)
                {
                    logDirectory = Path.Combine(buildDetail.DropLocation, @"logs");
                }
                else
                {
                    this.LogBuildWarning("drop location not defined. Will not write to log file");
                    return null;
                }

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                finalLogFile = Path.Combine(logDirectory, SanitizeFileName(fileName));

                this.LogBuildMessage(string.Format("Log file {0} will be used.", finalLogFile));

                return finalLogFile;
            }
        }

        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class DeletePrivateKeyTemporaryFile : BaseCodeActivity
        {
            [RequiredArgument]
            public InArgument<SSHAuthentication> Authentication { get; set; }

            protected override void InternalExecute()
            {
                var auth = this.Authentication.Get(this.ActivityContext);

                if (auth.AuthType == SSHAuthenticationType.PrivateKey && string.IsNullOrWhiteSpace(auth.PrivateKeyFileLocation) == false && VersionControlPath.IsServerItem(auth.Key))
                {
                    try
                    {
                        if (System.IO.File.Exists(auth.PrivateKeyFileLocation))
                        {
                            System.IO.File.Delete(auth.PrivateKeyFileLocation);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        LogBuildWarning(string.Format("Failed to delete file {0}", auth.PrivateKeyFileLocation));
                    }
                }
            }
        }

        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class DownloadKeyFileIfNecessary : BaseCodeActivity
        {
            [RequiredArgument]
            public InArgument<SSHAuthentication> Authentication { get; set; }

            protected override void InternalExecute()
            {
                var auth = this.Authentication.Get(this.ActivityContext);

                if (auth.AuthType == SSHAuthenticationType.PrivateKey)
                {
                    auth.PrivateKeyFileLocation = auth.Key;

                    if (VersionControlPath.IsServerItem(auth.Key))
                    {
                        var vcs = this.ActivityContext.GetExtension<TfsTeamProjectCollection>().GetService<VersionControlServer>();

                        auth.PrivateKeyFileLocation = Path.GetTempFileName();

                        vcs.DownloadFile(auth.Key, auth.PrivateKeyFileLocation);
                    }
                }
            }
        }

        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class RegisterKnownHosts : BaseCodeActivity
        {
            /// <summary>
            /// The registry file that contains the know hosts. 
            /// If the file is specified it's content will be imported 
            /// into the registry
            /// </summary>
            public InArgument<string> RegistryHostFileName { get; set; }

            protected override void InternalExecute()
            {
                var fileName = this.RegistryHostFileName.Get(this.ActivityContext);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                this.LogBuildMessage(string.Format("Loading know hosts file from {0}", fileName));

                using (var autoTracker = new AutoFileTrackerFromSourceControl(this.ActivityContext.GetExtension<TfsTeamProjectCollection>()))
                {
                    int errorCode;
                    var knowHostRegistryFile = autoTracker.GetFile(fileName);

                    if ((errorCode = BasePuttyActivity.RegisterKnownHosts.RegisterKnownHostsWithFile(knowHostRegistryFile)) != 0)
                    {
                        this.LogBuildError(string.Format("Failed to register the hosts from file {0} with errorcode: {1}", fileName, errorCode));
                    }
                }
            }

            /// <summary>
            /// Registers the know hosts using a .reg file.
            /// Doesn't verify the keys that the file contains.
            /// So this can be used to load any keys into the registry.
            /// The entries are written in the registry with reg.exe
            /// </summary>
            /// <param name="knowHostRegistryFile">file path for the .reg file that holds the know hosts</param>
            /// <returns>The exit code from calling reg.exe</returns>
            private static int RegisterKnownHostsWithFile(string knowHostRegistryFile)
            {
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = "reg.exe";
                    process.StartInfo.Arguments = string.Format("import {0}", knowHostRegistryFile);
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.ErrorDialog = false;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    process.WaitForExit();

                    return process.ExitCode;
                }
            }
        }
    }
}
