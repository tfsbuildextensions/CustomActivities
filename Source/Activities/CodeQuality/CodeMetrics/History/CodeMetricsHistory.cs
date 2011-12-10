//-----------------------------------------------------------------------
// <copyright file="CodeMetricsHistory.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.History
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.CodeQuality.Proxy;

    /// <summary>
    /// Activity to push the Code Metrics produce by the <see cref="CodeMetrics"/> activity in an history folder.
    /// <para/>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <tach:CodeMetricsHistory Enabled="[True]" 
    ///                          SourceFileName="[String.Format(&quot;{0}\Metrics.xml&quot;,BinariesDirectory)]"
    ///                          HistoryDirectory="[\\Server\CodeMetricsHistory]" 
    ///                          HistoryFileName="[String.Format(&quot;Metrics_{0}.xml&quot;, BuildNumber)]"
    ///                          HowManyFilesToKeepInDirectory="[50]" />
    /// ]]></code>    
    /// </example>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class CodeMetricsHistory : BaseCodeActivity
    {
        private readonly bool dependenciesCameFromExternalSource;
        private IActivityContextProxy proxyContext;
        private IFileSystemProxy proxyFileSystem;
        private IParametersValidations validations;

        public CodeMetricsHistory()
        {
            this.dependenciesCameFromExternalSource = false;
        }

        public CodeMetricsHistory(IActivityContextProxy proxyContext, IFileSystemProxy proxyFileSystem, IParametersValidations validations)
        {
            this.dependenciesCameFromExternalSource = true;
            this.InitializeDependencies(proxyContext, proxyFileSystem, validations);
        }

        /// <summary>
        /// Enable Code Metrics history
        /// </summary>
        [Description("Enable Code Metrics history")]
        [RequiredArgument, DefaultValue(false)]
        public InArgument<bool> Enabled { get; set; }

        /// <summary>
        /// The full filename where the metrics has been written by the Code Metrics activity.
        /// </summary>
        [Description("The full filename where the metrics has been written by the Code Metrics activity.")]
        [RequiredArgument]
        public InArgument<string> SourceFileName { get; set; }

        /// <summary>
        /// The directory where the metrics files will be copied.  This directory should contains only those files.
        /// </summary>
        [Description("The directory where the metrics files will be copied.  This directory should contains only those files.")]
        [RequiredArgument]
        public InArgument<string> HistoryDirectory { get; set; }

        /// <summary>
        /// The destination filename for the metrics file.  This name has to be unique for each iteration, history.
        /// Something like this String.Format(&quot;Metrics_{0}.xml&quot;, BuildNumber)
        /// </summary>
        [Description("The destination filename for the metrics file.  This name has to be unique for each iteration, history.  Example: String.Format(&quot;Metrics_{0}.xml&quot;, BuildNumber)")]
        [RequiredArgument]
        public InArgument<string> HistoryFileName { get; set; }

        /// <summary>
        /// How many files to keep in history before removing the oldest.  Default:50
        /// </summary>
        [Description("How many files to keep in history before removing the oldest.  Default:50")]
        [RequiredArgument, DefaultValue(50)]
        public InArgument<short> HowManyFilesToKeepInDirectory { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            this.InitializeMissingDependencies();
            if (this.proxyContext.Enabled && this.validations.ParametersAreValid())
            {
                this.PurgeOldHistory();
                this.ValidateDestinationFileAndCopy();
            }
        }

        /// <summary>
        /// Override for base.CacheMetadata
        /// </summary>
        /// <param name="metadata">CodeActivityMetadata</param>
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension(typeof(IBuildDetail));
        }

        private void InitializeMissingDependencies()
        {
            if (!this.dependenciesCameFromExternalSource)
            {
                this.InitializeDependencies(new ActivityContextProxy(this, this.ActivityContext), new FileSystemProxy());
            }
        }

        private void ValidateDestinationFileAndCopy()
        {
            var destinationFilename = this.BuildDestinationFilename();

            if (this.DestinationFileNameIsValid(destinationFilename))
            {
                this.proxyFileSystem.CopyFile(this.proxyContext.SourceFileName, destinationFilename);
                this.proxyContext.LogBuildMessage(string.Format("The file '{0}' has been successfully transfered to the new history file '{1}'", this.proxyContext.SourceFileName, destinationFilename));
            }
        }

        private void PurgeOldHistory()
        {
            var existingFilenames = this.proxyFileSystem.EnumerateFiles(this.proxyContext.HistoryDirectory);

            if (existingFilenames != null && existingFilenames.Count() >= this.proxyContext.HowManyFilesToKeepInDirectory)
            {
                var filesWithLastWrite = this.GetFilesOrderedByLastWriteDescending(existingFilenames);
                this.DeleteOldestFilesUntilUnderThreshold(filesWithLastWrite);
            }
        }

        private void DeleteOldestFilesUntilUnderThreshold(IEnumerable<Tuple<string, DateTime>> filesWithLastWrite)
        {
            for (var i = filesWithLastWrite.Count(); i >= this.proxyContext.HowManyFilesToKeepInDirectory; i--)
            {
                this.proxyFileSystem.DeleteFile(filesWithLastWrite.ElementAt(i - 1).Item1);
            }
        }

        private IEnumerable<Tuple<string, DateTime>> GetFilesOrderedByLastWriteDescending(IEnumerable<string> existingFilenames)
        {
            var filesWithLastWrite = existingFilenames.Select(filename => Tuple.Create(filename, this.proxyFileSystem.GetLastWriteTime(filename)));

            return filesWithLastWrite.OrderByDescending(x => x.Item2);
        }

        private bool DestinationFileNameIsValid(string destinationFilename)
        {
            if (this.proxyFileSystem.FileExists(destinationFilename))
            {
                this.FailCurrentBuild(string.Format("The history (destination) filename already exists [{0}]", destinationFilename));
                return false;
            }

            return true;
        }

        private string BuildDestinationFilename()
        {
            return Path.Combine(this.proxyContext.HistoryDirectory, this.proxyContext.HistoryFileName);
        }

        private void FailCurrentBuild(string msg)
        {
            this.proxyContext.BuildDetail.Status = BuildStatus.Failed;
            this.proxyContext.BuildDetail.Save();
            this.proxyContext.LogBuildError(msg);
        }

        private void InitializeDependencies(IActivityContextProxy vproxyContext, IFileSystemProxy vproxyFileSystem)
        {
            this.InitializeDependencies(vproxyContext, vproxyFileSystem, new ParametersValidations(vproxyContext, vproxyFileSystem));
        }

        private void InitializeDependencies(IActivityContextProxy vproxyContext, IFileSystemProxy vproxyFileSystem, IParametersValidations vvalidations)
        {
            this.proxyContext = vproxyContext;
            this.proxyFileSystem = vproxyFileSystem;
            this.validations = vvalidations;
        }
    }
}
