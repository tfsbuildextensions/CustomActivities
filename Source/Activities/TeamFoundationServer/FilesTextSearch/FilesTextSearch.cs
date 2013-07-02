//-----------------------------------------------------------------------
// <copyright file="FilesTextSearch.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// Contributed by Charlie Mott - http://geekswithblogs.net/charliemott/archive/2013/02/04/tfs-build-custom-activity--todo-counter.aspx
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.VisualBasic.Activities;

    /// <summary>
    /// Custom Activity to search specified files (defined by root directory and file extensions list) for specified strings.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [DisplayName("Files Text Search")]
    public class FilesTextSearch : CodeActivity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilesTextSearch"/> class. 
        /// </summary>
        public FilesTextSearch()
        {
            // Set Default Values.
            this.SearchDescription = "TODO Counter";
            this.BaseDirectory = new InArgument<DirectoryInfo>(new VisualBasicValue<DirectoryInfo>("New System.IO.DirectoryInfo(SourcesDirectory)"));
            this.SearchStrings = new InArgument<string[]>(new VisualBasicValue<string[]>("New String() {\"//TODO\", \"// TODO\", \"<!--TODO\", \"<!-- TODO\", \"NotImplementedException\"}"));
            this.FileExtensions = "cs,cshtml";
            this.DisplayName = "TODO Counter";
        }

        /// <summary>
        /// Gets or sets the search description.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> SearchDescription { get; set; }

        /// <summary>
        /// Gets or sets the base search directory.  All directories below this will also be searched,
        /// </summary>
        [RequiredArgument]
        public InArgument<DirectoryInfo> BaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of file extensions.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> FileExtensions { get; set; }

        /// <summary>
        /// Gets or sets the list of search strings.
        /// </summary>
        [RequiredArgument]
        public InArgument<string[]> SearchStrings { get; set; }

        /// <summary>
        /// Gets or sets the match count.
        /// </summary>
        public OutArgument<int> MatchCount { get; set; }

        /// <summary>
        /// Execute Activity
        /// </summary>
        /// <param name="context">code activity context </param>
        protected override void Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var directoryInfo = context.GetValue(this.BaseDirectory);
            if (!directoryInfo.Exists)
            {
                return;
            }

            var searchDescription = context.GetValue(this.SearchDescription);
            var fileExtensions = context.GetValue(this.FileExtensions);

            var searchStrings = context.GetValue(this.SearchStrings);
            if (searchStrings == null || searchStrings.Length == 0)
            {
                return;
            }

            var matches = directoryInfo.Search(fileExtensions, searchStrings);

            // Write to build outputs log
            context.TrackBuildMessage(string.Format("{0}: {1} items found.", searchDescription, matches.Count), BuildMessageImportance.High);
            foreach (var match in matches)
            {
                var fileAndLine = string.Format("{0} ({1})", match.File.Name, match.LineNumber);
                context.TrackBuildMessage(string.Format("{0,-50} {1}", fileAndLine, match.LineText.Trim()), BuildMessageImportance.High);
            }

            // Set output
            this.MatchCount.Set(context, matches.Count);
        }
    }
}