//-----------------------------------------------------------------------
// <copyright file="AddNantProperty.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using TfsBuildExtensions.Activities;

    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public class AddNAntProperty : BaseCodeActivity
    {
        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<ExecutionParameters> Parameters { get; set; }

        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<string> PropertyName { get; set; }

        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<string> PropertyValue { get; set; }

        protected override void InternalExecute()
        {
            var parameters = this.Parameters.Get(this.ActivityContext);
            var propertyName = this.PropertyName.Get(this.ActivityContext);
            var propertyValue = this.PropertyValue.Get(this.ActivityContext);

            if (parameters == null)
            {
                this.LogBuildError("Invalid Parameters");
                return;
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                this.LogBuildError("Property name is empty");
                return;
            }

            propertyName = propertyName.Trim();

            parameters.Properties[propertyName] = propertyValue;
        }
    }
}
