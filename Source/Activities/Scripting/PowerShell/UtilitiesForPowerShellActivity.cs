//-----------------------------------------------------------------------
// <copyright file="UtilitiesForPowerShellActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Scripting
{
    using System;
    using System.IO;
    using Microsoft.TeamFoundation.VersionControl.Common;

    /// <summary>
    /// The set of methods used to resolve scripts from source control or local file system
    /// </summary>
    public class UtilitiesForPowerShellActivity : IUtilitiesForPowerShellActivity
    {
        /// <summary>
        /// Checks if the path is a valid file under source control
        /// </summary>
        /// <param name="path">Path to source control in for '$/aaa/file.cs'</param>
        /// <returns>True if file found</returns>
        public bool IsServerItem(string path)
        {
            return VersionControlPath.IsServerItem(path);
        }

        /// <summary>
        /// Finds the local path for a server file path
        /// </summary>
        /// <param name="workspace">The current TFS workspace</param>
        /// <param name="fileName">The TFS server path</param>
        /// <returns>The local file path</returns>
        public string GetLocalFilePathFromWorkspace(Microsoft.TeamFoundation.VersionControl.Client.Workspace workspace, string fileName)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException("workspace");
            }

            return workspace.GetLocalItemForServerItem(fileName);
        }

        /// <summary>
        /// Checks if a file exists on local file system
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>True if file found</returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
