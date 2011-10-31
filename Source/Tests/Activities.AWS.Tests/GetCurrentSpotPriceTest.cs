//-----------------------------------------------------------------------
// <copyright file="GetCurrentSpotPriceTest.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Amazon.EC2.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.AWS.EC2;

    [TestClass]
    public class GetCurrentSpotPriceTest
    {
        [TestMethod]
        public void GetCurrentSpotPrice_SmallInstance_Price()
        {
            // Arrange           
            var target = new GetCurrentSpotPrice
            {
                AccessKey = "",
                SecretKey = "",
                InstanceType = InstanceType.M1Small,
                ProductDescription = ProductionDescriptionType.Windows
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(target, new Dictionary<string, object>());
            string spotPrice = results["CurrentSpotPrice"] as string;

            // Assert
            Assert.IsNotNull(spotPrice);
        }
    }
}
