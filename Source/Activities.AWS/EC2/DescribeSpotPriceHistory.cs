//-----------------------------------------------------------------------
// <copyright file="DescribeSpotPriceHistory.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// Get spot pricing history.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class DescribeSpotPriceHistory : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the type of instance desired.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> InstanceType { get; set; }

        /// <summary>
        /// Gets or sets the start date of the request.
        /// </summary>
        public InArgument<DateTime> StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end date of the request.
        /// </summary>
        public InArgument<DateTime> EndTime { get; set; }

        /// <summary>
        /// Gets or sets the spot price history.
        /// </summary>
        public OutArgument<List<SpotPriceHistory>> PriceHistory { get; set; }

        /// <summary>
        /// Query EC2 for the spot pricing history for a specified instance type.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new DescribeSpotPriceHistoryRequest
            {
                InstanceType = new List<string> { this.InstanceType.Get(this.ActivityContext) },
                StartTime = this.StartTime.Get(this.ActivityContext).ToAmazonDateTime(),
                EndTime = this.EndTime.Get(this.ActivityContext).ToAmazonDateTime()
            };

            try
            {
                var response = EC2Client.DescribeSpotPriceHistory(request);
                this.PriceHistory.Set(this.ActivityContext, response.DescribeSpotPriceHistoryResult.SpotPriceHistory);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
            }
        }
    }
}