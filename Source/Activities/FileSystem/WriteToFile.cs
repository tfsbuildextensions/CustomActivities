//-----------------------------------------------------------------------
// <copyright file="WriteToFile.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.FileSystem
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Writes the content passed into a file (tipically a line).
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class WriteToFile : CodeActivity
    {
        /// <summary>
        /// The filename where the content will be written
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Description("File Name")]
        [RequiredArgument()]
        public InArgument<string> FileName { get; set; }

        /// <summary>
        /// Should we create a new file or append to an existing one (if it doesn't exist it will be created)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Description("Create New File. If False appends")]
        public InArgument<bool> Create { get; set; }

        /// <summary>
        /// The content to be written to the fileName
        /// </summary>
        public InArgument<string> Content { get; set; }

        /// <summary>
        /// Automatically write a new line after the content?
        /// </summary>
        public InArgument<bool> AutoNewLine { get; set; }
        
        /// <summary>
        /// Executes the activity code. 
        /// Writes the desirad content to the file.
        /// Overwrites or appends the content if the file exists
        /// </summary>
        /// <param name="context">the activity context</param>
        protected override void Execute(CodeActivityContext context)
        {
            string fileName;
            bool createFile, autoNewLine;
            string content;

            fileName = this.FileName.Get(context);
            createFile = this.Create.Get(context);
            content = this.Content.Get(context);
            autoNewLine = this.AutoNewLine.Get(context);

            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("You have to define the FileName");
            }

            using (var streamWriter = new StreamWriter(fileName, createFile == false))
            {
                if (autoNewLine)
                {
                    streamWriter.WriteLine(content);
                }
                else
                {
                    streamWriter.Write(content);
                }
            }
        }
    }
}
