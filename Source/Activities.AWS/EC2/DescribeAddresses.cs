//-----------------------------------------------------------------------
// <copyright file="DescribeAddresses.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// Obtain a list of EC2 instances associated with a public IP address.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class DescribeAddresses : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the static IP address to query.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> PublicAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of instance reservations.
        /// </summary>
        public OutArgument<List<Address>> Addresses { get; set; }

        /// <summary>
        /// Connect to an AWS subscription and obtain information about instances.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new DescribeAddressesRequest
            {
                PublicIp = new List<string> { this.PublicAddress.Get(this.ActivityContext) }
            };

            try
            {
                var response = EC2Client.DescribeAddresses(request);
                this.Addresses.Set(this.ActivityContext, response.DescribeAddressesResult.Address);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
            }
        }
    }
}