//-----------------------------------------------------------------------
// <copyright file="GetValueFromRegistry.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Framework.Client;

    /// <summary>
    /// Activity to read 
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [ActivityTracking(ActivityTrackingOption.ActivityOnly)]
    public class GetValueFromRegistry : BaseCodeActivity<string>
    {
        /// <summary>
        /// Gets or sets the default value if the key is 
        /// not registered
        /// </summary>
        [Description("Default value to be used if the path is not defined")]
        [Category("Registry")]
        public InArgument<string> DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the path of the registration entry we
        /// want to read
        /// </summary>
        [RequiredArgument()]
        [Description("Reads the specified path from the registry (first will try the collection registry and if not defined the configuration registry")]
        [Category("Registry")]
        public InArgument<string> Path { get; set; }

        /// <summary>
        /// Reads the value from he registry. First we will read the collection 
        /// registry and if no value is found we will fallback to reading the
        /// configuration registry.
        /// If no value is found we log an error (so the activity may fail if
        /// FailBuildOnError is set to true
        /// </summary>
        /// <returns>The key value from the registry</returns>
        protected override string InternalExecute()
        {
            string value;
            var path = this.Path.Get(this.ActivityContext);
            var defaultValue = this.DefaultValue.Get(this.ActivityContext);

            var tfs = this.ActivityContext.GetExtension<TfsTeamProjectCollection>();
            var service = tfs.GetService<ITeamFoundationRegistry>();

            value = service.GetValue<string>(path);

            // If not found on the collection than try to read if from the configuration registry
            if (value == null)
            {
                // If the user doesn't has permissions than the call
                // will return nothing (even if the value exists). It doesn't fail with an exception
                // it just fails to return anything
                service = tfs.ConfigurationServer.GetService<ITeamFoundationRegistry>();
                value = service.GetValue<string>(path, defaultValue);
            }

            if (value == null)
            {
                this.LogBuildError(String.Format("Can't find value for path {0} and no default value has been set. Make sure the value exists and/or the user {1} has permissions to read it.", path, tfs.AuthorizedIdentity.DisplayName));
            }

            return value;
        }
    }
}
