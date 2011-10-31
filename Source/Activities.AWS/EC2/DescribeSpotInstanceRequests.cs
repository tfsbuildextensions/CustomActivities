//-----------------------------------------------------------------------
// <copyright file="DescribeSpotInstanceRequests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// Query the status of a series of spot instance requests.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class DescribeSpotInstanceRequests : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the list of spot requests to obtain information for.
        /// </summary>
        [RequiredArgument]
        public InArgument<List<SpotInstanceRequest>> SpotRequests { get; set; }

        /// <summary>
        /// Gets or sets the list of status information items.
        /// </summary>
        public OutArgument<List<SpotInstanceRequest>> UpdatedSpotRequests { get; set; }

        /// <summary>
        /// Query EC2 for spot request information.
        /// </summary>
        protected override void AmazonExecute()
        {
            // Get a list of requests to monitor
            var requestIds = new List<string>();
            foreach (var spotRequest in this.SpotRequests.Get(this.ActivityContext))
            {
                requestIds.Add(spotRequest.SpotInstanceRequestId);
            }

            // Create a monitoring request
            var request = new DescribeSpotInstanceRequestsRequest
            {
                SpotInstanceRequestId = requestIds
            };

            try
            {
                var response = EC2Client.DescribeSpotInstanceRequests(request);

                this.UpdatedSpotRequests.Set(this.ActivityContext, response.DescribeSpotInstanceRequestsResult.SpotInstanceRequest);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
            }
        }
    }
}