//-----------------------------------------------------------------------
// <copyright file="TerminateInstances.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.EC2
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ServiceModel;
    using Amazon.EC2.Model;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Terminate an active instance from EC2.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class TerminateInstances : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the instance IDs to terminate.
        /// </summary>
        [RequiredArgument]
        public InArgument<List<string>> InstanceIds { get; set; }

        /// <summary>
        /// Gets or sets the list of instance changes.
        /// </summary>
        public OutArgument<List<InstanceStateChange>> InstanceChanges { get; set; }

        /// <summary>
        /// Terminate an active EC2 instance.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new TerminateInstancesRequest
            {
                InstanceId = this.InstanceIds.Get(this.ActivityContext)
            };

            try
            {
                var response = EC2Client.TerminateInstances(request);
                this.InstanceChanges.Set(this.ActivityContext, response.TerminateInstancesResult.TerminatingInstance);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
            }
        }
    }
}