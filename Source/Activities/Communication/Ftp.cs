//-----------------------------------------------------------------------
// <copyright file="Ftp.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Communication
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;
 
    /// <summary>
    /// FtpAction
    /// </summary>
    public enum FtpAction
    {
        /// <summary>
        /// Create Directory
        /// </summary>
        CreateDirectory,

        /// <summary>
        /// Delete Directory
        /// </summary>
        DeleteDirectory,

        /// <summary>
        /// Delete Files
        /// </summary>
        DeleteFiles,

        /// <summary>
        /// Download Files
        /// </summary>
        DownloadFiles,

        /// <summary>
        /// Upload Files
        /// </summary>
        UploadFiles
    }

    /// <summary>
    /// <b>Valid Actions are:</b>
    /// <para><i>CreateDirectory</i> (<b>Required:</b> Host<b>Optional:</b> UserName, UserPassword, WorkingDirectory, RemoteDirectoryName, Port)</para>        
    /// <para><i>DeleteDirectory</i> (<b>Required:</b> Host<b>Optional:</b> UserName, UserPassword, WorkingDirectory, RemoteDirectoryName, Port)</para>    
    /// <para><i>DeleteFiles</i> (<b>Required:</b> Host, FileNames <b>Optional:</b> UserName, UserPassword, WorkingDirectory, RemoteDirectoryName, Port)</para>    
    /// <para><i>DownloadFiles</i> (<b>Required:</b> Host <b>Optional:</b> FileNames, UserName, UserPassword, WorkingDirectory, RemoteDirectoryName, Port)</para>    
    /// <para><i>UploadFiles</i> (<b>Required:</b> Host, FileNames <b>Optional:</b> UserName, UserPassword, WorkingDirectory, RemoteDirectoryName, Port)</para>    
    /// <para><b>Remote Execution Support:</b> NA</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="Upload all drops to the ftp folder" sap2010:WorkflowViewState.IdRef="Sequence_16">
    ///     <tac:Ftp FileNames="" WorkingDirectory="Your Working Directory" Action="[FtpAction.CreateDirectory]" DisplayName="Create Directory" Host="Your Ftp Host Name" sap2010:WorkflowViewState.IdRef="Ftp_3" Port="Your Fpt Port" RemoteDirectoryName="Your Ftp Remote directory name" UserName="Your ftp user name" UserPassword="Your ftp password" />          
    ///     <tac:Ftp FileNames="" WorkingDirectory="Your Working Directory" Action="[FtpAction.DeleteDirectory]" DisplayName="Delete Directory" Host="Your Ftp Host Name" sap2010:WorkflowViewState.IdRef="Ftp_3" Port="Your Fpt Port" RemoteDirectoryName="Your Ftp Remote directory name" UserName="Your ftp user name" UserPassword="Your ftp password" />          
    ///     <tac:Ftp FileNames="" WorkingDirectory="Your Working Directory" Action="[FtpAction.DeleteFiles]" DisplayName="Delete Files" Host="Your Ftp Host Name" sap2010:WorkflowViewState.IdRef="Ftp_3" FileNames="Your files to be deleted" Port="Your Fpt Port" RemoteDirectoryName="Your Ftp Remote directory name" UserName="Your ftp user name" UserPassword="Your ftp password" />          
    ///     <tac:Ftp FileNames="" WorkingDirectory="Your Working Directory" Action="[FtpAction.DownloadFiles]" DisplayName="Download Files" Host="Your Ftp Host Name" sap2010:WorkflowViewState.IdRef="Ftp_3" FileNames="Your files to be downloaded" Port="Your Fpt Port" RemoteDirectoryName="Your Ftp Remote directory name" UserName="Your ftp user name" UserPassword="Your ftp password" />          
    ///     <tac:Ftp FileNames="" WorkingDirectory="Your Working Directory" Action="[FtpAction.UploadFiles]" DisplayName="Upload Files" Host="Your Ftp Host Name" sap2010:WorkflowViewState.IdRef="Ftp_3" FileNames="Your files to be uploaded" Port="Your Fpt Port" RemoteDirectoryName="Your Ftp Remote directory name" UserName="Your ftp user name" UserPassword="Your ftp password" />          
    /// </Sequence>
    /// ]]></code>
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Ftp : BaseCodeActivity
    {
        /// <summary>
        /// Specifies the action to perform. 
        /// </summary>
        public InArgument<FtpAction> Action { get; set; }

        /// <summary>
        /// Sets the Host of the FTP Site.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Host { get; set; }

        /// <summary>
        /// Sets the Remote Path to connect to the FTP Site
        /// </summary>
        public InArgument<string> RemoteDirectoryName { get; set; }

        /// <summary>
        /// Sets the working directory on the local machine
        /// </summary>
        public InArgument<string> WorkingDirectory { get; set; }

        /// <summary>
        /// The port used to connect to the ftp server.
        /// </summary>
        public InArgument<int> Port { get; set; }

        /// <summary>
        /// The list of files that needs to be transfered over FTP
        /// </summary>
        public InArgument<string[]> FileNames { get; set; }

        /// <summary>
        /// Sets the UserName
        /// </summary>
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// Sets the UserPassword.
        /// </summary>
        public InArgument<string> UserPassword { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            if (string.IsNullOrEmpty(this.Host.Get(this.ActivityContext)))
            {
                throw new ArgumentException("Required  parameter missing: Host.");
            }

            switch (this.Action.Get(this.ActivityContext))
            {
                case FtpAction.CreateDirectory:
                    this.CreateDirectory();
                    break;

                case FtpAction.DeleteDirectory:
                    this.DeleteDirectory();
                    break;

                case FtpAction.DeleteFiles:
                    this.DeleteFiles();
                    break;

                case FtpAction.DownloadFiles:
                    this.DownloadFiles();
                    break;

                case FtpAction.UploadFiles:
                    this.UploadFiles();
                    break;

                default:
                    throw new ArgumentException("Action not supported.");
            }
        }

        /// <summary>
        /// Creates a new Ftp directory on the ftp server.
        /// </summary>
        private void CreateDirectory()
        {
            if (string.IsNullOrEmpty(this.RemoteDirectoryName.Get(this.ActivityContext)))
            {
                throw new ArgumentException("Required  parameter missing: RemoteDirectoryName.");
            }

            using (FtpConnection ftpConnection = this.CreateFtpConnection())
            {
                ftpConnection.LogOn();
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Creating Directory: {0}", this.RemoteDirectoryName), BuildMessageImportance.Low);
                try
                {
                    ftpConnection.CreateDirectory(this.RemoteDirectoryName.Get(this.ActivityContext));
                }
                catch (FtpException ex)
                {
                    if (ex.Message.Contains("550"))
                    {
                        return;
                    }

                    this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "There was an error creating ftp directory: {0}. The Error Details are \"{1}\" and error code is {2} ", this.RemoteDirectoryName, ex.Message, ex.ErrorCode));
                }
            }
        }

        /// <summary>
        /// Deletes an Ftp directory on the ftp server.
        /// </summary>
        private void DeleteDirectory()
        {
            if (string.IsNullOrEmpty(this.RemoteDirectoryName.Get(this.ActivityContext)))
            {
                throw new ArgumentException("Required  parameter missing: RemoteDirectoryName.");
            }

            using (FtpConnection ftpConnection = this.CreateFtpConnection())
            {
                ftpConnection.LogOn();
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting Directory: {0}", this.RemoteDirectoryName));
                try
                {
                    ftpConnection.DeleteDirectory(this.RemoteDirectoryName.Get(this.ActivityContext));
                }
                catch (FtpException ex)
                {
                    if (ex.Message.Contains("550"))
                    {
                        return;
                    }

                    this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "There was an error deleting ftp directory: {0}. The Error Details are \"{1}\" and error code is {2} ", this.RemoteDirectoryName, ex.Message, ex.ErrorCode));
                }
            }
        }

        /// <summary>
        /// Delete given files from the FTP Directory
        /// </summary>
        private void DeleteFiles()
        {
            if (this.FileNames == null)
            {
                throw new ArgumentException("Required  parameter missing: RemoteDirectoryName.");
            }

            using (FtpConnection ftpConnection = this.CreateFtpConnection())
            {
                ftpConnection.LogOn();
                this.LogBuildMessage("Deleting Files", BuildMessageImportance.Low);
                if (!string.IsNullOrEmpty(this.RemoteDirectoryName.Get(this.ActivityContext)))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Current Directory: {0}", this.RemoteDirectoryName.Get(this.ActivityContext)), BuildMessageImportance.Low);
                    ftpConnection.SetCurrentDirectory(this.RemoteDirectoryName.Get(this.ActivityContext));
                }

                foreach (string fileName in this.FileNames.Get(this.ActivityContext))
                {
                    try
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Current Directory: {0}", this.RemoteDirectoryName), BuildMessageImportance.Low);
                        ftpConnection.DeleteFile(fileName);
                    }
                    catch (FtpException ex)
                    {
                        if (ex.Message.Contains("550"))
                        {
                            continue;
                        }

                        this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "There was an error in deleting file: {0}. The Error Details are \"{1}\" and error code is {2} ", fileName, ex.Message, ex.ErrorCode));
                    }
                }
            }
        }

        /// <summary>
        /// Upload Files 
        /// </summary>
        private void UploadFiles()
        {
            if (this.FileNames == null)
            {
                throw new ArgumentException("Required  parameter missing: FileNames.");
            }

            using (FtpConnection ftpConnection = this.CreateFtpConnection())
            {
                this.LogBuildMessage("Uploading Files", BuildMessageImportance.Low);

                if (!string.IsNullOrEmpty(this.WorkingDirectory.Get(this.ActivityContext)))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Working Directory: {0}", this.WorkingDirectory.Get(this.ActivityContext)), BuildMessageImportance.Low);
                    FtpConnection.SetLocalDirectory(this.WorkingDirectory.Get(this.ActivityContext));
                }

                ftpConnection.LogOn();
                if (!string.IsNullOrEmpty(this.RemoteDirectoryName.Get(this.ActivityContext)))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Current Directory: {0}", this.RemoteDirectoryName.Get(this.ActivityContext)), BuildMessageImportance.Low);
                    ftpConnection.SetCurrentDirectory(this.RemoteDirectoryName.Get(this.ActivityContext));
                }

                foreach (string fileName in this.FileNames.Get(this.ActivityContext))
                {
                    try
                    {
                        if (File.Exists(fileName))
                        {
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Uploading: {0}", fileName), BuildMessageImportance.Low);
                            ftpConnection.PutFile(fileName);
                        }
                    }
                    catch (FtpException ex)
                    {
                        this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "There was an error uploading file: {0}. The Error Details are \"{1}\" and error code is {2} ", fileName, ex.Message, ex.ErrorCode));
                    }
                }
            }
        }

        /// <summary>
        /// Download Files 
        /// </summary>
        private void DownloadFiles()
        {
            using (FtpConnection ftpConnection = this.CreateFtpConnection())
            {
                if (!string.IsNullOrEmpty(this.WorkingDirectory.Get(this.ActivityContext)))
                {
                    if (!Directory.Exists(this.WorkingDirectory.Get(this.ActivityContext)))
                    {
                        Directory.CreateDirectory(this.WorkingDirectory.Get(this.ActivityContext));
                    }

                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Setting Local Directory: {0}", this.WorkingDirectory.Get(this.ActivityContext)), BuildMessageImportance.Low);
                    FtpConnection.SetLocalDirectory(this.WorkingDirectory.Get(this.ActivityContext));
                }

                ftpConnection.LogOn();
                if (!string.IsNullOrEmpty(this.RemoteDirectoryName.Get(this.ActivityContext)))
                {
                    ftpConnection.SetCurrentDirectory(this.RemoteDirectoryName.Get(this.ActivityContext));
                }

                this.LogBuildMessage("Downloading Files", BuildMessageImportance.Low);
                if (this.FileNames == null)
                {
                    FtpFileInfo[] filesToDownload = ftpConnection.GetFiles();
                    foreach (FtpFileInfo fileToDownload in filesToDownload)
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Downloading: {0}", fileToDownload), BuildMessageImportance.Low);
                        ftpConnection.GetFile(fileToDownload.Name, false);
                    }
                }
                else
                {
                    foreach (string fileName in this.FileNames.Get(this.ActivityContext))
                    {
                        try
                        {
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Downloading: {0}", fileName), BuildMessageImportance.Low);
                            ftpConnection.GetFile(fileName, false);
                        }
                        catch (FtpException ex)
                        {
                            this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "There was an error downloading file: {0}. The Error Details are \"{1}\" and error code is {2} ", fileName, ex.Message, ex.ErrorCode));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates an FTP Connection object 
        /// </summary>
        /// <returns>An initialised FTP Connection</returns>
        private FtpConnection CreateFtpConnection()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Connecting to FTP Host: {0}", this.Host.Get(this.ActivityContext)));
            if (!string.IsNullOrEmpty(this.UserName.Get(this.ActivityContext)))
            {
                return this.Port.Get(this.ActivityContext) != 0 ? new FtpConnection(this.Host.Get(this.ActivityContext), this.Port.Get(this.ActivityContext), this.UserName.Get(this.ActivityContext), this.UserPassword.Get(this.ActivityContext)) : new FtpConnection(this.Host.Get(this.ActivityContext), this.UserName.Get(this.ActivityContext), this.UserPassword.Get(this.ActivityContext));
            }

            return this.Port.Get(this.ActivityContext) != 0 ? new FtpConnection(this.Host.Get(this.ActivityContext), this.Port.Get(this.ActivityContext)) : new FtpConnection(this.Host.Get(this.ActivityContext));
        }
    }
}
