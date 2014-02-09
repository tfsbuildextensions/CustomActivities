//-----------------------------------------------------------------------
// <copyright file="XmlFile.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// Adapted from code in the MSBuild Extension Pack: http://msbuildextensionpack.codeplex.com/
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Xml
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Possible actions for the Xml activity.
    /// </summary>
    public enum XmlFileAction
    {
        /// <summary>
        /// Add an attribute to the specified element.
        /// </summary>
        AddAttribute = 0,

        /// <summary>
        /// Add an element to the document.
        /// </summary>
        AddElement,

        /// <summary>
        /// Read an attribute from an element.
        /// </summary>
        ReadAttribute,

        /// <summary>
        /// Read the text of an element.
        /// </summary>
        ReadElementText,

        /// <summary>
        /// Read the XML of an element.
        /// </summary>
        ReadElementXml,

        /// <summary>
        /// Remove an attribute from an element.
        /// </summary>
        RemoveAttribute,

        /// <summary>
        /// Remove an element from the document.
        /// </summary>
        RemoveElement,

        /// <summary>
        /// Update an element's attributes.
        /// </summary>
        UpdateAttribute,

        /// <summary>
        /// Update the content of an element.
        /// </summary>
        UpdateElement
    }

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>AddAttribute</i> (<b>Required: </b>File, Element or XPath, Key, Value <b>Optional:</b> Namespaces, RetryCount)</para>
    /// <para><i>AddElement</i> (<b>Required: </b>File, Element and ParentElement or Element and XPath, <b>Optional:</b> Prefix, Key, Value, Namespaces, RetryCount, InnerText, InnerXml, InsertBeforeXPath / InsertAfterXPath)</para>
    /// <para><i>ReadAttribute</i> (<b>Required: </b>File, XPath <b>Optional:</b> Namespaces <b>Output:</b> Value)</para>
    /// <para><i>ReadElementText</i> (<b>Required: </b>File, XPath <b>Optional:</b> Namespaces <b>Output:</b> Value)</para>
    /// <para><i>ReadElementXml</i> (<b>Required: </b>File, XPath <b>Optional:</b> Namespaces <b>Output:</b> Value)</para>
    /// <para><i>RemoveAttribute</i> (<b>Required: </b>File, Key, Element or XPath <b>Optional:</b> Namespaces, RetryCount)</para>
    /// <para><i>RemoveElement</i> (<b>Required: </b>File, Element and ParentElement or Element and XPath <b>Optional:</b> Namespaces, RetryCount)</para>
    /// <para><i>UpdateAttribute</i> (<b>Required: </b>File, XPath <b>Optional:</b> Namespaces, Key, Value, RetryCount)</para>
    /// <para><i>UpdateElement</i> (<b>Required: </b>File, XPath <b>Optional:</b> Namespaces, InnerText, InnerXml, RetryCount)</para>
    /// </summary>
    public class XmlFile : BaseCodeActivity
    {
        /// <summary>
        /// The action to perform
        /// </summary>
        private XmlFileAction action = XmlFileAction.ReadElementXml;

        private XmlDocument xmlFileDoc;
        private XmlNamespaceManager namespaceManager;
        private XmlNodeList elements;
        private int retryCount = 5;

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public XmlFileAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Sets a value indicating how many times to retry saving the file, e.g. if files are temporarily locked. Default is 5. The retry occurs every 5 seconds.
        /// </summary>
        public int RetryCount
        {
            get { return this.retryCount; }
            set { this.retryCount = value; }
        }

        /// <summary>
        /// Sets the element. For AddElement, if the element exists, it's InnerText / InnerXml will be updated
        /// </summary>
        public InArgument<string> Element { get; set; }

        /// <summary>
        /// Sets the InnerText.
        /// </summary>
        public InArgument<string> InnerText { get; set; }

        /// <summary>
        /// Sets the InnerXml.
        /// </summary>
        public InArgument<string> InnerXml { get; set; }

        /// <summary>
        /// Sets the Prefix used for an added element, prefix must exists in Namespaces.
        /// </summary>
        public InArgument<string> Prefix { get; set; }

        /// <summary>
        /// Sets the parent element.
        /// </summary>
        public InArgument<string> ParentElement { get; set; }

        /// <summary>
        /// Sets the Attribute key.
        /// </summary>
        public InArgument<string> Key { get; set; }

        /// <summary>
        /// Sets the file.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> File { get; set; }

        /// <summary>
        /// Specifies the XPath to be used
        /// </summary>
        public InArgument<string> XPath { get; set; }

        /// <summary>
        /// Specifies the XPath to be used to control where a new element is added. The Xpath must resolve to single node.
        /// </summary>
        public InArgument<string> InsertBeforeXPath { get; set; }

        /// <summary>
        /// Specifies the XPath to be used to control where a new element is added. The Xpath must resolve to single node.
        /// </summary>
        public InArgument<string> InsertAfterXPath { get; set; }

        /// <summary>
        /// Dictionary specifiying "Prefix" as key and "Uri" as value for use with the specified XPath
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Common workflow pattern.")]
        public InArgument<IDictionary<string, string>> Namespaces { get; set; }

        /// <summary>
        /// Gets or Sets the Attribute key value. Also stores the result of any Read TaskActions
        /// </summary>
        public InOutArgument<string> Value { get; set; }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Keep the code similar to its parent.")]
        protected override void InternalExecute()
        {
            if (!System.IO.File.Exists(this.File.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "File not found: {0}", this.File.Get(this.ActivityContext)));
                return;
            }

            this.xmlFileDoc = new XmlDocument();
            try
            {
                this.xmlFileDoc.Load(this.File.Get(this.ActivityContext));
            }
            catch (Exception ex)
            {
                this.LogBuildWarning(ex.Message);
                bool loaded = false;
                int count = 1;
                while (!loaded && count <= this.RetryCount)
                {
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Load failed, trying again in 5 seconds. Attempt {0} of {1}", count, this.RetryCount), BuildMessageImportance.High);
                    System.Threading.Thread.Sleep(5000);
                    count++;
                    try
                    {
                        this.xmlFileDoc.Load(this.File.Get(this.ActivityContext));
                        loaded = true;
                    }
                    catch
                    {
                        this.LogBuildWarning(ex.Message);
                    }
                }

                if (loaded != true)
                {
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.namespaceManager = this.GetNamespaceManagerForDoc();
                this.elements = this.xmlFileDoc.SelectNodes(this.XPath.Get(this.ActivityContext), this.namespaceManager);
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "XmlFile: {0}", this.File.Get(this.ActivityContext)), BuildMessageImportance.Low);
            switch (this.Action)
            {
                case XmlFileAction.AddElement:
                    this.AddElement();
                    break;
                case XmlFileAction.AddAttribute:
                    this.AddAttribute();
                    break;
                case XmlFileAction.ReadAttribute:
                    this.ReadAttribute();
                    break;
                case XmlFileAction.ReadElementText:
                case XmlFileAction.ReadElementXml:
                    this.ReadElement();
                    break;
                case XmlFileAction.RemoveAttribute:
                    this.RemoveAttribute();
                    break;
                case XmlFileAction.RemoveElement:
                    this.RemoveElement();
                    break;
                case XmlFileAction.UpdateElement:
                    this.UpdateElement();
                    break;
                case XmlFileAction.UpdateAttribute:
                    this.UpdateAttribute();
                    break;
                default:
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.Action));
                    return;
            }
        }

        private void ReadElement()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildError("XPath is Required");
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Read Element: {0}", this.XPath.Get(this.ActivityContext)));
            XmlNode node = this.xmlFileDoc.SelectSingleNode(this.XPath.Get(this.ActivityContext), this.namespaceManager);
            if (node != null && node.NodeType == XmlNodeType.Element)
            {
                this.Value.Set(this.ActivityContext, this.Action == XmlFileAction.ReadElementText ? node.InnerText : node.InnerXml);
            }
        }

        private void ReadAttribute()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildError("XPath is Required");
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Read Attribute: {0}", this.XPath.Get(this.ActivityContext)), BuildMessageImportance.Low);
            XmlNode node = this.xmlFileDoc.SelectSingleNode(this.XPath.Get(this.ActivityContext), this.namespaceManager);
            if (node != null && node.NodeType == XmlNodeType.Attribute)
            {
                this.Value.Set(this.ActivityContext, node.Value);
            }
        }

        private void UpdateElement()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildError("XPath is Required");
                return;
            }

            if (string.IsNullOrEmpty(this.InnerXml.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Update Element: {0}. InnerText: {1}", this.XPath.Get(this.ActivityContext), this.InnerText.Get(this.ActivityContext)), BuildMessageImportance.Low);
                if (this.elements != null && this.elements.Count > 0)
                {
                    foreach (XmlNode element in this.elements)
                    {
                        element.InnerText = this.InnerText.Get(this.ActivityContext);
                    }

                    this.TrySave();
                }

                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Update Element: {0}. InnerXml: {1}", this.XPath.Get(this.ActivityContext), this.InnerXml.Get(this.ActivityContext)), BuildMessageImportance.Low);
            if (this.elements != null && this.elements.Count > 0)
            {
                foreach (XmlNode element in this.elements)
                {
                    element.InnerXml = this.InnerXml.Get(this.ActivityContext);
                }

                this.TrySave();
            }
        }

        private void UpdateAttribute()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildError("XPath is Required");
                return;
            }

            if (string.IsNullOrEmpty(this.Key.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Update Attribute: {0}. Value: {1}", this.XPath.Get(this.ActivityContext), this.Value.Get(this.ActivityContext)), BuildMessageImportance.Low);
                XmlNode node = this.xmlFileDoc.SelectSingleNode(this.XPath.Get(this.ActivityContext), this.namespaceManager);
                if (node != null && node.NodeType == XmlNodeType.Attribute)
                {
                    node.Value = this.Value.Get(this.ActivityContext);
                }
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Update Attribute: {0} @ {1}. Value: {2}", this.Key.Get(this.ActivityContext), this.XPath.Get(this.ActivityContext), this.Value.Get(this.ActivityContext)), BuildMessageImportance.Low);
                if (this.elements != null && this.elements.Count > 0)
                {
                    foreach (XmlNode element in this.elements)
                    {
                        XmlAttribute attNode = element.Attributes.GetNamedItem(this.Key.Get(this.ActivityContext)) as XmlAttribute;
                        if (attNode != null)
                        {
                            attNode.Value = this.Value.Get(this.ActivityContext);
                        }
                    }
                }
            }

            this.TrySave();
        }

        private void RemoveAttribute()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Remove Attribute: {0}", this.Key.Get(this.ActivityContext)), BuildMessageImportance.Low);
                XmlNode elementNode = this.xmlFileDoc.SelectSingleNode(this.Element.Get(this.ActivityContext));
                if (elementNode == null)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentUICulture, "Element not found: {0}", this.Element.Get(this.ActivityContext)));
                    return;
                }

                XmlAttribute attNode = elementNode.Attributes.GetNamedItem(this.Key.Get(this.ActivityContext)) as XmlAttribute;
                if (attNode != null)
                {
                    elementNode.Attributes.Remove(attNode);
                    this.TrySave();
                }
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Remove Attribute: {0}", this.Key.Get(this.ActivityContext)), BuildMessageImportance.Low);
                if (this.elements != null && this.elements.Count > 0)
                {
                    foreach (XmlNode element in this.elements)
                    {
                        XmlAttribute attNode = element.Attributes.GetNamedItem(this.Key.Get(this.ActivityContext)) as XmlAttribute;
                        if (attNode != null)
                        {
                            element.Attributes.Remove(attNode);
                            this.TrySave();
                        }
                    }
                }
            }
        }

        private void AddAttribute()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Set Attribute: {0}={1}", this.Key.Get(this.ActivityContext), this.Value.Get(this.ActivityContext)), BuildMessageImportance.Low);
                XmlNode elementNode = this.xmlFileDoc.SelectSingleNode(this.Element.Get(this.ActivityContext));
                if (elementNode == null)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentUICulture, "Element not found: {0}", this.Element.Get(this.ActivityContext)));
                    return;
                }

                XmlAttribute attNode = elementNode.Attributes.GetNamedItem(this.Key.Get(this.ActivityContext)) as XmlAttribute;
                if (attNode == null)
                {
                    attNode = this.xmlFileDoc.CreateAttribute(this.Key.Get(this.ActivityContext));
                    attNode.Value = this.Value.Get(this.ActivityContext);
                    elementNode.Attributes.Append(attNode);
                }
                else
                {
                    attNode.Value = this.Value.Get(this.ActivityContext);
                }

                this.TrySave();
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Set Attribute: {0}={1}", this.Key.Get(this.ActivityContext), this.Value.Get(this.ActivityContext)), BuildMessageImportance.Low);
                if (this.elements != null && this.elements.Count > 0)
                {
                    foreach (XmlNode element in this.elements)
                    {
                        XmlNode attrib = element.Attributes[this.Key.Get(this.ActivityContext)] ?? element.Attributes.Append(this.xmlFileDoc.CreateAttribute(this.Key.Get(this.ActivityContext)));
                        attrib.Value = this.Value.Get(this.ActivityContext);
                    }

                    this.TrySave();
                }
            }
        }

        private XmlNamespaceManager GetNamespaceManagerForDoc()
        {
            XmlNamespaceManager localnamespaceManager = new XmlNamespaceManager(this.xmlFileDoc.NameTable);

            // If we have had namespace declarations specified add them to the Namespace Mgr for the XML Document.
            var namespaces = this.Namespaces.Get(this.ActivityContext);
            if (namespaces != null && namespaces.Any())
            {
                foreach (var key in namespaces.Keys)
                {
                    localnamespaceManager.AddNamespace(key, namespaces[key]);
                }
            }

            return localnamespaceManager;
        }

        private void AddElement()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Add Element: {0}", this.Element.Get(this.ActivityContext)), BuildMessageImportance.Low);
                XmlNode parentNode = this.xmlFileDoc.SelectSingleNode(this.ParentElement.Get(this.ActivityContext));
                if (parentNode == null)
                {
                    this.LogBuildError("ParentElement not found: " + this.ParentElement.Get(this.ActivityContext));
                    return;
                }

                // Ensure node does not already exist
                XmlNode newNode = this.xmlFileDoc.SelectSingleNode(this.ParentElement.Get(this.ActivityContext) + "/" + this.Element.Get(this.ActivityContext));
                if (newNode == null)
                {
                    newNode = this.CreateElement();

                    if (!string.IsNullOrEmpty(this.Key.Get(this.ActivityContext)))
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Add Attribute: {0} to: {1}", this.Key.Get(this.ActivityContext), this.Element.Get(this.ActivityContext)), BuildMessageImportance.Low);

                        XmlAttribute attNode = this.xmlFileDoc.CreateAttribute(this.Key.Get(this.ActivityContext));
                        attNode.Value = this.Value.Get(this.ActivityContext);
                        newNode.Attributes.Append(attNode);
                    }

                    if (string.IsNullOrEmpty(this.InsertAfterXPath.Get(this.ActivityContext)) && string.IsNullOrEmpty(this.InsertBeforeXPath.Get(this.ActivityContext)))
                    {
                        parentNode.AppendChild(newNode);
                    }
                    else if (!string.IsNullOrEmpty(this.InsertAfterXPath.Get(this.ActivityContext)))
                    {
                        parentNode.InsertAfter(newNode, parentNode.SelectSingleNode(this.InsertAfterXPath.Get(this.ActivityContext)));
                    }
                    else if (!string.IsNullOrEmpty(this.InsertBeforeXPath.Get(this.ActivityContext)))
                    {
                        parentNode.InsertBefore(newNode, parentNode.SelectSingleNode(this.InsertBeforeXPath.Get(this.ActivityContext)));
                    }

                    this.TrySave();
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.InnerText.Get(this.ActivityContext)))
                    {
                        newNode.InnerText = this.InnerText.Get(this.ActivityContext);
                    }
                    else if (!string.IsNullOrEmpty(this.InnerXml.Get(this.ActivityContext)))
                    {
                        newNode.InnerXml = this.InnerXml.Get(this.ActivityContext);
                    }

                    this.TrySave();
                }
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Add Element: {0}", this.XPath.Get(this.ActivityContext)), BuildMessageImportance.Low);
                if (this.elements != null && this.elements.Count > 0)
                {
                    foreach (XmlNode element in this.elements)
                    {
                        XmlNode newNode = this.CreateElement();

                        if (!string.IsNullOrEmpty(this.Key.Get(this.ActivityContext)))
                        {
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Add Attribute: {0} to: {1}", this.Key.Get(this.ActivityContext), this.Element.Get(this.ActivityContext)), BuildMessageImportance.Low);

                            XmlAttribute attNode = this.xmlFileDoc.CreateAttribute(this.Key.Get(this.ActivityContext));
                            attNode.Value = this.Value.Get(this.ActivityContext);
                            newNode.Attributes.Append(attNode);
                        }

                        element.AppendChild(newNode);
                    }

                    this.TrySave();
                }
            }
        }

        private XmlNode CreateElement()
        {
            XmlNode newNode;
            if (string.IsNullOrEmpty(this.Prefix.Get(this.ActivityContext)))
            {
                newNode = this.xmlFileDoc.CreateElement(this.Element.Get(this.ActivityContext), this.xmlFileDoc.DocumentElement.NamespaceURI);
            }
            else
            {
                string prefixNamespace = this.namespaceManager.LookupNamespace(this.Prefix.Get(this.ActivityContext));
                if (string.IsNullOrEmpty(prefixNamespace))
                {
                    this.LogBuildError("Prefix not defined in Namespaces in parameters: " + this.Prefix.Get(this.ActivityContext));
                    return null;
                }

                newNode = this.xmlFileDoc.CreateElement(this.Prefix.Get(this.ActivityContext), this.Element.Get(this.ActivityContext), prefixNamespace);
            }

            if (!string.IsNullOrEmpty(this.InnerText.Get(this.ActivityContext)))
            {
                newNode.InnerText = this.InnerText.Get(this.ActivityContext);
            }
            else if (!string.IsNullOrEmpty(this.InnerXml.Get(this.ActivityContext)))
            {
                newNode.InnerXml = this.InnerXml.Get(this.ActivityContext);
            }

            return newNode;
        }

        private void RemoveElement()
        {
            if (string.IsNullOrEmpty(this.XPath.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Remove Element: {0}", this.Element.Get(this.ActivityContext)), BuildMessageImportance.Low);
                XmlNode parentNode = this.xmlFileDoc.SelectSingleNode(this.ParentElement.Get(this.ActivityContext));
                if (parentNode == null)
                {
                    this.LogBuildError("ParentElement not found: " + this.ParentElement.Get(this.ActivityContext));
                    return;
                }

                XmlNode nodeToRemove = this.xmlFileDoc.SelectSingleNode(this.ParentElement.Get(this.ActivityContext) + "/" + this.Element.Get(this.ActivityContext));
                if (nodeToRemove != null)
                {
                    parentNode.RemoveChild(nodeToRemove);
                    this.TrySave();
                }
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentUICulture, "Remove Element: {0}", this.XPath.Get(this.ActivityContext)), BuildMessageImportance.Low);
                if (this.elements != null && this.elements.Count > 0)
                {
                    foreach (XmlNode element in this.elements)
                    {
                        element.ParentNode.RemoveChild(element);
                    }

                    this.TrySave();
                }
            }
        }

        private void TrySave()
        {
            string path = this.File.Get(this.ActivityContext);
            bool attributesChanged = false;

            try
            {
                if (System.IO.File.Exists(path))
                {
                    var fileAttributes = System.IO.File.GetAttributes(path);
                    if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        System.IO.File.SetAttributes(path, fileAttributes ^ FileAttributes.ReadOnly);
                        attributesChanged = true;
                    }
                }

                this.xmlFileDoc.Save(path);
                if (attributesChanged)
                {
                    System.IO.File.SetAttributes(path, FileAttributes.ReadOnly);
                }
            }
            catch (XmlException ex)
            {
                this.LogBuildWarning(ex.Message);
                bool saved = false;
                int count = 1;
                while (!saved && count <= this.RetryCount)
                {
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Save failed, trying again in 5 seconds. Attempt {0} of {1}", count, this.RetryCount), BuildMessageImportance.High);
                    System.Threading.Thread.Sleep(5000);
                    count++;
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            var fileAttributes = System.IO.File.GetAttributes(path);
                            if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                System.IO.File.SetAttributes(path, fileAttributes ^ FileAttributes.ReadOnly);
                                attributesChanged = true;
                            }
                        }

                        this.xmlFileDoc.Save(path);
                        if (attributesChanged)
                        {
                            System.IO.File.SetAttributes(path, FileAttributes.ReadOnly);
                        } 
                        
                        saved = true;
                    }
                    catch (XmlException exc)
                    {
                        this.LogBuildWarning(exc.Message);
                    }
                }

                if (saved != true)
                {
                    throw;
                }
            }
        }
    }
}