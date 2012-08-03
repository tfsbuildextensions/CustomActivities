//-----------------------------------------------------------------------
// <copyright file="AzureAsyncOperation.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.ComponentModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using TfsBuildExtensions.Activities.Azure.Common;

    /// <summary>
    /// Composite activity to perform an Azure operation and poll for its results.
    /// </summary>
    [Description("Composite activity to perform an Azure operation and poll for its results.")]
    [Designer(typeof(AzureOperationDesigner))]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class AzureAsyncOperation : NativeActivity
    {
        private int timeoutSeconds = 300;
        private int pollingInterval = 15;
        private ActivityFunc<string, string, string, string> pollingBody;
        private ActivityAction<int> delayBody;

        /// <summary>
        /// Initializes a new instance of the AzureAsyncOperation class.
        /// </summary>
        public AzureAsyncOperation()
        {
            this.PollingEndTime = new Variable<DateTime>() { Default = null, Name = "EndTime" };
            this.OperationId = new Variable<string>() { Default = null, Name = "LocalOperationId" };
            this.AzureActivityExceptionCaught = new Variable<bool>() { Default = false, Name = "AzureExceptionCaught" };
        }

        /// <summary>
        /// The Azure operation to perform and monitor.
        /// </summary>
        public BaseAzureAsynchronousActivity Operation { get; set; }

        /// <summary>
        /// The activity to perform if the operation is successful.
        /// </summary>
        public Activity Success { get; set; }

        /// <summary>
        /// The activity to perform if the operation is not successful.
        /// </summary>
        public Activity Failure { get; set; }

        /// <summary>
        /// Gets or sets the Azure subscription ID.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the Azure account certificate.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> CertificateThumbprintId { get; set; }

        /// <summary>
        /// Gets or sets the maximum time to wait for the Operation to complete.
        /// </summary>
        /// <remarks>This timeout will be a minimum of 30 seconds, and a default of 5 minutes.</remarks>
        public int TimeoutSeconds
        {
            get
            {
                return this.timeoutSeconds;
            }

            set
            {
                if (value > 30)
                {
                    this.timeoutSeconds = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the interval between polling retries for the Operation status.
        /// </summary>
        /// <remarks>This interval will be between 1 and 30 seconds, and a default of 15 seconds.</remarks>
        public int PollingInterval
        {
            get
            {
                return this.pollingInterval;
            }

            set
            {
                if (value >= 1 && value <= 30)
                {
                    this.pollingInterval = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the time at which polling must end.
        /// </summary>
        private Variable<DateTime> PollingEndTime { get; set; }

        /// <summary>
        /// Gets or sets the Azure aynchronous operation ID.
        /// </summary>
        private Variable<string> OperationId { get; set; }

        /// <summary>
        /// Gets or sets whether an exception occured during execution of the Azure activity or polling.
        /// </summary>
        private Variable<bool> AzureActivityExceptionCaught { get; set; }

        /// <summary>
        /// Gets the body of the internal status polling activities.
        /// </summary>
        private ActivityFunc<string, string, string, string> PollingBody
        {
            get { return this.pollingBody ?? (this.pollingBody = this.CreatePollingBody()); }
        }

        /// <summary>
        /// Gets the body of the internal delay activity.
        /// </summary>
        private ActivityAction<int> DelayBody
        {
            get { return this.delayBody ?? (this.delayBody = this.CreateDelayBody()); }
        }

        /// <summary>
        /// Creates and validates a description of the activity's arguments, variables, child activities, and activity delegates.
        /// </summary>
        /// <param name="metadata">The activity's metadata that encapsulates the activity's arguments, variables, child activities, and activity delegates.</param>
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            // Add a validation error if the Operation is not defined.
            if (this.Operation == null)
            {
                metadata.AddValidationError("AzureAsyncOperation requires an Azure activity to execute.");
                return;
            }

            // Add the publicly defined activities as children.
            metadata.AddChild(this.Operation);
            metadata.AddChild(this.Success);
            metadata.AddChild(this.Failure);

            // Define internal variables.
            metadata.AddImplementationVariable(this.PollingEndTime);
            metadata.AddImplementationVariable(this.OperationId);
            metadata.AddImplementationVariable(this.AzureActivityExceptionCaught);

            // Define public arguments.
            var thumbArgument = new RuntimeArgument("CertificateThumbprintId", typeof(string), ArgumentDirection.In);
            metadata.Bind(this.CertificateThumbprintId, thumbArgument);
            metadata.AddArgument(thumbArgument);
            var subArgument = new RuntimeArgument("SubscriptionId", typeof(string), ArgumentDirection.In);
            metadata.Bind(this.SubscriptionId, subArgument);
            metadata.AddArgument(subArgument);

            // Add our activities as delegates.
            metadata.AddDelegate(this.PollingBody);
            metadata.AddDelegate(this.DelayBody);
        }

        /// <summary>
        /// Initiate the the execution of the Operation activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(NativeActivityContext context)
        {
            // Ensure that we start thinking that there are no exceptions
            this.AzureActivityExceptionCaught.Set(context, false);

            // Schedule the Operation for evaluation
            context.ScheduleActivity(this.Operation, new CompletionCallback<string>(this.OnOperationCompleted), new FaultCallback(this.OnOperationFault));
        }

        /// <summary>
        /// Create the structure of the activity for polling Azure for operation status.
        /// </summary>
        /// <returns>An activity delegate.</returns>
        private ActivityFunc<string, string, string, string> CreatePollingBody()
        {
            var operationId = new DelegateInArgument<string>() { Name = "FuncOperationId" };
            var subscriptionId = new DelegateInArgument<string>() { Name = "FuncSubscriptionId" };
            var certificateThumbprintId = new DelegateInArgument<string>() { Name = "FuncThumbprintId" };

            return new ActivityFunc<string, string, string, string>
            {
                Argument1 = operationId,
                Argument2 = subscriptionId,
                Argument3 = certificateThumbprintId,
                Handler = new GetOperationStatus
                {
                    CertificateThumbprintId = new InArgument<string>(certificateThumbprintId),
                    FailBuildOnError = new InArgument<bool>(false),                     
                    IgnoreExceptions = new InArgument<bool>(false),
                    LogExceptionStack = new InArgument<bool>(false),
                    OperationId = new InArgument<string>(operationId),
                    SubscriptionId = new InArgument<string>(subscriptionId),
                    TreatWarningsAsErrors = new InArgument<bool>(false)
                }
            };
        }

        /// <summary>
        /// Create the structure of the activity for pausing between successive polling activities.
        /// </summary>
        /// <returns>An activity delegate.</returns>
        private ActivityAction<int> CreateDelayBody()
        {
            var timeout = new DelegateInArgument<int>() { Name = "FuncTimeout" };

            return new ActivityAction<int>
            {
                Argument = timeout,
                Handler = new Delay
                {
                    Duration = new InArgument<TimeSpan>(ctx => TimeSpan.FromSeconds(timeout.Get(ctx)))
                }
            };
        }

        /// <summary>
        /// Respond to the completion callback for the Operation activity.
        /// </summary>
        /// <param name="context">The activity context.</param>
        /// <param name="instance">The current instance of the activity.</param>
        /// <param name="result">The result returned by the activity at completion.</param>
        private void OnOperationCompleted(NativeActivityContext context, ActivityInstance instance, string result)
        {
            // Check to see if the operation faulted
            if (this.AzureActivityExceptionCaught.Get(context) == true)
            {
                context.ScheduleActivity(this.Failure);
                return;
            }

            // Store the results of the activity for later
            context.SetValue(this.PollingEndTime, DateTime.UtcNow.AddSeconds(this.TimeoutSeconds));
            context.SetValue(this.OperationId, result);

            // Start the process of polling for status - kind of like a do/while
            context.ScheduleFunc<string, string, string, string>(
                this.PollingBody,
                this.OperationId.Get(context),
                this.SubscriptionId.Get(context),
                this.CertificateThumbprintId.Get(context),
                new CompletionCallback<string>(this.OnGetStatusCompleted),
                new FaultCallback(this.OnOperationFault));
        }

        /// <summary>
        /// Respond to the completion callback of the status polling activity.
        /// </summary>
        /// <param name="context">The activity context.</param>
        /// <param name="instance">The current instance of the activity.</param>
        /// <param name="result">The result of the status inquiry.</param>
        private void OnGetStatusCompleted(NativeActivityContext context, ActivityInstance instance, string result)
        {
            // Check to see if the operation faulted
            if (this.AzureActivityExceptionCaught.Get(context) == true)
            {
                context.ScheduleActivity(this.Failure);
                return;
            }

            // Determine what to do based on the status of the Azure operation.
            switch (result)
            {
                case OperationState.Succeeded:
                    context.ScheduleActivity(this.Success);
                    break;

                case OperationState.Failed:
                    context.ScheduleActivity(this.Failure);
                    break;

                case OperationState.InProgress:
                    // Test to see if we are within the timeout
                    if (context.GetValue(this.PollingEndTime).CompareTo(DateTime.UtcNow) <= 0)
                    {
                        context.ScheduleActivity(this.Failure);
                    }

                    // Otherwise delay for the requested interval
                    context.ScheduleAction<int>(
                        this.DelayBody, 
                        this.PollingInterval,
                        new CompletionCallback(this.OnDelayCompleted));
                    break;
            }            
        }

        /// <summary>
        /// Respond to the completion callback for the Delay activity.
        /// </summary>
        /// <param name="context">The activity context.</param>
        /// <param name="instance">The current instance of the activity.</param>
        private void OnDelayCompleted(NativeActivityContext context, ActivityInstance instance)
        {
            // Poll again
            context.ScheduleFunc<string, string, string, string>(
                this.PollingBody,
                this.OperationId.Get(context),
                this.SubscriptionId.Get(context),
                this.CertificateThumbprintId.Get(context),
                new CompletionCallback<string>(this.OnGetStatusCompleted),
                new FaultCallback(this.OnOperationFault));
        }

        /// <summary>
        /// Respond to the fault callback, used for all scheduled activities.
        /// </summary>
        /// <param name="context">The activity context.</param>
        /// <param name="exception">An exception which was thrown by the activity.</param>
        /// <param name="instance">The current instance of the activity.</param>
        private void OnOperationFault(NativeActivityFaultContext context, Exception exception, ActivityInstance instance)
        {
            // Mark the fault handled, or else this activity will throw and will not contine after this method returns.
            context.HandleFault();

            // TODO: Make this logging dependent on the operation configuration
            this.LogBuildError(context, string.Format("AzureAsyncOperation Fault {0} during execution of {1}\r\n{2}", exception.GetType().Name, instance.Activity.GetType().Name, exception.Message));
            this.LogBuildMessage(context, exception.StackTrace, BuildMessageImportance.High);

            // Cancel the running activity
            context.CancelChild(instance);

            // Notify that an exception has been caught
            // The CompletionCallback will be called because we handled the exception.
            // This makes a better design choice to do any scheduling or further logic there.
            this.AzureActivityExceptionCaught.Set(context, true);
        }

        /// <summary>
        /// Log an error message to the build tracking participant.
        /// </summary>
        /// <param name="context">The current activity context.</param>
        /// <param name="message">The message to log.</param>
        private void LogBuildError(NativeActivityContext context, string message)
        {            
            var record = new BuildInformationRecord<BuildError>
            {
                ParentToBuildDetail = false,
                Value = new BuildError
                {
                    Message = message
                }
            };
            context.Track(record);
        }

        /// <summary>
        /// Log a message to the build tracking participant.
        /// </summary>
        /// <param name="context">The current activity context.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="importance">The importance level of the message.</param>
        private void LogBuildMessage(NativeActivityContext context, string message, BuildMessageImportance importance)
        {
            var record = new BuildInformationRecord<BuildMessage>
            {
                ParentToBuildDetail = false,
                Value = new BuildMessage
                {
                    Importance = importance,
                    Message = message
                }
            };
            context.Track(record);
        }
    }
}