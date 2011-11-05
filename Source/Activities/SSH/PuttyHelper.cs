//-----------------------------------------------------------------------
// <copyright file="PuttyHelper.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SSH
{
    using System;
    using System.IO;

    internal static class PuttyHelper
    {
        public static string GetPuttyPath(string toolsPath)
        {
            string puttyPath;

            if (string.IsNullOrEmpty(toolsPath))
            {
                if (Environment.Is64BitProcess)                    
                {
                    puttyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PuTTY");

                    if (Directory.Exists(puttyPath))
                    {
                        return puttyPath;
                    }
                }
                
                puttyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PuTTY");

                if (Directory.Exists(puttyPath))
                {
                    return puttyPath;
                }
            }

            return toolsPath ?? string.Empty;
        }

        /// <summary>
        /// Constructs the string with the command line parameters
        /// related to authentication
        /// </summary>
        /// <param name="auth">authentication parameters</param>
        /// <returns>The command line parameters</returns>
        internal static string GetAuthenticationParameters(SSHAuthentication auth)
        {
            switch (auth.AuthType)
            {
                case SSHAuthenticationType.UserNamePassword: return string.Format("-l {0} -pw \"{1}\"", auth.User, auth.Key);
                case SSHAuthenticationType.PrivateKey: return string.Format("-l {0} -i \"{1}\"", auth.User, auth.PrivateKeyFileLocation);
                default: throw new NotImplementedException("Unknown authentication type");
            }
        }
    }
}
