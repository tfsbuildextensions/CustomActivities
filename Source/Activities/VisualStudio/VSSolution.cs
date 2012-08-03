//-----------------------------------------------------------------------
// <copyright file="VSSolution.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class VSSolution
    {
        private static readonly Type SolutionParser;
        private static readonly PropertyInfo SolutionParserSolutionReader;
        private static readonly MethodInfo SolutionParserParseSolution;
        private static readonly PropertyInfo SolutionParserVersion;

        private readonly string solutionPath;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Would lead to null pointer exceptions if .Net 4.0 not installed")]
        static VSSolution()
        {
            SolutionParser = Type.GetType("Microsoft.Build.Construction.SolutionParser, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false, false);

            if (SolutionParser == null)
            {
                return;
            }

            SolutionParserSolutionReader = SolutionParser.GetProperty("SolutionReader", BindingFlags.NonPublic | BindingFlags.Instance);
            SolutionParserParseSolution = SolutionParser.GetMethod("ParseSolution", BindingFlags.NonPublic | BindingFlags.Instance);
            SolutionParserVersion = SolutionParser.GetProperty("Version", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public VSSolution(string solutionFileName)
        {
            if (SolutionParser == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.SolutionParser' .Net 4.0 or higher not installed?");
            }

            if (string.IsNullOrEmpty(solutionFileName))
            {
                throw new ArgumentException("Solution File Name Not supplied", "solutionFileName");
            }

            if (File.Exists(solutionFileName) == false)
            {
                throw new FileNotFoundException("file not found", solutionFileName);
            }

            this.FileName = solutionFileName;

            this.solutionPath = Path.GetDirectoryName(solutionFileName);

            if (Path.IsPathRooted(this.solutionPath) == false)
            {
                throw new NotSupportedException("relative paths not supported");
            }

            var solutionParserInstance = SolutionParser.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First().Invoke(null);

            using (var streamReader = new StreamReader(this.FileName))
            {
                SolutionParserSolutionReader.SetValue(solutionParserInstance, streamReader, null);
                SolutionParserParseSolution.Invoke(solutionParserInstance, null);
            }

            var intObject = SolutionParserVersion.GetValue(solutionParserInstance, null);

            if (intObject != null)
            {
                this.Version = Convert.ToInt32(intObject);
            }

            if (this.Version < 7)
            {
                return;
            }
        }

        /// <summary>
        /// The internal version on the solution fileName
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// The version number as people are using to refer to (marketing name)
        /// </summary>
        public VSVersionInternal FriendlyVersion
        {
            get
            {
                if (this.Version > 12)
                {
                    return VSVersionInternal.VSNext;
                }

                if (this.Version == 12)
                {
                    return VSVersionInternal.VS2012;
                }

                if (this.Version == 11)
                {
                    return VSVersionInternal.VS2010;
                }

                if (this.Version == 10)
                {
                    return VSVersionInternal.VS2008;
                }

                if (this.Version == 9)
                {
                    return VSVersionInternal.VS2005;
                }

                if (this.Version == 8)
                {
                    return VSVersionInternal.VSNet2003;
                }

                if (this.Version == 7)
                {
                    return VSVersionInternal.VSNet2002;
                }

                return VSVersionInternal.Previous;
            }
        }

        /// <summary>
        /// Filename of the solution
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Checks if we can auto detect the version of the FileName
        /// <para></para>
        /// Only solution (.sln) files are supported
        /// </summary>
        /// <param name="filePath">The path (with filename) to check</param>
        /// <returns>true if we can auto detect the version false otherwise</returns>
        internal static bool CanAutoDetectVersion(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension != null && extension.ToLower() == ".sln")
            {
                return true;
            }

            return false;
        }
    }
}
