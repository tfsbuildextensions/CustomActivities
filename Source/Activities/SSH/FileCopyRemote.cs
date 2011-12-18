//-----------------------------------------------------------------------
// <copyright file="FileCopyRemote.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;

    /// <summary>
    /// Activity to copy files to a remote system or from a remote system.
    /// The files are copied either using scp or sftp.
    /// This activity relies on PuTTy (http://www.putty.org) being installed on 
    /// the agent machine
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public sealed class FileCopyRemote : BasePuttyActivity
    {
        /// <summary>
        /// Initializes a new instance of the FileCopyRemote class.
        /// </summary>
        public FileCopyRemote()
        {
            LinkMessage = "File copy remote log file";
        }

        /// <summary>
        /// The source of the files to be copied.
        /// This is a straight quote from pscp documentation (http://the.earth.li/~sgtatham/putty/0.61/htmldoc/Chapter5.html#pscp-usage-basics-source)
        /// <para>
        /// One or more source files. Wildcards are allowed. The syntax of wildcards
        /// depends on the system to which they apply, so if you are copying from a Windows 
        /// system to a UNIX system, you should use Windows wildcard syntax (e.g. *.*), but
        /// if you are copying from a UNIX system to a Windows system, you would use 
        /// the wildcard syntax allowed by your UNIX shell (e.g. *). 
        /// </para>
        /// <para>
        /// If the source is a remote server and you do not specify a full pathname 
        /// (in UNIX, a pathname beginning with a / (slash) character), what you specify as 
        /// a source will be interpreted relative to your home directory on the remote server. 
        /// </para>
        /// </summary>
        [RequiredArgument]
        [Description("the source of the files. The local path or the remote path in the form host:remotepath. May contain wildcards")]
        [Category("Files")]
        public InArgument<string> Source { get; set; }

        /// <summary>
        /// The target path where the files will be copied too.
        /// This is a straight quote from pscp documentation (http://the.earth.li/~sgtatham/putty/0.61/htmldoc/Chapter5.html#pscp-usage-basics-target)
        /// <para>
        /// The filename or directory to put the file(s). When copying from a remote server
        /// to a local host, you may wish simply to place the file(s) in 
        /// the current directory. To do this, you should specify a target of .. 
        /// For example: pscp fred@example.com:/home/tom/.emacs .
        /// ...would copy /home/tom/.emacs on the remote server to the current directory. 
        /// </para>
        /// <para>
        /// As with the source parameter, if the target is on a remote server and is not a
        /// full path name, it is interpreted relative to your 
        /// home directory on the remote server. 
        /// </para>
        /// </summary>
        [RequiredArgument]
        [Description("the target folder where files will be copied to. The local path or the remote path in the form host:remotepath")]
        [Category("Files")]
        public InArgument<string> Target { get; set; }

        /// <summary>
        /// By default only files are copied. If you specify a folder it will skipped (as well as it's contents)
        /// if you wish to copy directories and it's content you need to see this to true
        /// </summary>
        [Description("Should directories by copied recursively?")]
        [Category("Files")]
        public InArgument<bool> Recursively { get; set; }

        /// <summary>
        /// Should we enable compression on file copying operation?
        /// </summary>
        [Description("Compress the files while copying?")]
        [Category("Files")]
        public InArgument<bool> EnableCompression { get; set; }

        /// <summary>
        /// Should the file attributes be preserved while copying file.
        /// By default the attributes are not preserver (when a file is copied a new timestap will be
        /// associated with the file). Se this to true if you wish to keep the timestamp of the original
        /// file.
        /// </summary>
        [Description("Preserve File Attributes?")]
        [Category("Files")]
        public InArgument<bool> PreserveFileAttributes { get; set; }

        /// <summary>
        /// Should we allow the expansion of wildcards at the server level?
        /// </summary>
        [Description("Allow Server side wildcards")]
        [Category("Files")]
        public InArgument<bool> Unsafe { get; set; }

        /// <summary>
        /// The protocol to be used to copy the files (<see cref="SSHCopyProtocol"/>)
        /// </summary>
        [Description("The Protocol to be used to perform the copy. (scp is used by default")]
        [Category("Files")]
        public InArgument<SSHCopyProtocol> Protocol { get; set; }

        /// <summary>
        /// Base on the main activity parameters constructs the code activity that 
        /// will return the command line arguments and putty executable location 
        /// that will be used to perform the copy operation
        /// </summary>
        /// <param name="toolsPathVariable">The PuTTY location + executable (pscp) that will perform the copy operation</param>
        /// <param name="toolArgumentsVariable">the parameters to be passed to the pscp so the copy of files is performed</param>
        /// <returns>The activity that will construct the command + parameters needed to perform the copy</returns>
        protected override Activity CreateCallingParametersBody(Variable<string> toolsPathVariable, Variable<string> toolArgumentsVariable)
        {
            return new GetPscpCallingParameters
            {
                ToolsPath = new InArgument<string>(env => this.ToolsPath.Get(env)),
                Authentication = new InArgument<SSHAuthentication>(env => this.Authentication.Get(env)),
                Port = new InArgument<int>(env => this.Port.Get(env)),

                // Specific input parameters
                EnableCompression = new InArgument<bool>(env => this.EnableCompression.Get(env)),
                PreserveFileAttributes = new InArgument<bool>(env => this.PreserveFileAttributes.Get(env)),
                Protocol = new InArgument<SSHCopyProtocol>(env => this.Protocol.Get(env)),
                Recursively = new InArgument<bool>(env => this.Recursively.Get(env)),
                Source = new InArgument<string>(env => this.Source.Get(env)),
                Target = new InArgument<string>(env => this.Target.Get(env)),
                Unsafe = new InArgument<bool>(env => this.Unsafe.Get(env)),

                // Output parameters
                ToolCommandPath = new OutArgument<string>(toolsPathVariable),
                Arguments = new OutArgument<string>(toolArgumentsVariable)
            };
        }

        /// <summary>
        /// This method is responsable for returning an activity that will validate if the
        /// main activity has received all the necessary parameters and if their
        /// semantic is valid.
        /// Only validates the subclass specific parameters. The common parameters are
        /// validated in the parent.
        /// <para>
        /// Validates if source and target parameters are valid.
        /// </para>
        /// </summary>
        /// <returns>The activity code that will validate the parameters</returns>
        protected override Activity CreateParametersValidationBody()
        {
            return new ValidatePscpParametersActivityInternal
            {
                DisplayName = "Validate pscp Parameters",

                Source = new InArgument<string>(env => this.Source.Get(env)),
                Target = new InArgument<string>(env => this.Target.Get(env)),

                HasErrors = new OutArgument<bool>(env => this.HasErrors.GetLocation(env).Value),

                IgnoreExceptions = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),
                FailBuildOnError = new InArgument<bool>(env => this.FailBuildOnError.Get(env)),
                LogExceptionStack = new InArgument<bool>(env => this.LogExceptionStack.Get(env)),
                TreatWarningsAsErrors = new InArgument<bool>(env => this.TreatWarningsAsErrors.Get(env))
            };
        }

        /// <summary>
        /// Validates the parameters of the main activity.
        /// <para></para>
        /// If the parameters are invalid logs a build error message
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
        private sealed class ValidatePscpParametersActivityInternal : BaseCodeActivity
        {
            /// <summary>
            /// The source of the files to be copied.
            /// This is a straight quote from pscp documentation (http://the.earth.li/~sgtatham/putty/0.61/htmldoc/Chapter5.html#pscp-usage-basics-source)
            /// <para>
            /// One or more source files. Wildcards are allowed. The syntax of wildcards
            /// depends on the system to which they apply, so if you are copying from a Windows 
            /// system to a UNIX system, you should use Windows wildcard syntax (e.g. *.*), but
            /// if you are copying from a UNIX system to a Windows system, you would use 
            /// the wildcard syntax allowed by your UNIX shell (e.g. *). 
            /// </para>
            /// <para>
            /// If the source is a remote server and you do not specify a full pathname 
            /// (in UNIX, a pathname beginning with a / (slash) character), what you specify as 
            /// a source will be interpreted relative to your home directory on the remote server. 
            /// </para>
            /// </summary>
            [Description("the source of the files. The local path or the remote path in the form host:remotepath. May contain wildcards")]
            [Category("Files")]
            public InArgument<string> Source { get; set; }

            /// <summary>
            /// The target path where the files will be copied too.
            /// This is a straight quote from pscp documentation (http://the.earth.li/~sgtatham/putty/0.61/htmldoc/Chapter5.html#pscp-usage-basics-target)
            /// <para>
            /// The filename or directory to put the file(s). When copying from a remote server
            /// to a local host, you may wish simply to place the file(s) in 
            /// the current directory. To do this, you should specify a target of .. 
            /// For example: pscp fred@example.com:/home/tom/.emacs .
            /// ...would copy /home/tom/.emacs on the remote server to the current directory. 
            /// </para>
            /// <para>
            /// As with the source parameter, if the target is on a remote server and is not a
            /// full path name, it is interpreted relative to your 
            /// home directory on the remote server. 
            /// </para>
            /// </summary>
            [Description("the target folder where files will be copied to. The local path or the remote path in the form host:remotepath")]
            [Category("Files")]
            public InArgument<string> Target { get; set; }

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
                var source = this.Source.Get(this.ActivityContext);
                var target = this.Target.Get(this.ActivityContext);

                if (string.IsNullOrWhiteSpace(source))
                {
                    LogBuildError("You have to specify the source where the files will be copied from");

                    this.HasErrors.Set(this.ActivityContext, true);

                    return;
                }

                if (string.IsNullOrWhiteSpace(target))
                {
                    LogBuildError("You have to specify the target where the files will be copied to");

                    this.HasErrors.Set(this.ActivityContext, true);

                    return;
                }
            }
        }

        /// <summary>
        /// Gets the parameters to call plink (the exec name + arguments)
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class GetPscpCallingParameters : CodeActivity
        {
            #region common input parameters
            /// <summary>
            /// The path (only the directory) where putty tools are installed.
            /// <example>c:\program files(x86)\putty</example>
            /// If empty we will try to determine the correct path automatically and as 
            /// last resort rely on the PATH environment variable
            /// </summary>
            [RequiredArgument]
            public InArgument<string> ToolsPath { get; set; }

            [RequiredArgument]
            public InArgument<SSHAuthentication> Authentication { get; set; }

            public InArgument<int> Port { get; set; }

            #endregion

            #region specific input parameters

            /// <summary>
            /// The source of the files to be copied.
            /// This is a straight quote from pscp documentation (http://the.earth.li/~sgtatham/putty/0.61/htmldoc/Chapter5.html#pscp-usage-basics-source)
            /// <para>
            /// One or more source files. Wildcards are allowed. The syntax of wildcards
            /// depends on the system to which they apply, so if you are copying from a Windows 
            /// system to a UNIX system, you should use Windows wildcard syntax (e.g. *.*), but
            /// if you are copying from a UNIX system to a Windows system, you would use 
            /// the wildcard syntax allowed by your UNIX shell (e.g. *). 
            /// </para>
            /// <para>
            /// If the source is a remote server and you do not specify a full pathname 
            /// (in UNIX, a pathname beginning with a / (slash) character), what you specify as 
            /// a source will be interpreted relative to your home directory on the remote server. 
            /// </para>
            /// </summary>
            [RequiredArgument]
            [Description("the source of the files. The local path or the remote path in the form host:remotepath. May contain wildcards")]
            [Category("Files")]
            public InArgument<string> Source { get; set; }

            /// <summary>
            /// The target path where the files will be copied too.
            /// This is a straight quote from pscp documentation (http://the.earth.li/~sgtatham/putty/0.61/htmldoc/Chapter5.html#pscp-usage-basics-target)
            /// <para>
            /// The filename or directory to put the file(s). When copying from a remote server
            /// to a local host, you may wish simply to place the file(s) in 
            /// the current directory. To do this, you should specify a target of .. 
            /// For example: pscp fred@example.com:/home/tom/.emacs .
            /// ...would copy /home/tom/.emacs on the remote server to the current directory. 
            /// </para>
            /// <para>
            /// As with the source parameter, if the target is on a remote server and is not a
            /// full path name, it is interpreted relative to your 
            /// home directory on the remote server. 
            /// </para>
            /// </summary>
            [RequiredArgument]
            [Description("the target folder where files will be copied to. The local path or the remote path in the form host:remotepath")]
            [Category("Files")]
            public InArgument<string> Target { get; set; }

            /// <summary>
            /// By default only files are copied. If you specify a folder it will skipped (as well as it's contents)
            /// if you wish to copy directories and it's content you need to see this to true
            /// </summary>
            [Description("Should directories by copied recursively?")]
            [Category("Files")]
            public InArgument<bool> Recursively { get; set; }

            /// <summary>
            /// Should the file attributes be preserved while copying file.
            /// By default the attributes are not preserver (when a file is copied a new timestap will be
            /// associated with the file). Se this to true if you wish to keep the timestamp of the original
            /// file.
            /// </summary>
            [Description("Preserve File Attributes?")]
            [Category("Files")]
            public InArgument<bool> PreserveFileAttributes { get; set; }

            /// <summary>
            /// Should we enable compression on file copying operation?
            /// </summary>
            [Description("Compress the files while copying?")]
            [Category("Files")]
            public InArgument<bool> EnableCompression { get; set; }

            [Description("Allow Server side wildcards")]
            [Category("Files")]
            public InArgument<bool> Unsafe { get; set; }

            [Description("The Protocol to be used to perform the copy. (scp is used by default")]
            [Category("Files")]
            public InArgument<SSHCopyProtocol> Protocol { get; set; }

            #endregion

            #region common output parameters

            /// <summary>
            /// The complete path (directory + executable name) to be executed.
            /// </summary>
            public OutArgument<string> ToolCommandPath { get; set; }

            /// <summary>
            /// The arguments to be passed to tool
            /// </summary>
            public OutArgument<string> Arguments { get; set; }

            #endregion

            /// <summary>
            /// Executes the activity and returns the data necessary to execute putty (executable + arguments)
            /// </summary>
            /// <param name="context">The context</param>
            protected override void Execute(CodeActivityContext context)
            {
                var toolsPath = PuttyHelper.GetPuttyPath(this.ToolsPath.Get(context));
                
                if (string.IsNullOrEmpty(toolsPath))
                {
                    context.TrackBuildWarning("can't determine PuTTy tools path. Will rely on path");
                    toolsPath = string.Empty;
                }

                this.ToolCommandPath.Set(context, Path.Combine(toolsPath, "pscp.exe"));
                this.Arguments.Set(context, this.GenerateCommandLineCommands(context));
            }

            /// <summary>
            /// Gets the string passed as parameter if condition is true
            /// </summary>
            /// <param name="condition">The condition value</param>
            /// <param name="value">the value to be returned if the condition value is true</param>
            /// <returns>The value if true an empty string otherwise</returns>
            private static string GetStringIfTrue(bool condition, string value)
            {
                return condition ? value : string.Empty;
            }

            /// <summary>
            /// Gets the string passed as parameter if the argument has the value
            /// true
            /// </summary>
            /// <param name="argument">the argument to check</param>
            /// <param name="context">activity context</param>
            /// <param name="value">the value to be returned if the variable holds true </param>
            /// <returns>The value if true an empty string otherwise</returns>
            private static string GetStringIfTrue(InArgument<bool> argument, ActivityContext context, string value)
            {
                return GetStringIfTrue(argument.Get(context), value);
            }

            /// <summary>
            /// Generate the command line arguments to be passed plink
            /// <para></para>
            /// If the user hasn't specified an output FileName we will generate a log
            /// file name with the project name (using the appropriate configuration/platform settings)
            /// and store it on the logs folder in the drops directory
            /// </summary>
            /// <param name="context">Activity context</param>
            /// <returns>The command line arguments</returns>
            private string GenerateCommandLineCommands(ActivityContext context)
            {
                var auth = this.Authentication.Get(context);
                var port = this.Port.Get(context);
                var portParameter = string.Empty;

                if (port > 0)
                {
                    portParameter = string.Format("-P {0}", port);
                }

                return string.Format(
                    "{0} -{1} {2} {3} {4} {5} -batch -noagent {6} {7} {8}",
                    PuttyHelper.GetAuthenticationParameters(auth),
                    this.Protocol.Get(context),
                    GetStringIfTrue(this.Unsafe, context, "-unsafe"),
                    GetStringIfTrue(this.EnableCompression, context, "-C"),
                    GetStringIfTrue(this.Recursively, context, "-r"),
                    GetStringIfTrue(this.PreserveFileAttributes, context, "-p"),
                    portParameter,
                    this.Source.Get(context),
                    this.Target.Get(context));
            }
        }
    }
}
