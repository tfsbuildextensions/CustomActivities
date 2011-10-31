//-----------------------------------------------------------------------
// <copyright file="SSHCopyProtocol.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    /// <summary>
    /// Represents the protocol used to copy files
    /// </summary>
    public enum SSHCopyProtocol
    {
        /// <summary>
        /// Copy files using scp (mandatory if you only have ssh v1 in the server)
        /// </summary>
        scp = 0, 

        /// <summary>
        /// Copy files using sftp (prefered method)
        /// </summary>
        sftp
    }
}