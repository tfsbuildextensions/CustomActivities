//-----------------------------------------------------------------------
// <copyright file="DescribeImages.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// Discover the AMI images associated with an owner.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class DescribeImages : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the AMI owner to find images for.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Owner { get; set; }

        /// <summary>
        /// Gets or sets the list of owned images.
        /// </summary>
        public OutArgument<List<Image>> Images { get; set; }

        /// <summary>
        /// Query EC2 for the list of owned AMIs.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new DescribeImagesRequest
            {
                Owner = new List<string> { this.Owner.Get(this.ActivityContext) }
            };

            try
            {
                var response = EC2Client.DescribeImages(request);
                this.Images.Set(this.ActivityContext, response.DescribeImagesResult.Image);
            }
            catch (EndpointNotFoundException ex)
            {
                LogBuildMessage(ex.Message);
            }
        }
    }
}