//-----------------------------------------------------------------------
// <copyright file="InvokeRemoteScript.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    using System.Activities;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;

    /// <summary>
    /// Activity for invoking a remote script via SSH protocol.
    /// This is achieved by invoking plink.exe command from PuTTy (http://www.putty.org/)
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public sealed class InvokeRemoteScript : BasePuttyActivity
    {
        /// <summary>
        /// Initializes a new instance of the InvokeRemoteScript class.
        /// </summary>
        public InvokeRemoteScript()
        {
            this.LinkMessage = "Remote script invocation log file";
        }

        /// <summary>
        /// The host where the operation will be performed
        /// No need to specify the host with the username@host since we pass the username explicitily
        /// </summary>
        [RequiredArgument]
        [Description("the host where the command will be executed")]
        [Category("Host")]
        public InArgument<string> Host { get; set; }                

        /// <summary>
        /// The shell command to be executed remotely. You can pass not only the command name but also the
        /// wanted parameters
        /// <example>ls -al</example>
        /// </summary>
        [RequiredArgument]
        [Description("the shell command to be executed remotely")]
        [Category("Command")]
        public InArgument<string> Command { get; set; }

        /// <summary>
        /// This method is responsable for returning an activity that will validate if the
        /// main activity has received all the necessary parameters and if their
        /// semantic is valid.
        /// Only validates the subclass specific parameters. The common parameters are
        /// validated in the parent.
        /// <para>
        /// Validates if host and the command are present.
        /// </para>
        /// </summary>
        /// <returns>The activity code that will validate the parameters</returns>
        protected override Activity CreateParametersValidationBody()
        {
            return new ValidatePlinkParametersActivityInternal
            {
                DisplayName = "Validate Plink Parameters",

                Host = new InArgument<string>(env => this.Host.Get(env)),
                Command = new InArgument<string>(env => this.Command.Get(env)),

                HasErrors = new OutArgument<bool>(env => this.HasErrors.GetLocation(env).Value),

                IgnoreExceptions = new InArgument<bool>(env => this.IgnoreExceptions.Get(env)),
                FailBuildOnError = new InArgument<bool>(env => this.FailBuildOnError.Get(env)),
                LogExceptionStack = new InArgument<bool>(env => this.LogExceptionStack.Get(env)),
                TreatWarningsAsErrors = new InArgument<bool>(env => this.TreatWarningsAsErrors.Get(env))
            };
        }

        /// <summary>
        /// Gets the code for the activity that is responsable for generating the parameters that will be used to invoke plink
        /// </summary>
        /// <param name="toolsPathVariable">The PuTTY location + executable (plink) that will perform the remote call operation</param>
        /// <param name="toolArgumentsVariable">the parameters to be passed to the plink so command is remotely invoked</param>
        /// <returns>the activity</returns>
        protected override Activity CreateCallingParametersBody(Variable<string> toolsPathVariable, Variable<string> toolArgumentsVariable)
        {
            return new GetPlinkCallingParameters
            {
                // Common Input parameters
                ToolsPath = new InArgument<string>(env => this.ToolsPath.Get(env)),                
                Authentication = new InArgument<SSHAuthentication>(env => this.Authentication.Get(env)),
                Port = new InArgument<int>(env => this.Port.Get(env)),

                // Specific input parameters
                Host = new InArgument<string>(env => this.Host.Get(env)),
                Command = new InArgument<string>(env => this.Command.Get(env)),

                // Output parameters
                ToolCommandPath = new OutArgument<string>(toolsPathVariable),
                Arguments = new OutArgument<string>(toolArgumentsVariable)
            };
        }

        /// <summary>
        /// Gets the parameters to call plink (the exec name + arguments)
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.None)]
        private sealed class GetPlinkCallingParameters : CodeActivity
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
            /// The remote command to be executed. It can include parameters
            /// <example>ls -al /root </example>
            /// </summary>
            [RequiredArgument]
            [Description("The remote command to be executed (can include arguments)")]
            public InArgument<string> Command { get; set; }

            [RequiredArgument]
            public InArgument<string> Host { get; set; }

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

                this.ToolCommandPath.Set(context, Path.Combine(toolsPath, "plink.exe"));
                this.Arguments.Set(context, this.GenerateCommandLineCommands(context));
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
                var host = this.Host.Get(context);
                var portParameter = string.Empty;

                if (port > 0)
                {
                    portParameter = string.Format("-P {0}", port);
                }

                return string.Format(
                    "{0} -ssh -batch -noagent {1} {2} {3}",
                    PuttyHelper.GetAuthenticationParameters(auth),
                    portParameter,
                    host,
                    this.Command.Get(context));
            }
        }

        /// <summary>
        /// Validates the parameters of the main activity.
        /// The hosts is mandatory        
        /// Username/password also mandatory
        /// <para></para>
        /// If the parameters are invalid logs a build error message
        /// </summary>
        [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
        private sealed class ValidatePlinkParametersActivityInternal : BaseCodeActivity
        {
            /// <summary>
            /// the command to be executed
            /// </summary>
            [Description("the shell command to be executed remotely")]
            public InArgument<string> Command { get; set; }

            /// <summary>
            /// The host where the operation will be performed
            /// No need to specify the host with the username@host since we pass the username explicitily
            /// </summary>
            [Description("the host where the command will be executed")]
            public InArgument<string> Host { get; set; }

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
                var command = this.Command.Get(this.ActivityContext);
                var host = this.Host.Get(this.ActivityContext);

                if (string.IsNullOrWhiteSpace(host))
                {
                    this.LogBuildError("You have to specify the host where the command will be executed");
                    this.HasErrors.Set(this.ActivityContext, true);
                    return;
                }

                if (string.IsNullOrWhiteSpace(command))
                {
                    this.LogBuildError("You have to specify the command to be executed");
                    this.HasErrors.Set(this.ActivityContext, true);
                }
            }
        }
    }
}