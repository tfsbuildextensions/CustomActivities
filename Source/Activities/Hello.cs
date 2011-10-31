//-----------------------------------------------------------------------
// <copyright file="Hello.cs"> Copy as you feel fit! </copyright>
//-----------------------------------------------------------------------
namespace CodeActivitySamples
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;

    /// <summary>
    /// Hello sample
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Hello : CodeActivity<string>
    {
        /// <summary>
        /// Message
        /// </summary>
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// Message2
        /// </summary>
        public InOutArgument<string> Message2 { get; set; }

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>string</returns>
        protected override string Execute(CodeActivityContext context)
        {
            // Get the message
            string myMessage = "Hello " + this.Message.Get(context);

            // Get the second message
            myMessage += this.Message2.Get(context);

            // Get the current BuildNumber using the GetExtension method
            string tfsBuildNumber = context.GetExtension<IBuildDetail>().BuildNumber;

            // add the build number to the message
            myMessage += " from " + tfsBuildNumber;

            // log it to the build output
            context.TrackBuildMessage(myMessage, BuildMessageImportance.High);

            // return the message
            return myMessage;
        }
    }
}