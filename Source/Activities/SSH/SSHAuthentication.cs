//-----------------------------------------------------------------------
// <copyright file="SSHAuthentication.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    /// <summary>
    /// Class the represents the authentication informat used by the varias PuTTY related activities
    /// </summary>
    public class SSHAuthentication
    {
        /// <summary>
        /// The login of the user in which name the operation will be performed
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The password of the user in which name the operation will be performed
        /// </summary>
        public string Password { get; set; }
    }
}
