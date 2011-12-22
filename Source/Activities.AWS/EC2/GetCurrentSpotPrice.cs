//-----------------------------------------------------------------------
// <copyright file="GetCurrentSpotPrice.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.EC2
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ServiceModel;
    using Amazon.EC2.Model;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.AWS.Extended;

    /// <summary>
    /// Get the spot price for a specified instance type.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetCurrentSpotPrice : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the type of instance desired.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> InstanceType { get; set; }

        /// <summary>
        /// Gets or sets the type of product desired.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ProductDescription { get; set; }

        /// <summary>
        /// Gets or sets the current spot price.
        /// </summary>
        public OutArgument<decimal> CurrentSpotPrice { get; set; }

        /// <summary>
        /// Query the market for product spot pricing.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new DescribeSpotPriceHistoryRequest
            {
                InstanceType = new List<string> { this.InstanceType.Get(this.ActivityContext) },
                ProductDescription = new List<string> { this.ProductDescription.Get(this.ActivityContext) },
                StartTime = DateTime.Now.ToAmazonDateTime(),
            };

            try
            {
                var response = EC2Client.DescribeSpotPriceHistory(request);

                // Get the first price in the price history array
                decimal price = decimal.Parse(response.DescribeSpotPriceHistoryResult.SpotPriceHistory[0].SpotPrice);
                this.CurrentSpotPrice.Set(this.ActivityContext, price);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
            }
        }
    }
}