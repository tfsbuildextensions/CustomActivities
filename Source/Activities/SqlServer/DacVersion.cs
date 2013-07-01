//-----------------------------------------------------------------------
// <copyright file="DacVersion.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SqlServer
{
    using System;
    using System.Activities;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Updates the DacVersion version number in any .sqlproj file inside a solution.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class DacVersion : BaseCodeActivity<string>
    {
        /// <summary>
        /// The absolute path to the .sqlproj file in solution
        /// </summary>
        public InArgument<string> SqlProjFilePath { get; set; }

        /// <summary>
        /// A valid version number to set the DacVersion element of a sqlproj file
        /// </summary>
        public InArgument<string> Version { get; set; }

        /// <summary>
        /// InternalExecute
        /// </summary>
        /// <returns>string</returns>
        protected override string InternalExecute()
        {
            string path = this.SqlProjFilePath.Get(this.ActivityContext);
            string fileStr = string.Empty;
            foreach (var line in File.ReadLines(path))
            {
                if (line.Trim().Contains("<DacVersion>"))
                {
                    string currLine = line.Trim();
                    int i = currLine.IndexOf("<DacVersion>", StringComparison.OrdinalIgnoreCase);
                    int j = currLine.IndexOf("</DacVersion>", StringComparison.OrdinalIgnoreCase) + "</DacVersion>".Length;
                    string currDacVersionElement = currLine.Substring(i, j);
                    fileStr += currLine.Replace(currDacVersionElement, "<DacVersion>" + this.Version.Get(this.ActivityContext) + "</DacVersion>");
                }
                else
                {
                    fileStr += line;
                }
            }

            // write to file (unset and set back ReadOnly attribute if present).
            var fileAttributes = File.GetAttributes(path);
            var attributesChanged = false;
            if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, fileAttributes ^ FileAttributes.ReadOnly);
                attributesChanged = true;
            }

            using (var file = new StreamWriter(path))
            {
                file.Write(fileStr);
            }

            if (attributesChanged)
            {
                File.SetAttributes(path, FileAttributes.ReadOnly);
            }

            return File.ReadAllText(path);
        }
    }
}
