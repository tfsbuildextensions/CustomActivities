//-----------------------------------------------------------------------
// <copyright file="SSHAuthentication.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    /// <summary>
    /// Class the represents the authentication information used by the varias 
    /// PuTTY related activities
    /// </summary>
    public class SSHAuthentication
    {
        /// <summary>
        /// Defines the method used to authenticate against the server.
        /// </summary>
        public SSHAuthenticationType AuthType { get; set; }

        /// <summary>
        /// The login of the user in which name the operation will be performed
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The password if using username/password authentication  or the private key if using key authentication
        /// The key can be file in the filesystem or stored in source control (<example>$/TeamProject/Folder/private.ppk</example>
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// This is an internal member that will hold the physical location
        /// of the private key (if auth is of private key) if the private 
        /// key file is stored in source control, the file will be downloaded
        /// into a temporary file and it's true location will be stored on
        /// this member
        /// </summary>
        internal string PrivateKeyFileLocation { get; set; }
    }
}
