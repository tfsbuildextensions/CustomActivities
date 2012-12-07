//-----------------------------------------------------------------------
// <copyright file="GetAffinityGroups.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.AffinityGroups
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get a list all of the affinity groups that are associated to a subscription.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetAffinityGroups : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the location list.
        /// </summary>
        public OutArgument<AffinityGroupList> AffinityGroups { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain a list of affinity groups.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                AffinityGroupList groups = this.RetryCall(s => this.Channel.ListAffinityGroups(s));
                this.AffinityGroups.Set(this.ActivityContext, groups);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                this.AffinityGroups.Set(this.ActivityContext, null);
            }
        }
    }
}