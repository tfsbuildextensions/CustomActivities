//-----------------------------------------------------------------------
// <copyright file="DescribeInstancesTest.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Amazon.EC2.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.AWS.EC2;

    [TestClass]
    public class DescribeInstancesTest
    {
        [TestMethod]
        public void DescribeInstances_NoId_InstanceList()
        {
            // Arrange           
            var target = new DescribeInstances
            {
                AccessKey = string.Empty,
                SecretKey = string.Empty
            };

            // Act
            IDictionary<string, object> results = WorkflowInvoker.Invoke(target, new Dictionary<string, object>());
            List<Reservation> reservation = results["Reservations"] as List<Reservation>;

            // Assert
            Assert.IsNotNull(reservation);
        }
    }
}
