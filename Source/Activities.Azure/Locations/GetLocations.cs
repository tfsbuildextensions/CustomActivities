//-----------------------------------------------------------------------
// <copyright file="GetLocations.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.Locations
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get a list all of the data center locations that are valid for a subscription.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetLocations : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the location list.
        /// </summary>
        public OutArgument<LocationList> Locations { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a list of locations.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                LocationList locations = this.RetryCall(s => this.Channel.ListLocations(s));
                this.Locations.Set(this.ActivityContext, locations);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
                this.Locations.Set(this.ActivityContext, null);
            }
        }
    }
}