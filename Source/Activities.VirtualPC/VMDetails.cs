//-----------------------------------------------------------------------
// <copyright file="VMDetails.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Virtualization.Extended
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds details of a virtual machine
    /// </summary>
    public class VMDetails
    {
         private Dictionary<string, string> metadata = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the VMDetails class
           /// </summary>
        /// <param name="name">The name of the virtual machine</param>
        public VMDetails(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// The name of the virtual machine
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Sets the value of a property of the virtual PC
        /// </summary>
        /// <param name="key">The name of the value</param>
        /// <param name="value">The value</param>
        public void SetProperty(string key, string value)
        {
            this.metadata.Add(key, value);
        }

        /// <summary>
        /// Gets the value of a property of the virtual PC
        /// </summary>
        /// <param name="key">The name of the value</param>
        /// <returns>The value</returns>
        public string GetProperty(string key)
        {
            return this.metadata[key];
        }
    }
}
