//-----------------------------------------------------------------------
// <copyright file="SSHAuthenticationType.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    /// <summary>
    /// What kind of authentication do we want to run agains the
    /// server
    /// </summary>
    public enum SSHAuthenticationType
    {
        /// <summary>
        /// Authenticate against the server using a username and a password.
        /// Default method
        /// </summary>
        UserNamePassword = 0,

        /// <summary>
        /// Authenticate against the server using a private key
        /// </summary>
        PrivateKey
    }
}
