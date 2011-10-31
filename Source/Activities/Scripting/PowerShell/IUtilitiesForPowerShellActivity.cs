//-----------------------------------------------------------------------
// <copyright file="IUtilitiesForPowerShellActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Scripting
{
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// The set of methods used to resolve scripts from source control or local file system
    /// </summary>
    public interface IUtilitiesForPowerShellActivity
    {
        /// <summary>
        /// Checks if the path is a valid file under source control
        /// </summary>
        /// <param name="path">Path to source control in for '$/aaa/file.cs'</param>
        /// <returns>True if file found</returns>
        bool IsServerItem(string path);

        /// <summary>
        /// Finds the local path for a server file path
        /// </summary>
        /// <param name="workspace">The current TFS workspace</param>
        /// <param name="fileName">The TFS server path</param>
        /// <returns>The local file path</returns>
        string GetLocalFilePathFromWorkspace(Workspace workspace, string fileName);

        /// <summary>
        /// Checks if a file exists on local file system
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>True if file found</returns>
        bool FileExists(string path);
    }
}
