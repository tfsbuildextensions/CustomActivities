//-----------------------------------------------------------------------
// <copyright file="BaseAmazonActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS
{
    using System;
    using System.Activities;
    using Amazon;
    using Amazon.EC2;

    /// <summary>
    /// Provide the base activity arguments and channel setup for interacting with AWS.
    /// </summary>
    public abstract class BaseAmazonActivity : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the Amazon account access key.
        /// </summary>
        public InArgument<string> AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the Amazon account secret key.
        /// </summary>
        public InArgument<string> SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the Amazon web services client
        /// </summary>
        protected internal AmazonEC2 EC2Client { get; set; }

        /// <summary>
        /// Prevent inheritance of the method.  Bind required parameters.
        /// </summary>
        protected sealed override void InternalExecute()
        {
            // Setup the WCF channel to the EC2 computing environment
            if (this.EC2Client == null)
            {
                this.EC2Client = AWSClientFactory.CreateAmazonEC2Client(this.AccessKey.Get(this.ActivityContext), this.SecretKey.Get(this.ActivityContext));
            }

            this.AmazonExecute();
        }

        /// <summary>
        /// AmazonExecute method which Amazon-specific activities should implement
        /// </summary>
        protected abstract void AmazonExecute();
    }
}
