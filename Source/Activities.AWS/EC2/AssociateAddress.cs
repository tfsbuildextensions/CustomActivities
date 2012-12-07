//-----------------------------------------------------------------------
// <copyright file="AssociateAddress.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    /// Activity to associate an public IP address with an EC2 instance.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class AssociateAddress : BaseAmazonActivity
    {
        /// <summary>
        /// Gets or sets the instance identifier.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the static IP address to associate.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> PublicAddress { get; set; }

        /// <summary>
        /// Connect to an EC2 instance and associate a public IP address with it.
        /// </summary>
        protected override void AmazonExecute()
        {
            var request = new AssociateAddressRequest
            {
                InstanceId = this.InstanceId.Get(this.ActivityContext),
                PublicIp = this.PublicAddress.Get(this.ActivityContext)
            };

            try
            {
                EC2Client.AssociateAddress(request);
            }
            catch (EndpointNotFoundException ex)
            {
                this.LogBuildMessage(ex.Message);
            }
        }
    }
}