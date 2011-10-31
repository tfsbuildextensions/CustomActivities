//-----------------------------------------------------------------------
// <copyright file="SharePointDeploymentStatus.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SharePoint
{
    using System;

    /// <summary>
    /// The data returned from a Sharepoint powershell activiy
    /// </summary>
    public class SharePointDeploymentStatus
    {
        /// <summary>
        /// Gets or sets the name of the feature or solution
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or set the ID of the frayure or solution
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or set if the solutions is deloyed (always returns true for features)
        /// </summary>
        public bool Deployed { get; set; }

        /// <summary>
        /// A string representation of the stored data
        /// </summary>
        /// <returns>A list of values</returns>
        public override string ToString()
        {
            return string.Format("Name: [{0}], ID [{1}], Deployed [{2}]", this.Name, this.Id, this.Deployed);
        }
    }
}
