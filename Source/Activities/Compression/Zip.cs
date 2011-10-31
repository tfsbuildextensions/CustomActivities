//-----------------------------------------------------------------------
// <copyright file="Zip.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
#pragma warning disable 618
namespace TfsBuildExtensions.Activities.Compression
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Ionic.Zip;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// ZipAction
    /// </summary>
    public enum ZipAction
    {
        /// <summary>
        /// AddFiles
        /// </summary>
        AddFiles,

        /// <summary>
        /// Create
        /// </summary>
        Create,

        /// <summary>
        /// Extract
        /// </summary>
        Extract
    }

    /// <summary>
    /// <b>Valid Action values are:</b>
    /// <para><i>AddFiles</i> (<b>Required: </b> ZipFileName, CompressFiles or Path <b>Optional: </b>CompressionLevel, Password; RemoveRoot) Existing files will be updated</para>
    /// <para><i>Create</i> (<b>Required: </b> ZipFileName, CompressFiles or Path <b>Optional: </b>CompressionLevel, Password; RemoveRoot)</para>
    /// <para><i>Extract</i> (<b>Required: </b> ZipFileName, ExtractPath <b>Optional:</b> Password)</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Sequence DisplayName="TFSBuildExtensions Zip Sequence" sap:VirtualizedContainerService.HintSize="1178,146">
    /// <tac:Zip ExtractPath="{x:Null}" Files="{x:Null}" LogExceptionStack="{x:Null}" Password="{x:Null}" RemoveRoot="{x:Null}" TreatWarningsAsErrors="{x:Null}" Action="Create" CompressPath="[BinariesDirectory]" CompressionLevel="Default" FailBuildOnError="True" sap:VirtualizedContainerService.HintSize="200,22" ZipFileName="[BinariesDirectory + &quot;\\&quot; + &quot;MyBuild&quot; + &quot;.zip&quot;]" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Zip : BaseCodeActivity
    {
        private ZipAction action = ZipAction.Create;
        private Ionic.Zlib.CompressionLevel compressLevel = Ionic.Zlib.CompressionLevel.Default;
        
        /// <summary>
        /// Sets the root to remove from the zip path. Note that this should be part of the file to compress path, not the target path of the ZipFileName
        /// </summary>
        public InArgument<string> RemoveRoot { get; set; }

        /// <summary>
        /// Sets the files to Compress
        /// </summary>
        public InArgument<IEnumerable<string>> Files { get; set; }

        /// <summary>
        /// Sets the Path to Zip.
        /// </summary>
        public InArgument<string> CompressPath { get; set; }

        /// <summary>
        /// Sets the name of the Zip File
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ZipFileName { get; set; }

        /// <summary>
        /// Path to extract the zip file to
        /// </summary>
        public InArgument<string> ExtractPath { get; set; }

        /// <summary>
        /// Sets the Password to be used
        /// </summary>
        public InArgument<string> Password { get; set; }

        /// <summary>
        /// Sets the CompressionLevel to use. Default is Default, also supports BestSpeed and BestCompression
        /// </summary>
        public string CompressionLevel
        {
            get { return this.compressLevel.ToString(); }
            set { this.compressLevel = (Ionic.Zlib.CompressionLevel)Enum.Parse(typeof(Ionic.Zlib.CompressionLevel), value); }
        }

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public ZipAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case ZipAction.Create:
                    this.Create();
                    break;
                case ZipAction.Extract:
                    this.Extract();
                    break;
                case ZipAction.AddFiles:
                    this.AddFiles();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private void AddFiles()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Adding files to ZipFile: {0}", this.ZipFileName.Get(this.ActivityContext)));
            if (this.Files.Get(this.ActivityContext) != null)
            {
                using (ZipFile zip = ZipFile.Read(this.ZipFileName.Get(this.ActivityContext)))
                {
                    zip.ParallelDeflateThreshold = -1;
                    zip.UseUnicodeAsNecessary = true;
                    zip.CompressionLevel = this.compressLevel;
                    if (!string.IsNullOrEmpty(this.Password.Get(this.ActivityContext)))
                    {
                        zip.Password = this.Password.Get(this.ActivityContext);
                    }

                    foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
                    {
                        if (this.RemoveRoot.Expression != null)
                        {
                            FileInfo f = new FileInfo(fullfilename);
                            string location = f.DirectoryName.Replace(this.RemoveRoot.Get(this.ActivityContext), string.Empty);
                            zip.UpdateFile(fullfilename, location);
                        }
                        else
                        {
                            zip.UpdateFile(fullfilename);
                        }
                    }

                    zip.Save();
                }
            }
            else
            {
                using (ZipFile zip = ZipFile.Read(this.ZipFileName.Get(this.ActivityContext)))
                {
                    zip.ParallelDeflateThreshold = -1;
                    zip.UseUnicodeAsNecessary = true;
                    zip.CompressionLevel = this.compressLevel;
                    if (!string.IsNullOrEmpty(this.Password.Get(this.ActivityContext)))
                    {
                        zip.Password = this.Password.Get(this.ActivityContext);
                    }

                    if (this.RemoveRoot.Expression != null)
                    {
                        DirectoryInfo d = new DirectoryInfo(this.CompressPath.Get(this.ActivityContext));
                        string location = d.FullName.Replace(this.RemoveRoot.Get(this.ActivityContext), string.Empty);
                        zip.AddDirectory(this.CompressPath.Get(this.ActivityContext), location);
                    }
                    else
                    {
                        zip.UpdateDirectory(this.CompressPath.Get(this.ActivityContext));
                    }

                    zip.Save();
                }
            }
        }

        private void Create()
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Creating ZipFile: {0}", this.ZipFileName.Get(this.ActivityContext)));
            if (this.CompressPath.Expression == null)
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.ParallelDeflateThreshold = -1;
                    zip.UseUnicodeAsNecessary = true;
                    zip.CompressionLevel = this.compressLevel;
                    if (!string.IsNullOrEmpty(this.Password.Get(this.ActivityContext)))
                    {
                        zip.Password = this.Password.Get(this.ActivityContext);
                    }

                    foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
                    {
                        if (this.RemoveRoot.Expression != null)
                        {
                            FileInfo f = new FileInfo(fullfilename);
                            string location = f.DirectoryName.Replace(this.RemoveRoot.Get(this.ActivityContext), string.Empty);
                            zip.AddFile(fullfilename, location);
                        }
                        else
                        {
                            zip.AddFile(fullfilename);
                        }
                    }

                    zip.Save(this.ZipFileName.Get(this.ActivityContext));
                }
            }
            else
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.ParallelDeflateThreshold = -1;
                    zip.UseUnicodeAsNecessary = true;
                    zip.CompressionLevel = this.compressLevel;
                    if (!string.IsNullOrEmpty(this.Password.Get(this.ActivityContext)))
                    {
                        zip.Password = this.Password.Get(this.ActivityContext);
                    }

                    if (this.RemoveRoot.Expression != null)
                    {
                        DirectoryInfo d = new DirectoryInfo(this.CompressPath.Get(this.ActivityContext));
                        string location = d.FullName.Replace(this.RemoveRoot.Get(this.ActivityContext), string.Empty);
                        zip.AddDirectory(this.CompressPath.Get(this.ActivityContext), location);
                    }
                    else
                    {
                        DirectoryInfo d = new DirectoryInfo(this.CompressPath.Get(this.ActivityContext));
                        zip.AddDirectory(this.CompressPath.Get(this.ActivityContext), d.Name);
                    }

                    zip.Save(this.ZipFileName.Get(this.ActivityContext));
                }
            }
        }

        private void Extract()
        {
            if (!File.Exists(this.ZipFileName.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "ZipFileName not found: {0}", this.ZipFileName.Get(this.ActivityContext)));
                return;
            }

            if (string.IsNullOrEmpty(this.ExtractPath.Get(this.ActivityContext)))
            {
                this.LogBuildError("ExtractPath is required");
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Extracting ZipFile: {0} to: {1}", this.ZipFileName.Get(this.ActivityContext), this.ExtractPath.Get(this.ActivityContext)));

            using (ZipFile zip = ZipFile.Read(this.ZipFileName.Get(this.ActivityContext)))
            {
                if (!string.IsNullOrEmpty(this.Password.Get(this.ActivityContext)))
                {
                    zip.Password = this.Password.Get(this.ActivityContext);
                }

                foreach (ZipEntry e in zip)
                {
                    e.Extract(this.ExtractPath.Get(this.ActivityContext), ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }
    }
}