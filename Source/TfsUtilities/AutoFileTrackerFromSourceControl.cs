//-----------------------------------------------------------------------
// <copyright file="AutoFileTrackerFromSourceControl.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.TfsUtilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.VersionControl.Common;

    /// <summary>
    /// Auto deleteable temporary file fetched from TFS
    /// <para></para>
    /// Returns a temporary file that is deleted automatically when dispose is called.
    /// <para></para>
    /// If you need to use temporary files,wrap its use with a using command so no care is necessary to
    /// delete temporary left over files.
    /// It can also be used with filesystem file references. In that case no action is performed on the file
    /// </summary>
    public class AutoFileTrackerFromSourceControl : IDisposable
    {
        private bool isDisposed;
        
        /// <summary>
        /// Contains the file paths of the files we are tracking.
        /// When the class is disposed tracked files are deleted.
        /// </summary>
        private Dictionary<string, string> trackedFiles = new Dictionary<string, string>();
                
        private TfsTeamProjectCollection tfsProjectCollection;
                
        /// <summary>
        /// Initializes a new instance of the AutoFileTrackerFromSourceControl class.
        /// Tracks a file reference.
        /// <para></para>
        /// If the the file is referencing a source control item the file content 
        /// is downloaded an download file reference will be returned.
        /// When the end of life of the object is reached the (temporary) file 
        /// will be automaticaly deleted. 
        /// If a filesystem reference is passed then the file is unaltered.
        /// </summary>
        /// <param name="tfs">The project collection from which we should delete files from source control</param>
        public AutoFileTrackerFromSourceControl(TfsTeamProjectCollection tfs)
        {
            this.tfsProjectCollection = tfs;
        }

        private AutoFileTrackerFromSourceControl()
        {
        }

        /// <summary>
        /// Finalizes an instance of the AutoFileTrackerFromSourceControl class.
        /// </summary>
        ~AutoFileTrackerFromSourceControl()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets a file name. If the file references a file in source control a temporary file 
        /// is created by download the content. Repeated calls to the same file will allways return
        /// the same temporary file (the file will not be download again from source control).
        /// The temporary file will be deleted when the instance of the class is disposed. Non source
        /// control files will be left untouched
        /// </summary>
        /// <param name="filePath">The path to track. May be a file from source control in the format $/project/path/filename.txt or
        /// a file in the filesystem.</param>
        /// <returns>the filename of the file. The filename itself if it's a file in the filesystem or a reference to the temporary file if a file is in source control</returns>
        public string GetFile(string filePath)
        {
            if (VersionControlPath.IsServerItem(filePath))
            {
                if (this.trackedFiles.ContainsKey(filePath))
                {
                    return this.trackedFiles[filePath];
                }

                return this.trackedFiles[filePath] = this.DownloadToTemporaryFile(filePath);
            }

            return filePath;
        }
        
        /// <summary>
        /// Deletes the (tracked) file(s) (if they still exist)
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deletes the tracked files if they still exist
        /// </summary>
        /// <param name="disposing">disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed == false)
            {
                this.DeleteFiles();
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Deletes the files that are referenced
        /// </summary>
        private void DeleteFiles()
        {
            foreach (var tmpFile in this.trackedFiles)
            {
                if (File.Exists(tmpFile.Value))
                {
                    File.Delete(tmpFile.Value);
                }
            }
        }

        /// <summary>
        /// Downloads the content of a file from source control into a temporary file
        /// </summary>
        /// <param name="serverPath">The file to download from source control</param>
        /// <returns>The name of the temporary file</returns>
        private string DownloadToTemporaryFile(string serverPath)
        {
            var vcs = this.tfsProjectCollection.GetService<VersionControlServer>();

            var tmpFileName = Path.GetTempFileName();

            vcs.DownloadFile(serverPath, tmpFileName);

            return tmpFileName;
        }
    }
}
