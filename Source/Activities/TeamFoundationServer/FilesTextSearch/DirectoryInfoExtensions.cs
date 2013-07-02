//-----------------------------------------------------------------------
// <copyright file="DirectoryInfoExtensions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// Contributed by Charlie Mott - http://geekswithblogs.net/charliemott/archive/2013/02/04/tfs-build-custom-activity--todo-counter.aspx
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The directory extensions.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Returns the names of files in a specified directories that match the specified patterns.
        /// </summary>
        /// <param name="source">The directory to search.</param>
        /// <param name="fileExtensions">The comma separated list of file extensions. <example>cs,cshtml</example>. </param>
        /// <param name="searchStrings">The list of search strings (ignore case).</param>
        /// <param name="searchOption">The search options. Default value is AllDirectories.</param>
        /// <returns> The list of files that match the specified criteria. </returns>
        public static List<SearchMatch> Search(this DirectoryInfo source, string fileExtensions, IEnumerable<string> searchStrings, SearchOption searchOption = SearchOption.AllDirectories)
        {
            var fileExtensionsList = fileExtensions.Split(',').Select(fileExtension => string.Format(@"*.{0}", fileExtension)).ToList();

            var filesToSearch = from searchPattern in fileExtensionsList
                                from files in Directory.GetFiles(source.FullName, searchPattern, searchOption)
                                select files;

            var matches = new List<SearchMatch>();

            foreach (var file in filesToSearch)
            {
                var lineNumber = 1;
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    lineNumber++;
                    matches.AddRange(from searchString in searchStrings
                                     where line.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                     select new SearchMatch(file, lineNumber, line.Trim()));
                }
            }

            return matches;
        }
    }
}