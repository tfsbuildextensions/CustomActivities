//-----------------------------------------------------------------------
// <copyright file="DescribeInstances.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// Get reservation information for an instance.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class DescribeInstances : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the instance IDs to retrieve the description of.
        /// </summary>
        [RequiredArgument]
        public InArgument<List<string>> InstanceIds { get; set; }

        /// <summary>
        /// Gets or sets the list of instance reservations.
        /// </summary>
        public OutArgument<List<Reservation>> Reservations { get; set; }

        /// <summary>
        /// Connect to an Amazon subscription and obtain information about instance reservations.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new DescribeInstancesRequest
            {
                InstanceId = this.InstanceIds.Get(this.ActivityContext)
            };

            try
            {
                var response = EC2Client.DescribeInstances(request);
                this.Reservations.Set(this.ActivityContext, response.DescribeInstancesResult.Reservation);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
            }
        }
    }
}