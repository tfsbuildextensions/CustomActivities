//-----------------------------------------------------------------------
// <copyright file="DacVersion.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SqlServer
{
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Updates the DacVersion version number in any .sqlproj file inside a solution.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class DacVersion : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the files where the DacVersion tag is to be updated.
        /// </summary>
        public InArgument<IEnumerable<string>> Files { get; set; }

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
        protected override void InternalExecute()
        {
            if (string.IsNullOrWhiteSpace(this.ActivityContext.GetValue(this.Version)))
            {
                this.LogBuildError("Version is required.");
                return;
            }

            IEnumerable<string> filesPaths = this.ActivityContext.GetValue(this.Files);
            if (null != filesPaths && filesPaths.Any())
            {
                foreach (string filePath in filesPaths)
                {
                    UpdateDacVersionInFile(filePath);
                }
            }
            else
            {
                string path = this.ActivityContext.GetValue(this.SqlProjFilePath);
                if (string.IsNullOrWhiteSpace(path))
                {
                    this.LogBuildError("If a list of file names is not given for Files, SqlProjFilePath is required.");
                }

                this.UpdateDacVersionInFile(path);
            }
        }

        private void UpdateDacVersionInFile(string filePath)
        {
            // First make sure the file is writable.
            FileAttributes fileAttributes = File.GetAttributes(filePath);
            var changedAttribute = false;

            // If readonly attribute is set, reset it.
            if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                this.LogBuildMessage("Making file writable", BuildMessageImportance.Low);
                File.SetAttributes(filePath, fileAttributes ^ FileAttributes.ReadOnly);
                changedAttribute = true;
            }

            var document = new XmlDocument();
            document.Load(filePath);

            var versionNodes = document.GetElementsByTagName("DacVersion", "http://schemas.microsoft.com/developer/msbuild/2003");
            foreach (XmlNode node in versionNodes)
            {
                node.InnerText = this.ActivityContext.GetValue(this.Version);
            }

            document.Save(filePath);

            if (changedAttribute)
            {
                this.LogBuildMessage("Making file readonly", BuildMessageImportance.Low);
                File.SetAttributes(filePath, FileAttributes.ReadOnly);
            }
        }
    }
}
