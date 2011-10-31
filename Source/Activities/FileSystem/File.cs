//-----------------------------------------------------------------------
// <copyright file="File.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.FileSystem
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// FileAction
    /// </summary>
    public enum FileAction
    {
        /// <summary>
        /// AddAttributes
        /// </summary>
        AddAttributes,

        /// <summary>
        /// GetChecksum
        /// </summary>
        GetChecksum,

        /// <summary>
        /// GetTempFileName
        /// </summary>
        GetTempFileName,

        /// <summary>
        /// Move
        /// </summary>
        Move,

        /// <summary>
        /// RemoveAttributes
        /// </summary>
        RemoveAttributes,

        /// <summary>
        /// SetAttributes
        /// </summary>
        SetAttributes,

        /// <summary>
        /// Replace
        /// </summary>
        Replace,

        /// <summary>
        /// Touch
        /// </summary>
        Touch,

        /// <summary>
        /// Delete
        /// </summary>
        Delete
    }

    /// <summary>
    /// <b>Valid Action values are:</b>
    /// <para><i>AddAttributes</i> (<b>Required: </b>Attributes, Files)</para>
    /// <para><i>GetChecksum</i> (<b>Required: </b>Path <b>Output: </b>Checksum)</para>
    /// <para><i>GetTempFileName</i> (<b>Output: </b>Path)</para>
    /// <para><i>Move</i> (<b>Required: </b>Path, TargetPath)</para>
    /// <para><i>RemoveAttributes</i> (<b>Required: </b>Attributes, Files)</para>
    /// <para><i>Replace</i> (<b>Required: </b>RegexPattern <b>Optional: </b>Replacement, Path, TextEncoding, Files)</para>
    /// <para><i>SetAttributes</i> (<b>Required: </b>Attributes, Files)</para>
    /// <para><i>Touch</i> (<b>Required: </b>Files <b>Optional: </b>Force, Time)</para>
    /// <para><i>Delete</i> (<b>Required:</b> Files <b>Optional: </b>Force)</para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class File : BaseCodeActivity
    {
        private FileAction action = FileAction.Replace;
        private Encoding fileEncoding = Encoding.UTF8;
        private InArgument<string> replacement = string.Empty;
        private Regex parseRegex;

        /// <summary>
        /// Sets the regex pattern.
        /// </summary>
        public InArgument<string> RegexPattern { get; set; }

        /// <summary>
        /// The replacement text to use. Default is string.Empty
        /// </summary>
        public InArgument<string> Replacement
        {
            get { return this.replacement; }
            set { this.replacement = value; }
        }

        /// <summary>
        /// A path to process or get. Use * for recursive folder processing. For the GetChecksum TaskAction, this indicates the path to the file to create a checksum for.
        /// </summary>
        public InOutArgument<string> Path { get; set; }

        /// <summary>
        /// The file encoding to write the new file in. The task will attempt to default to the current file encoding.
        /// </summary>
        public string TextEncoding { get; set; }

        /// <summary>
        /// The File attributes
        /// </summary>
        public string Attributes { get; set; }

        /// <summary>
        /// An ItemList of files to process. If calling SetAttributes, RemoveAttributes or AddAttributes, include the attributes in an Attributes metadata tag, separated by a semicolon.
        /// </summary>
        public InArgument<IEnumerable<string>> Files { get; set; }

        /// <summary>
        /// Sets the TargetPath for a renamed file
        /// </summary>
        public InArgument<string> TargetPath { get; set; }

        /// <summary>
        /// Gets the file checksum
        /// </summary>
        public OutArgument<string> Checksum { get; set; }

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public FileAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Forces the action on read only files.
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Date and time to use for touch. If not specify, the activity will use the current date and time.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case FileAction.GetChecksum:
                    this.GetChecksum();
                    break;
                case FileAction.Replace:
                    this.Replace();
                    break;
                case FileAction.SetAttributes:
                case FileAction.AddAttributes:
                case FileAction.RemoveAttributes:
                    this.SetAttributes();
                    break;
                case FileAction.GetTempFileName:
                    this.LogBuildMessage("Getting temp file name");
                    this.Path.Set(this.ActivityContext, System.IO.Path.GetTempFileName());
                    break;
                case FileAction.Move:
                    this.Move();
                    break;
                case FileAction.Touch:
                    this.Touch();
                    break;
                case FileAction.Delete:
                    this.Delete();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private static FileAttributes SetAttributes(string[] attributes)
        {
            FileAttributes flags = new FileAttributes();
            if (Array.IndexOf(attributes, "Archive") >= 0)
            {
                flags |= FileAttributes.Archive;
            }

            if (Array.IndexOf(attributes, "Compressed") >= 0)
            {
                flags |= FileAttributes.Compressed;
            }

            if (Array.IndexOf(attributes, "Encrypted") >= 0)
            {
                flags |= FileAttributes.Encrypted;
            }

            if (Array.IndexOf(attributes, "Hidden") >= 0)
            {
                flags |= FileAttributes.Hidden;
            }

            if (Array.IndexOf(attributes, "Normal") >= 0)
            {
                flags |= FileAttributes.Normal;
            }

            if (Array.IndexOf(attributes, "ReadOnly") >= 0)
            {
                flags |= FileAttributes.ReadOnly;
            }

            if (Array.IndexOf(attributes, "System") >= 0)
            {
                flags |= FileAttributes.System;
            }

            return flags;
        }

        private void SetAttributes()
        {
            if (this.Files == null)
            {
                this.LogBuildError("Files is required");
                return;
            }

            switch (this.Action)
            {
                case FileAction.SetAttributes:
                    this.LogBuildMessage("Setting file attributes");
                    foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
                    {
                        FileInfo afile = new FileInfo(fullfilename) { Attributes = SetAttributes(this.Attributes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) };
                    }

                    break;
                case FileAction.AddAttributes:
                    this.LogBuildMessage("Adding file attributes");
                    foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
                    {
                        FileInfo file = new FileInfo(fullfilename);
                        file.Attributes = file.Attributes | SetAttributes(this.Attributes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    break;
                case FileAction.RemoveAttributes:
                    this.LogBuildMessage("Removing file attributes");
                    foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
                    {
                        FileInfo file = new FileInfo(fullfilename);
                        file.Attributes = file.Attributes & ~SetAttributes(this.Attributes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    break;
            }
        }

        private void GetChecksum()
        {
            if (!System.IO.File.Exists(this.Path.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Invalid File passed: {0}", this.Path.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Getting Checksum for file: {0}", this.Path.Get(this.ActivityContext)));
            using (FileStream fs = System.IO.File.OpenRead(this.Path.Get(this.ActivityContext)))
            {
                using (MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider())
                {
                    byte[] hash = csp.ComputeHash(fs);
                    this.Checksum.Set(this.ActivityContext, BitConverter.ToString(hash).Replace("-", string.Empty).ToUpperInvariant());
                }
            }
        }

        private void Move()
        {
            if (!System.IO.File.Exists(this.Path.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Invalid File passed: {0}", this.Path.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Moving File: {0} to: {1}", this.Path.Get(this.ActivityContext), this.TargetPath.Get(this.ActivityContext)));

            // If the TargetPath has multiple folders, then we need to create the parent
            DirectoryInfo f = new DirectoryInfo(this.TargetPath.Get(this.ActivityContext));
            string parentPath = this.TargetPath.Get(this.ActivityContext).Replace(@"\" + f.Name, string.Empty);
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            else if (System.IO.File.Exists(this.TargetPath.Get(this.ActivityContext)))
            {
                System.IO.File.Delete(this.TargetPath.Get(this.ActivityContext));
            }

            System.IO.File.Move(this.Path.Get(this.ActivityContext), this.TargetPath.Get(this.ActivityContext));
        }

        private void Replace()
        {
            if (!string.IsNullOrEmpty(this.TextEncoding))
            {
                try
                {
                    this.fileEncoding = Encoding.GetEncoding(this.TextEncoding);
                }
                catch (ArgumentException)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "{0} is not a supported encoding name.", this.TextEncoding));
                    return;
                }
            }

            if (this.RegexPattern.Expression == null)
            {
                this.LogBuildError("RegexPattern is required.");
                return;
            }

            // Load the regex to use
            this.parseRegex = new Regex(this.RegexPattern.Get(this.ActivityContext), RegexOptions.Compiled);

            // Check to see if we are processing a file collection or a path
            if (this.Path.Expression != null)
            {
                // we need to process a path
                this.ProcessPath();
            }
            else
            {
                // we need to process a collection
                this.ProcessCollection();
            }
        }

        private void Touch()
        {
            if (this.Files == null)
            {
                this.LogBuildError("Files is required");
                return;
            }

            // get date & time for touch
            var time = this.Time == default(DateTime) ? DateTime.Now : this.Time;

            // touch each file
            foreach (var fullfilename in this.ActivityContext.GetValue(this.Files))
            {
                if (!System.IO.File.Exists(fullfilename))
                {
                    // invalid file
                    this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "Invalid File passed: {0}", fullfilename));

                    continue;
                }

                // remove readonly if needed
                var fileAttributes = System.IO.File.GetAttributes(fullfilename);
                var restoreAttributes = false;

                if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    if (!this.Force)
                    {
                        // don't touch read only file
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Skipping ReadOnly File: {0}", fullfilename));

                        continue;
                    }

                    System.IO.File.SetAttributes(fullfilename, fileAttributes & ~FileAttributes.ReadOnly);
                    restoreAttributes = true;
                }

                // touch file
                try
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Touching File: {0}", fullfilename));

                    System.IO.File.SetLastAccessTime(fullfilename, time);
                    System.IO.File.SetLastWriteTime(fullfilename, time);
                }
                finally
                {
                    // restore attributes
                    if (restoreAttributes)
                    {
                        System.IO.File.SetAttributes(fullfilename, fileAttributes);
                    }
                }
            }
        }

        private void Delete()
        {
            if (this.Files == null)
            {
                this.LogBuildError("Files is required");
                return;
            }

            // delete each file
            foreach (var fullfilename in this.ActivityContext.GetValue(this.Files))
            {
                if (!System.IO.File.Exists(fullfilename))
                {
                    // invalid file
                    this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "Invalid File passed: {0}", fullfilename));

                    continue;
                }

                // remove readonly if needed
                var fileAttributes = System.IO.File.GetAttributes(fullfilename);

                if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    if (!this.Force)
                    {
                        // don't delete read only file
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Skipping ReadOnly File: {0}", fullfilename));

                        continue;
                    }

                    System.IO.File.SetAttributes(fullfilename, fileAttributes & ~FileAttributes.ReadOnly);
                }

                // delete file
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Deleting File: {0}", fullfilename));

                System.IO.File.Delete(fullfilename);
            }
        }

        private void ProcessPath()
        {
            bool recursive = false;
            string path = this.Path.Get(this.ActivityContext);
            if (path.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                this.Path.Set(this.ActivityContext, path.Remove(path.Length - 1, 1));
                recursive = true;
            }

            // Validation
            if (Directory.Exists(this.Path.Get(this.ActivityContext)) == false)
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Path not found: {0}", this.Path.Get(this.ActivityContext)));
                return;
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Processing Path: {0} with RegEx: {1}, ReplacementText: {2}", this.Path.Get(this.ActivityContext), this.RegexPattern.Get(this.ActivityContext), this.Replacement.Get(this.ActivityContext)));

            // Check if we need to do a recursive search
            if (recursive)
            {
                // We have to do a recursive search
                // Create a new DirectoryInfo object.
                DirectoryInfo dir = new DirectoryInfo(this.Path.Get(this.ActivityContext));

                if (!dir.Exists)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The directory does not exist: {0}", this.Path.Get(this.ActivityContext)));
                    return;
                }

                // Call the GetFileSystemInfos method.
                FileSystemInfo[] infos = dir.GetFileSystemInfos("*");
                this.ProcessFolder(infos);
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(this.Path.Get(this.ActivityContext));

                if (!dir.Exists)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The directory does not exist: {0}", this.Path.Get(this.ActivityContext)));
                    return;
                }

                FileInfo[] fileInfo = dir.GetFiles();

                foreach (FileInfo f in fileInfo)
                {
                    this.ParseAndReplaceFile(f.FullName, false);
                }
            }
        }

        private void ProcessFolder(IEnumerable<FileSystemInfo> fileSysInfo)
        {
            // Iterate through each item.
            foreach (FileSystemInfo i in fileSysInfo)
            {
                // Check to see if this is a DirectoryInfo object.
                if (i is DirectoryInfo)
                {
                    // Cast the object to a DirectoryInfo object.
                    DirectoryInfo dirInfo = new DirectoryInfo(i.FullName);

                    // Iterate through all sub-directories.
                    this.ProcessFolder(dirInfo.GetFileSystemInfos("*"));
                }
                else if (i is FileInfo)
                {
                    // Check to see if this is a FileInfo object.
                    this.ParseAndReplaceFile(i.FullName, false);
                }
            }
        }

        private void ProcessCollection()
        {
            if (this.Files == null)
            {
                this.LogBuildError("No file collection has been passed");
                return;
            }

            this.LogBuildMessage("Processing File Collection");

            foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
            {
                this.ParseAndReplaceFile(fullfilename, true);
            }
        }

        private void ParseAndReplaceFile(string parseFile, bool checkExists)
        {
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Processing File: {0}", parseFile));
            if (checkExists && System.IO.File.Exists(parseFile) == false)
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "The file does not exist: {0}", parseFile));
                return;
            }

            // Open the file and attempt to read the encoding from the BOM.
            string entireFile;

            using (StreamReader streamReader = new StreamReader(parseFile, this.fileEncoding, true))
            {
                if (this.fileEncoding == null)
                {
                    this.fileEncoding = streamReader.CurrentEncoding;
                }

                entireFile = streamReader.ReadToEnd();
            }

            // Parse the entire file.
            string newFile = this.parseRegex.Replace(entireFile, this.Replacement.Get(this.ActivityContext));

            if (newFile != entireFile)
            {
                // First make sure the file is writable.
                FileAttributes fileAttributes = System.IO.File.GetAttributes(parseFile);
                bool changedAttribute = false;

                // If readonly attribute is set, reset it.
                if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Making File Writeable: {0}", parseFile));
                    System.IO.File.SetAttributes(parseFile, fileAttributes ^ FileAttributes.ReadOnly);
                    changedAttribute = true;
                }

                // Set TextEncoding if it was specified.
                if (string.IsNullOrEmpty(this.TextEncoding) == false)
                {
                    try
                    {
                        this.fileEncoding = System.Text.Encoding.GetEncoding(this.TextEncoding);
                    }
                    catch (ArgumentException)
                    {
                        this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "{0} is not a supported encoding name.", this.TextEncoding));
                        return;
                    }
                }

                // Write out the new file.
                using (StreamWriter streamWriter = new StreamWriter(parseFile, false, this.fileEncoding))
                {
                    streamWriter.Write(newFile);
                }

                if (changedAttribute)
                {
                    this.LogBuildMessage("Making file readonly", BuildMessageImportance.Low);
                    System.IO.File.SetAttributes(parseFile, FileAttributes.ReadOnly);
                }
            }
        }
    }
}