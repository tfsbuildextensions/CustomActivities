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

    /// <summary>
    /// Activity used to add a parameter to NAnt
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public class AddNAntProperty : BaseCodeActivity
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<ExecutionParameters> Parameters { get; set; }

        /// <summary>
        /// PropertyName
        /// </summary>
        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<string> PropertyName { get; set; }

        /// <summary>
        /// PropertyValue
        /// </summary>
        [Category("Parameters")]
        [RequiredArgument]
        public InArgument<string> PropertyValue { get; set; }

        /// <summary>
        /// InternalExecute method which activities should implement
        /// </summary>
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
