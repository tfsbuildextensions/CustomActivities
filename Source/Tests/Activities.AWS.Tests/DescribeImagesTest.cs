//-----------------------------------------------------------------------
// <copyright file="DescribeImagesTest.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Amazon.EC2.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.AWS.EC2;

    [TestClass]
    public class DescribeImagesTest
    {
        [TestMethod]
        public void DescribeImages_Owner_ImageList()
        {
            // Arrange           
            var target = new DescribeImages
            {
                AccessKey = "",
                SecretKey = "",
                Owner = ""
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(target, new Dictionary<string, object>());
            var images = results["Images"] as List<Image>;

            // Assert
            Assert.IsNotNull(images);
        }
    }
}
