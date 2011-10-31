using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Build.Workflow.Tracking;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;


namespace WorkflowUtils
{

    public sealed class CustLogMessage : CodeActivity
    {
        // Define an activity input argument of type string
        public InArgument<string> CustMsg { get; set; }
        public CustLogMessage(string logMsg)
        {
            CustMsg = logMsg;
        }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string CustMsg = context.GetValue(this.CustMsg);
            context.Track(new BuildInformationRecord<BuildMessage>()
            {
                Value = new BuildMessage()
                {
                    Importance = BuildMessageImportance.Normal,
                    Message = CustMsg,
                }
            });
        }
    }
}
