//-----------------------------------------------------------------------
// <copyright file="Xml.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Possible actions for the Xml activity.
    /// </summary>
    public enum XmlAction
    {
        /// <summary>
        /// Reboots the deployment defined by the unique deployment name.
        /// </summary>
        Validate = 0,

        /// <summary>
        /// Reboots the deployment defined by the service name and slot.
        /// </summary>
        Transform
    }

    /// <summary>
    /// Process an XML file.
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Transform</i> (<b>Required: </b>XmlText or XmlFile, XslTransform or XslTransformFile <b>Optional:</b> Conformance, Indent, OmitXmlDeclaration, OutputFile, TextEncoding, EnableDocumentFunction <b>Output: </b>Output)</para>
    /// <para><i>Validate</i> (<b>Required: </b>XmlText or XmlFile, SchemaFiles <b>Optional: </b> TargetNamespace <b>Output: </b>IsValid, Output)</para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [System.ComponentModel.Description("Activity to interact with XML files.")]
    public class Xml : BaseCodeActivity
    {
        /// <summary>
        /// The action to perform
        /// </summary>
        private XmlAction action = XmlAction.Validate;

        private string targetNamespace = string.Empty;
        private XDocument xmlDoc;
        private Encoding fileEncoding = Encoding.UTF8;
        private ConformanceLevel conformanceLevel;

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public XmlAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The TargetNamespace for Validate. Default is ""
        /// </summary>
        public string TargetNamespace
        {
            get { return this.targetNamespace; }
            set { this.targetNamespace = value; }
        }

        /// <summary>
        /// Set the Encoding option for TransForm. Default is utf-8. This maps to the Encoding.WebName property
        /// </summary>
        public string TextEncoding
        {
            get { return this.fileEncoding.WebName; }
            set { this.fileEncoding = System.Text.Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// Sets the ConformanceLevel. Supports Auto, Document and Fragment. Default is ConformanceLevel.Document
        /// </summary>
        public string Conformance
        {
            get { return this.conformanceLevel.ToString(); }
            set { this.conformanceLevel = (ConformanceLevel)Enum.Parse(typeof(ConformanceLevel), value); }
        }

        /// <summary>
        /// Gets or sets the XmlFile
        /// </summary>
        public InArgument<string> XmlFilePath { get; set; }

        /// <summary>
        /// Gets or sets the XslTransformFile
        /// </summary>
        public InArgument<string> XslTransformFile { get; set; }

        /// <summary>
        /// Gets or sets the Xml
        /// </summary>
        public InArgument<string> XmlText { get; set; }

        /// <summary>
        /// Gets or sets the XslTransform
        /// </summary>
        public InArgument<string> XslTransform { get; set; }

        /// <summary>
        /// Gets or sets the OutputFile
        /// </summary>
        public InArgument<string> OutputFile { get; set; }

        /// <summary>
        /// Sets the Schema Files collection
        /// </summary>
        public InArgument<IEnumerable<string>> SchemaFiles { get; set; }

        /// <summary>
        /// Set the OmitXmlDeclaration option for TransForm. Default is False
        /// </summary>
        public InArgument<bool> OmitXmlDeclaration { get; set; }

        /// <summary>
        /// Set the Indent option for TransForm. Default is False
        /// </summary>
        public InArgument<bool> Indent { get; set; }

        /// <summary>
        /// Set the EnableDocumentFunction option for TransForm. Default is False
        /// </summary>
        public InArgument<bool> EnableDocumentFunction { get; set; }

        /// <summary>
        /// Gets whether an XmlFile is valid xml
        /// </summary>
        public OutArgument<bool> IsValid { get; set; }

        /// <summary>
        /// Get the Output
        /// </summary>
        public OutArgument<string> Output { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            // Check to make sure our xml file exists
            if (!string.IsNullOrEmpty(this.XmlFilePath.Get(this.ActivityContext)) && !File.Exists(this.XmlFilePath.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "XmlFile not found: {0}", this.XmlFilePath.Get(this.ActivityContext)));
                return;
            }

            // Load the Xml
            if (!string.IsNullOrEmpty(this.XmlFilePath.Get(this.ActivityContext)))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Loading XmlFile: {0}", this.XmlFilePath.Get(this.ActivityContext)), BuildMessageImportance.Low);
                this.xmlDoc = XDocument.Load(this.XmlFilePath.Get(this.ActivityContext));
            }
            else if (!string.IsNullOrEmpty(this.XmlText.Get(this.ActivityContext)))
            {
                // Load the Xml
                this.LogBuildMessage("Loading Xml", BuildMessageImportance.Low);
                using (StringReader sr = new StringReader(this.XmlText.Get(this.ActivityContext)))
                {
                    this.xmlDoc = XDocument.Load(sr);
                }
            }
            else
            {
                this.LogBuildError("Xml or XmlFile must be specified");
                return;
            }

            switch (this.Action)
            {
                case XmlAction.Transform:
                    this.Transform();
                    break;
                case XmlAction.Validate:
                    this.Validate();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private void Transform()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Transforming: {0}", this.XmlFilePath.Get(this.ActivityContext)), BuildMessageImportance.Low);
            XDocument xslDoc;
            if (!string.IsNullOrEmpty(this.XslTransformFile.Get(this.ActivityContext)) && !File.Exists(this.XslTransformFile.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "XslTransformFile not found: {0}", this.XslTransformFile));
                return;
            }

            if (!string.IsNullOrEmpty(this.XslTransformFile.Get(this.ActivityContext)))
            {
                // Load the XslTransformFile
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Loading XslTransformFile: {0}", this.XslTransformFile), BuildMessageImportance.Low);
                xslDoc = XDocument.Load(this.XslTransformFile.Get(this.ActivityContext));
            }
            else if (!string.IsNullOrEmpty(this.XslTransform.Get(this.ActivityContext)))
            {
                // Load the XslTransform
                this.LogBuildMessage("Loading XslTransform", BuildMessageImportance.Low);
                using (StringReader sr = new StringReader(this.XslTransform.Get(this.ActivityContext)))
                {
                    xslDoc = XDocument.Load(sr);
                }
            }
            else
            {
                this.LogBuildError("XslTransform or XslTransformFile must be specified");
                return;
            }

            // Load the style sheet.
            XslCompiledTransform xslt = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings { EnableScript = true, EnableDocumentFunction = this.EnableDocumentFunction.Get(this.ActivityContext) };
            
            using (StringReader sr = new StringReader(xslDoc.ToString()))
            {
                xslt.Load(XmlReader.Create(sr), settings, null);
                StringBuilder builder = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(builder, xslt.OutputSettings))
                {
                    this.LogBuildMessage("Running XslTransform", BuildMessageImportance.Low);

                    // Execute the transform and output the results to a writer.
                    xslt.Transform(this.xmlDoc.CreateReader(), writer);
                }

                this.Output.Set(this.ActivityContext, builder.ToString());
            }

            if (!string.IsNullOrEmpty(this.OutputFile.Get(this.ActivityContext)))
            {
                if (xslt.OutputSettings.OutputMethod == XmlOutputMethod.Text)
                {
                    this.LogBuildMessage("Writing using text method", BuildMessageImportance.Low);
                    using (FileStream stream = new FileStream(this.OutputFile.Get(this.ActivityContext), FileMode.Create))
                    {
                        StreamWriter streamWriter = null;

                        try
                        {
                            streamWriter = new StreamWriter(stream, Encoding.Default);

                            // Output the results to a writer.
                            streamWriter.Write(this.Output.Get(this.ActivityContext));
                        }
                        finally
                        {
                            if (streamWriter != null)
                            {
                                streamWriter.Close();
                            }
                        }
                    }
                }
                else
                {
                    this.LogBuildMessage("Writing using XML method", BuildMessageImportance.Low);
                    using (StringReader sr = new StringReader(this.Output.Get(this.ActivityContext)))
                    {
                        XDocument newxmlDoc = XDocument.Load(sr);
                        if (!string.IsNullOrEmpty(this.OutputFile.Get(this.ActivityContext)))
                        {
                            XmlWriterSettings writerSettings = new XmlWriterSettings 
                            { 
                                ConformanceLevel = this.conformanceLevel, 
                                Encoding = this.fileEncoding, 
                                Indent = this.Indent.Get(this.ActivityContext),
                                OmitXmlDeclaration = this.OmitXmlDeclaration.Get(this.ActivityContext),
                                CloseOutput = true 
                            };
                            using (XmlWriter xw = XmlWriter.Create(this.OutputFile.Get(this.ActivityContext), writerSettings))
                            {
                                newxmlDoc.WriteTo(xw);
                            }
                        }
                    }
                }
            }
        }

        private void Validate()
        {
            this.LogBuildMessage(!string.IsNullOrEmpty(this.XmlFilePath.Get(this.ActivityContext)) ? string.Format(CultureInfo.CurrentCulture, "Validating: {0}", this.XmlFilePath.Get(this.ActivityContext)) : "Validating Xml");
            this.IsValid.Set(this.ActivityContext, false);

            var schemaFiles = this.SchemaFiles.Get(this.ActivityContext);
            if (schemaFiles == null || !schemaFiles.Any())
            {
                this.LogBuildError("No schema were provided for validation");
                return;
            }

            XmlSchemaSet schemas = new XmlSchemaSet();
            foreach (string i in this.SchemaFiles.Get(this.ActivityContext))
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Loading SchemaFile: {0}", i), BuildMessageImportance.Low);
                schemas.Add(this.TargetNamespace, i);
            }

            bool errorEncountered = false;
            StringBuilder builder = new StringBuilder();
            this.xmlDoc.Validate(
                schemas,
                (o, e) =>
                {
                    builder.Append(e.Message);
                    this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "{0}", e.Message));
                    errorEncountered = true;
                });
            this.Output.Set(this.ActivityContext, builder.ToString());

            this.IsValid.Set(this.ActivityContext, !errorEncountered);
        }
    }
}
