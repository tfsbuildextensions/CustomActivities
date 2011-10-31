//-----------------------------------------------------------------------
// <copyright file="VBPProject.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Internal class to wrap a VB6 project file
    /// </summary>
    internal class VBPProject
    {
        private readonly List<string> Lines = new List<string>();
        private string projectFile;

        /// <summary>
        /// Initializes a new instance of the VBPProject class. Wrapper class to parse and edit a VB6 project file
        /// </summary>
        public VBPProject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the VBPProject class. Wrapper class to parse and edit a VB6 project file
        /// </summary>
        /// <param name="projectFileExt">Project filename</param>
        public VBPProject(string projectFileExt)
        {
            this.ProjectFile = projectFileExt;
        }

        /// <summary>
        /// VB6 project file
        /// </summary>
        public string ProjectFile
        {
            get
            {
                return this.projectFile;
            }

            set
            {
                if (!File.Exists(value))
                {
                    throw new ApplicationException("Project file name does not exist");
                }

                this.projectFile = value;
            }
        }

        /// <summary>
        /// Method to load a VB6 project file
        /// </summary>
        /// <returns>True if project was loaded successfully, false otherwise</returns>
        public bool Load()
        {
            if (string.IsNullOrEmpty(this.ProjectFile))
            {
                return false;
            }

            StreamReader lineStream = null;
            try
            {
                lineStream = new StreamReader(this.projectFile, Encoding.Default);
                while (!lineStream.EndOfStream)
                {
                    this.Lines.Add(lineStream.ReadLine());
                }
            }
            catch 
            {
                // intended
            }
            finally
            {
                if (lineStream != null)
                {
                    lineStream.Close();
                }
            }

            return true;
        }

        /// <summary>
        /// Method to save a VB6 project file
        /// </summary>
        /// <returns>True if project was loaded successfully, false otherwise</returns>
        public bool Save()
        {
            if (string.IsNullOrEmpty(this.projectFile) | this.Lines.Count == 0)
            {
                return false;
            }

            StreamWriter lineStream = null;
            bool readOnly = false;
            try
            {
                if ((File.GetAttributes(this.projectFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    readOnly = true;
                    File.SetAttributes(this.projectFile, FileAttributes.Normal);
                }

                lineStream = new StreamWriter(this.projectFile, false, Encoding.Default);
                foreach (string line in this.Lines)
                {
                    lineStream.WriteLine(line);
                }
            }
            catch
            {
                // intended
            }
            finally
            {
                if (lineStream != null)
                {
                    lineStream.Close();
                }

                if (readOnly)
                {
                    File.SetAttributes(this.projectFile, FileAttributes.ReadOnly);
                }
            }

            return true;
        }

        /// <summary>
        /// Sets a VB6 project property
        /// </summary>
        /// <param name="name">Name of property to set</param>
        /// <param name="value">Value to set</param>
        /// <param name="addProp">True if property should be created</param>
        /// <returns>bool</returns>
        public bool SetProjectProperty(string name, string value, bool addProp)
        {
            if (string.IsNullOrEmpty(name) | string.IsNullOrEmpty(value))
            {
                return false;
            }

            int index;

            for (index = 0; index <= this.Lines.Count - 1; index++)
            {
                string buffer = this.Lines[index].ToUpper(CultureInfo.InvariantCulture);

                if (buffer.StartsWith(name.ToUpper(CultureInfo.InvariantCulture) + "=", StringComparison.OrdinalIgnoreCase))
                {
                    this.Lines[index] = this.Lines[index].Substring(0, (name + "=").Length) + value;
                    return true;
                }
            }

            if (addProp)
            {
                this.Lines.Add(name + "=" + value);
                return true;
            }

            return false;
        }
    }
}
