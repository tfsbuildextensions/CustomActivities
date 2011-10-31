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

            return toolsPath ?? String.Empty;
        }
    }
}
