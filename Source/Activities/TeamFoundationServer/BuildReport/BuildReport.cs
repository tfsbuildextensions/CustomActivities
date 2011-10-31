//-----------------------------------------------------------------------
// <copyright file="BuildReport.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Xsl;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using TfsBuildExtensions.Activities;
    using Attachment = System.Net.Mail.Attachment;

    /// <summary>
    ///     BuildReportAction
    /// </summary>
    public enum BuildReportAction
    {
        /// <summary>
        /// Generate
        /// </summary>
        Generate
    }

    /// <summary>
    /// <b>Valid Action values are:</b> Generate
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class BuildReport : BaseCodeActivity
    {
        private readonly string buildReportTime = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss");
        private BuildReportAction action = BuildReportAction.Generate;
        private XDocument xmlDoc;
        private InArgument<string> reportFileName = "BuildReport.txt";
        private FileInfo xmlfile;
        private FileInfo reportFile;
        private FileInfo transformedReportFile;
        private IBuildDetail buildDetail;
        private IBuildAgent buildAgent;
        private List<string> allFiles;
        private IList<Changeset> associatedChangesets;

        /// <summary>
        /// Sets the Build Report file name. Defaults to BuildReport.txt
        /// </summary>
        public InArgument<string> ReportFileName
        {
            get { return this.reportFileName; }
            set { this.reportFileName = value; }
        }

        /// <summary>
        /// Sets the Build Report file path. Defaults to the build DropLocation
        /// </summary>
        public InArgument<string> FilePath { get; set; }

        /// <summary>
        /// Sets the Xsl File to transform the Xml report. An embbeded Xsl file is used by default.
        /// </summary>
        public InArgument<string> XslTransformFile { get; set; }

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public BuildReportAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The SMTP server to use to send the email.
        /// </summary>
        public InArgument<string> SmtpServer { get; set; }

        /// <summary>
        /// Sets the port to use. Ignored if not specified.
        /// </summary>
        public InArgument<int> Port { get; set; }

        /// <summary>
        /// Sets whether to email the report. Default is false
        /// </summary>
        public InArgument<bool> EmailReport { get; set; }

        /// <summary>
        /// Sets whether to EnableSsl
        /// </summary>
        public InArgument<bool> EnableSsl { get; set; }

        /// <summary>
        /// Sets whether to skip the reporting of all files in the DropLocation. Default is false;
        /// </summary>
        public InArgument<bool> SkipFiles { get; set; }

        /// <summary>
        /// The email address to send the email from.
        /// </summary>
        public InArgument<string> MailFrom { get; set; }

        /// <summary>
        /// Sets the Item Collection of email address to send the email to.
        /// </summary>
        public InArgument<string[]> MailTo { get; set; }

        /// <summary>
        /// The subject of the email. Defaults to "Build Report: BUILDNAME (BUILDNUMBER)"
        /// </summary>
        public InArgument<string> Subject { get; set; }

        /// <summary>
        /// The priority of the email. Default is Normal
        /// </summary>
        public InArgument<string> Priority { get; set; }

        /// <summary>
        /// Sets the format of the email. Default is Text
        /// </summary>
        public InArgument<string> Format { get; set; }

        /// <summary>
        /// Sets the UserName if SmtpClient requires credentials
        /// </summary>
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// Sets the UserPassword if SmtpClient requires credentials
        /// </summary>
        public InArgument<string> UserPassword { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that controls whether the DefaultCredentials are sent with requests. DefaultCredentials represents the system credentials for the current security context in which the application is running. Default is true.
        /// <para>If UserName and UserPassword is supplied, this is set to false. If UserName and UserPassword are not supplied and this is set to false then mail is sent to the server anonymously.</para>
        /// <para><b>If you provide credentials for basic authentication, they are sent to the server in clear text. This can present a security issue because your credentials can be seen, and then used by others.</b></para>
        /// </summary>
        public InArgument<bool> UseDefaultCredentials { get; set; }

        /// <summary>
        /// An Item Collection of full paths of files to attach to the email.
        /// </summary>
        public InArgument<IEnumerable<string>> Attachments { get; set; }

        /// <summary>
        /// Sets the changesets associated with the build
        /// </summary>
        public InArgument<IList<Changeset>> AssociatedChangesets { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            // Initialise defaults
            if (this.UseDefaultCredentials.Expression == null)
            {
                this.UseDefaultCredentials.Set(this.ActivityContext, true);
            }

            if (this.Format.Expression == null)
            {
                this.Format.Set(this.ActivityContext, "Text");
            }

            if (this.Priority.Expression == null)
            {
                this.Priority.Set(this.ActivityContext, "Normal");
            }

            this.buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            this.buildAgent = this.ActivityContext.GetExtension<IBuildAgent>();
            string filePath = this.FilePath.Expression == null ? this.buildDetail.DropLocation : this.FilePath.Get(this.ActivityContext);
            this.reportFile = new FileInfo(Path.Combine(filePath, this.ReportFileName.Get(this.ActivityContext)));
            this.xmlfile = new FileInfo(Path.Combine(filePath, this.ReportFileName.Get(this.ActivityContext).Replace(".txt", ".xml")));
            this.transformedReportFile = new FileInfo(Path.Combine(filePath, this.ReportFileName.Get(this.ActivityContext).Replace(".txt", ".html")));
            this.associatedChangesets = this.AssociatedChangesets.Get(this.ActivityContext);
            
            switch (this.Action)
            {
                case BuildReportAction.Generate:
                    this.GenerateReport();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private static List<Change> GetFilesAssociatedWithBuild(VersionControlServer versionControlServer, int changesetId)
        {
            List<Change> files = new List<Change>();
            Changeset changeset = versionControlServer.GetChangeset(changesetId);
            if (changeset.Changes != null)
            {
                files.AddRange(changeset.Changes);
            }

            return files;
        }

        private static List<string> GetFilesRecursive(string b)
        {
            List<string> result = new List<string>();

            // stack the directories
            Stack<string> stack = new Stack<string>();

            // Add initial directory
            stack.Push(b);

            // Continue while there are directories to process
            while (stack.Count > 0)
            {
                // Get top directory
                string dir = stack.Pop();

                try
                {
                    // Add all files at this directory to the result List.
                    result.AddRange(Directory.GetFiles(dir, "*.*"));

                    // Add all directories at this directory.
                    foreach (string dn in Directory.GetDirectories(dir))
                    {
                        stack.Push(dn);
                    }
                }
                catch
                {
                    // do nothing
                }
            }

            return result;
        }

        private static string GetFileSize(double byteCount)
        {
            string size;
            if (byteCount >= 1073741824.0)
            {
                size = string.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";
            }
            else if (byteCount >= 1048576.0)
            {
                size = string.Format("{0:##.##}", byteCount / 1048576.0) + " MB";
            }
            else
            {
                size = string.Format("{0:##.##}", byteCount / 1024.0) + " KB";
            }

            return size;
        }

        private void Transform()
        {
            this.LogBuildMessage("Transforming Xml Build Report", BuildMessageImportance.Low);
            XDocument xslDoc;

            if (this.XslTransformFile.Expression == null)
            {
                System.IO.Stream s = this.GetType().Assembly.GetManifestResourceStream("TfsBuildExtensions.Activities.TeamFoundationServer.BuildReport.DefaultTransform.xslt");
                xslDoc = XDocument.Load(s);
            }
            else
            {
                if (!File.Exists(this.XslTransformFile.Get(this.ActivityContext)))
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "XslTransformFile not found: {0}", this.XslTransformFile.Get(this.ActivityContext)));
                    return;
                }

                // Load the XslTransformFile
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Loading XslTransformFile: {0}", this.XslTransformFile.Get(this.ActivityContext)));
                xslDoc = XDocument.Load(this.XslTransformFile.Get(this.ActivityContext));
            }

            // Load the style sheet.
            XslCompiledTransform xslt = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings { EnableScript = true };
            using (StringReader sr = new StringReader(xslDoc.ToString()))
            {
                xslt.Load(XmlReader.Create(sr), settings, null);
                StringBuilder builder = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(builder, xslt.OutputSettings))
                {
                    this.LogBuildMessage("Running XslTransform", BuildMessageImportance.Low);
                    this.xmlDoc = XDocument.Load(this.xmlfile.FullName);

                    // Execute the transform and output the results to a writer.
                    xslt.Transform(this.xmlDoc.CreateReader(), writer);
                }

                if (xslt.OutputSettings.OutputMethod == XmlOutputMethod.Text)
                {
                    this.LogBuildMessage("Writing using text method", BuildMessageImportance.Low);
                    using (FileStream stream = new FileStream(this.transformedReportFile.FullName, FileMode.Create))
                    {
                        StreamWriter streamWriter = new StreamWriter(stream, Encoding.Default);

                        // Output the results to a writer.
                        streamWriter.Write(builder.ToString());
                    }
                }
                else
                {
                    this.LogBuildMessage("Writing using XML method", BuildMessageImportance.Low);
                    using (StringReader sr1 = new StringReader(builder.ToString()))
                    {
                        XDocument newxmlDoc = XDocument.Load(sr1);
                        XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, CloseOutput = true };
                        using (XmlWriter xw = XmlWriter.Create(this.transformedReportFile.FullName, writerSettings))
                        {
                            newxmlDoc.WriteTo(xw);
                        }
                    }
                }
            }
        }

        private void GenerateReport()
        {
            if (!Convert.ToBoolean(this.SkipFiles.Get(this.ActivityContext)))
            {
                this.allFiles = GetFilesRecursive(this.buildDetail.DropLocation);
            }

            this.GenerateTextReport();
            this.GenerateXmlReport();
            this.Transform();

            if (Convert.ToBoolean(this.EmailReport.Get(this.ActivityContext)))
            {
                this.Send();
            }
        }

        private void GenerateXmlReport()
        {
            this.LogBuildMessage(string.Format("Generating Xml Build Report to: {0}", this.reportFile.FullName.Replace(".txt", ".xml")));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<buildreport/>");

            doc.DocumentElement.SetAttribute("buildName", this.buildDetail.BuildDefinition.Name);
            doc.DocumentElement.SetAttribute("buildNumber", this.buildDetail.BuildNumber);
            doc.DocumentElement.SetAttribute("buildAgent", this.buildAgent.Name);
            doc.DocumentElement.SetAttribute("buildAgentUri", this.buildAgent.Uri.ToString());
            doc.DocumentElement.SetAttribute("buildController", this.buildDetail.BuildController.Name);
            doc.DocumentElement.SetAttribute("teamProject", this.buildDetail.TeamProject);
            doc.DocumentElement.SetAttribute("compilationStatus", this.buildDetail.CompilationStatus.ToString());
            doc.DocumentElement.SetAttribute("testStatus", this.buildDetail.TestStatus.ToString());
            doc.DocumentElement.SetAttribute("buildReason", this.buildDetail.Reason.ToString());
            doc.DocumentElement.SetAttribute("requestedBy", this.buildDetail.RequestedBy);
            doc.DocumentElement.SetAttribute("requestedFor", this.buildDetail.RequestedFor);
            doc.DocumentElement.SetAttribute("sourceGetVersion", this.buildDetail.SourceGetVersion);
            doc.DocumentElement.SetAttribute("startTime", this.buildDetail.StartTime.ToString("dd MMM yyyy hh:mm:ss"));
            doc.DocumentElement.SetAttribute("reportTime", this.buildReportTime);
            doc.DocumentElement.SetAttribute("buildUri", this.buildDetail.Uri.ToString());
            doc.DocumentElement.SetAttribute("logLocation", this.buildDetail.LogLocation);
            doc.DocumentElement.SetAttribute("dropLocation", this.buildDetail.DropLocation);

            if (!Convert.ToBoolean(this.SkipFiles.Get(this.ActivityContext)))
            {
                XmlElement filesElement = doc.CreateElement("OutputFiles");
                filesElement.SetAttribute("count", this.allFiles.Count.ToString());

                foreach (string p in this.allFiles)
                {
                    XmlElement elem = doc.CreateElement("file");

                    FileInfo f = new FileInfo(p);
                    elem.SetAttribute("creationTimeUtc", f.CreationTimeUtc.ToLongDateString() + " " + f.CreationTimeUtc.ToLongTimeString());
                    elem.SetAttribute("size", GetFileSize(f.Length));
                    XmlText text = doc.CreateTextNode(p);
                    filesElement.AppendChild(elem);
                    filesElement.LastChild.AppendChild(text);
                }

                doc.DocumentElement.AppendChild(filesElement);
            }

            if (this.associatedChangesets != null)
            {
                XmlElement changesetElement = doc.CreateElement("Changesets");
                changesetElement.SetAttribute("count", this.associatedChangesets.Count.ToString());
                foreach (Changeset changeset in this.associatedChangesets)
                {
                    XmlElement elem = doc.CreateElement("changeset");
                    elem.SetAttribute("id", changeset.ChangesetId.ToString());
                    elem.SetAttribute("committer", changeset.Committer);
                    elem.SetAttribute("owner", changeset.Owner);
                    elem.SetAttribute("associatedWorkItems", changeset.WorkItems.Length.ToString());

                    IList<WorkItem> workitems = changeset.WorkItems;
                    List<Change> files = GetFilesAssociatedWithBuild(changeset.VersionControlServer, changeset.ChangesetId);

                    XmlText text = doc.CreateTextNode(changeset.Comment);
                    changesetElement.AppendChild(elem);
                    changesetElement.LastChild.AppendChild(text);

                    XmlElement workitemsElement = doc.CreateElement("WorkItems");
                    workitemsElement.SetAttribute("count", workitems.Count.ToString());
                    foreach (WorkItem workitem in workitems)
                    {
                        XmlElement wi = doc.CreateElement("workitem");
                        wi.SetAttribute("type", workitem.Type.Name);
                        wi.SetAttribute("id", workitem.Id.ToString());
                        wi.SetAttribute("title", workitem.Title);
                        wi.SetAttribute("state", workitem.State);
                        wi.SetAttribute("reason", workitem.Reason);
                        workitemsElement.AppendChild(wi);
                    }

                    elem.AppendChild(workitemsElement);

                    XmlElement changedFilesElement = doc.CreateElement("Files");
                    changedFilesElement.SetAttribute("count", files.Count.ToString());
                    foreach (Change file in files)
                    {
                        XmlElement fi = doc.CreateElement("file");
                        fi.SetAttribute("name", file.Item.ServerItem);
                        fi.SetAttribute("change", file.ChangeType.ToString());
                        changedFilesElement.AppendChild(fi);
                    }

                    elem.AppendChild(changedFilesElement);
                }

                doc.DocumentElement.AppendChild(changesetElement);
            }

            doc.Save(this.reportFile.FullName.Replace(".txt", ".xml"));
        }

        private void GenerateTextReport()
        {
            this.LogBuildMessage(string.Format("Generating BuildReport to: {0}", this.reportFile.FullName));

            this.WriteToFile(string.Format("Build Name:\t{0}", this.buildDetail.BuildDefinition.Name), 0);
            this.WriteToFile(string.Format("Build Number:\t{0}", this.buildDetail.BuildNumber), 0);
            this.WriteToFile("________________________________________________________________", 1);
            this.WriteToFile(string.Format("Build Agent:\t\t{0}", this.buildAgent.Name), 0);
            this.WriteToFile(string.Format("Build Agent Uri:\t{0}", this.buildAgent.Uri), 0);
            this.WriteToFile(string.Format("Build Controller:\t{0}", this.buildDetail.BuildController.Name), 0);
            this.WriteToFile(string.Format("Build Reason:\t\t{0}", this.buildDetail.Reason), 0);
            this.WriteToFile(string.Format("Build Uri:\t\t{0}", this.buildDetail.Uri), 0);
            this.WriteToFile(string.Format("Compilation Status:\t{0}", this.buildDetail.CompilationStatus), 0);
            this.WriteToFile(string.Format("Drop Location:\t\t{0}", this.buildDetail.DropLocation), 0);
            this.WriteToFile(string.Format("Log Location:\t\t{0}", this.buildDetail.LogLocation), 0);
            this.WriteToFile(string.Format("Report Time:\t\t{0}", this.buildReportTime), 0);
            this.WriteToFile(string.Format("Requested By:\t\t{0}", this.buildDetail.RequestedBy), 0);
            this.WriteToFile(string.Format("Requested For:\t\t{0}", this.buildDetail.RequestedFor), 0);
            this.WriteToFile(string.Format("Source GetVersion:\t{0}", this.buildDetail.SourceGetVersion), 0);
            this.WriteToFile(string.Format("Start Time:\t\t{0}", this.buildDetail.StartTime.ToString("dd MMM yyyy hh:mm:ss")), 0);
            this.WriteToFile(string.Format("Team Project:\t\t{0}", this.buildDetail.TeamProject), 0);
            this.WriteToFile(string.Format("Test Status:\t\t{0}", this.buildDetail.TestStatus), 0);          
            
            if (this.associatedChangesets != null)
            {
                this.WriteToFile(string.Empty, 1);
                string label = this.associatedChangesets.Count > 1 ? "Changesets" : "Changeset";
                this.WriteToFile(string.Format("{1} ({0})", this.associatedChangesets.Count, label), 0);
                this.WriteToFile("_____________________________________________________________", 1);

                foreach (Changeset changeset in this.associatedChangesets)
                {
                    this.WriteToFile(string.Format("Id:\t\t{0}", changeset.ChangesetId), 0);
                    this.WriteToFile(string.Format("Comment:\t{0}", changeset.Comment), 0);
                    this.WriteToFile(string.Format("Owner:\t\t{0}", changeset.Owner), 0);
                    this.WriteToFile(string.Format("Committer:\t{0}", changeset.Committer), 1);

                    IList<WorkItem> workitems = changeset.WorkItems;
                    List<Change> files = GetFilesAssociatedWithBuild(changeset.VersionControlServer, changeset.ChangesetId);

                    if (workitems.Count > 0)
                    {
                        this.WriteToFile(string.Format("\tWorkitems ({0})", workitems.Count), 0);
                        this.WriteToFile("\t______________________________", 1);
                        foreach (WorkItem workitem in workitems)
                        {
                            this.WriteToFile(string.Format("\tType:\t{0}", workitem.Type.Name), 0);
                            this.WriteToFile(string.Format("\tId:\t{0}", workitem.Id), 0);
                            this.WriteToFile(string.Format("\tTitle:\t{0}", workitem.Title), 0);
                            this.WriteToFile(string.Format("\tState:\t{0}", workitem.State), 0);
                            this.WriteToFile(string.Format("\tReason:\t{0}", workitem.Reason), 1);
                        }
                    }

                    if (files.Count > 0)
                    {
                        this.WriteToFile(string.Format("\tFiles ({0})", files.Count), 0);
                        this.WriteToFile("\t______________________________", 1);
                        foreach (Change file in files)
                        {
                            this.WriteToFile(string.Format("\tName:\t{0}", file.Item.ServerItem), 0);
                            this.WriteToFile(string.Format("\tChange:\t{0}", file.ChangeType), 1);
                        }
                    }
                }
            }

            if (!Convert.ToBoolean(this.SkipFiles.Get(this.ActivityContext)))
            {
                if (this.allFiles.Count > 0)
                {
                    this.WriteToFile(string.Empty, 1);
                    string label = this.allFiles.Count > 1 ? "Output Files" : "Output File";
                    this.WriteToFile(string.Format("{1} ({0})", this.allFiles.Count, label), 0);
                    this.WriteToFile("_____________________________________________________________", 0);
                    foreach (string p in this.allFiles)
                    {
                        this.WriteToFile(p.Replace(this.buildDetail.DropLocation + @"\", string.Empty), 0);
                    }
                }
            }
        }

        private void WriteToFile(string line, int linebreaks)
        {
            using (StreamWriter file = new StreamWriter(this.reportFile.FullName, true))
            {
                file.WriteLine(line);
                if (linebreaks > 0)
                {
                    for (int i = 1; i <= linebreaks; i++)
                    {
                        file.Write(Environment.NewLine);
                    }
                }
            }  
        }
        
        private void Send()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Sending BuildReport email: {0}", this.Subject.Get(this.ActivityContext)));
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(this.MailFrom.Get(this.ActivityContext));
                foreach (string recipient in this.MailTo.Get(this.ActivityContext))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding recipient: {0}", recipient), BuildMessageImportance.Low);
                    msg.To.Add(new MailAddress(recipient));
                }

                Attachment attachment = new Attachment(this.reportFile.FullName);
                msg.Attachments.Add(attachment);

                if (this.Attachments.Get(this.ActivityContext) != null)
                {
                    foreach (string file in this.Attachments.Get(this.ActivityContext))
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding attachment: {0}", file), BuildMessageImportance.Low);
                        attachment = new Attachment(file);
                        msg.Attachments.Add(attachment);
                    }
                }

                msg.Subject = this.Subject.Expression == null ? string.Format("Build Report: {0} ({1})", this.buildDetail.BuildDefinition.Name, this.buildDetail.BuildNumber) : this.Subject.Get(this.ActivityContext);

                if (this.Format.Get(this.ActivityContext).ToUpperInvariant() == "HTML")
                {
                    msg.Body = System.IO.File.ReadAllText(this.transformedReportFile.FullName);
                    msg.IsBodyHtml = true;
                }
                else
                {
                    msg.Body = System.IO.File.ReadAllText(this.reportFile.FullName);
                    msg.IsBodyHtml = false;
                }

                using (SmtpClient client = new SmtpClient(this.SmtpServer.Get(this.ActivityContext)))
                {
                    if (Convert.ToInt32(this.Port.Get(this.ActivityContext)) > 0)
                    {
                        client.Port = Convert.ToInt32(this.Port.Get(this.ActivityContext));
                    }

                    client.EnableSsl = Convert.ToBoolean(this.EnableSsl.Get(this.ActivityContext));
                    client.UseDefaultCredentials = this.UseDefaultCredentials.Get(this.ActivityContext);
                    if (!string.IsNullOrEmpty(this.UserName.Get(this.ActivityContext)))
                    {
                        client.Credentials = new NetworkCredential(this.UserName.Get(this.ActivityContext), this.UserPassword.Get(this.ActivityContext));
                    }

                    client.Send(msg);
                }
            }
        }
    }
}