//-----------------------------------------------------------------------
// <copyright file="XmlFileTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.Xml
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Xml;

    [TestClass]
    public class XmlFileTests
    {
        [TestMethod]
        public void XmlFile_ReadElementText()
        {
            // Arrange
            var target = new TfsBuildExtensions.Activities.Xml.XmlFile { Action = XmlFileAction.ReadElementText };

            // Define activity arguments
            var arguments = new Dictionary<string, object>
            {
                { "File", "..\\..\\..\\Tests\\Activities.Tests\\Xml\\book.xml" },
                { "XPath", "//title" },
            };

            // Act
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var result = invoker.Invoke(arguments);

            // Assert
            string value = (string)result["Value"];
            Assert.AreEqual("XML Developer's Guide", value);
        }

        [TestMethod]
        public void XmlFile_UpdateElement()
        {
            // Arrange
            var target = new TfsBuildExtensions.Activities.Xml.XmlFile { Action = XmlFileAction.UpdateElement };

            // Define activity arguments
            var arguments = new Dictionary<string, object>
            {
                { "File", "..\\..\\..\\Tests\\Activities.Tests\\Xml\\book.xml" },
                { "XPath", "//title" },
                { "InnerText", "New Title" },
            };

            // Act
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var result = invoker.Invoke(arguments);

            // Assert
        }
    }
}
