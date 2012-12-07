//-----------------------------------------------------------------------
// <copyright file="GetAffinityGroup.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure.AffinityGroups
{
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Get the system properties associated with the specified affinity group.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetAffinityGroup : BaseAzureActivity
    {
        /// <summary>
        /// Gets or sets the affinity group name.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> AffinityGroupName { get; set; }

        /// <summary>
        /// Gets or sets the group properties.
        /// </summary>
        public OutArgument<AffinityGroup> AffinityGroup { get; set; }

        /// <summary>
        /// Connect to an Azure subscription and obtain affinity group information.
        /// </summary>
        protected override void AzureExecute()
        {
            try
            {
                AffinityGroup group = this.RetryCall(s => this.Channel.GetAffinityGroup(s, this.AffinityGroupName.Get(this.ActivityContext)));
                this.AffinityGroup.Set(this.ActivityContext, group);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
                this.AffinityGroup.Set(this.ActivityContext, null);
            }
        }
    }
}