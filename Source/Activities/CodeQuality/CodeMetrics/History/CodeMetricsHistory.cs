//-----------------------------------------------------------------------
// <copyright file="CodeMetricsHistory.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

using TfsBuildExtensions.Activities.CodeQuality.Proxy;

namespace TfsBuildExtensions.Activities.CodeQuality.History
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    

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
        private readonly bool _dependenciesCameFromExternalSource;
        private IActivityContextProxy _proxyContext;
        private IFileSystemProxy _proxyFileSystem;
        private IParametersValidations _validations;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CodeMetricsHistory()
        {
            _dependenciesCameFromExternalSource = false;
        }

        /// <summary>
        /// Constructor to inject dependencies (coupling concerns)
        /// </summary>
        public CodeMetricsHistory(IActivityContextProxy proxyContext, IFileSystemProxy proxyFileSystem, IParametersValidations validations)
        {
            _dependenciesCameFromExternalSource = true;
            InitializeDependencies(proxyContext, proxyFileSystem, validations);
        }

        /// <summary>
        /// Enable Code Metrics history
        /// </summary>
        [Description("Enable Code Metrics history")]
        [RequiredArgument, DefaultValue(false)]
        public InArgument<Boolean> Enabled { get; set; }

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
        public InArgument<Int16> HowManyFilesToKeepInDirectory { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            InitializeMissingDependencies();
            if (_proxyContext.Enabled && _validations.ParametersAreValid())
            {
                PurgeOldHistory();
                ValidateDestinationFileAndCopy();
            }
        }

        private void InitializeMissingDependencies()
        {
            if (!_dependenciesCameFromExternalSource)
                InitializeDependencies(new ActivityContextProxy(this, this.ActivityContext), new FileSystemProxy());
        }

        private void ValidateDestinationFileAndCopy()
        {
            var destinationFilename = BuildDestinationFilename();

            if (DestinationFileNameIsValid(destinationFilename))
            {
                _proxyFileSystem.CopyFile(_proxyContext.SourceFileName, destinationFilename);
                _proxyContext.LogBuildMessage(string.Format("The file '{0}' has been successfully transfered to the new history file '{1}'", _proxyContext.SourceFileName, destinationFilename));
            }
        }

        private void PurgeOldHistory()
        {
            var existingFilenames = _proxyFileSystem.EnumerateFiles(_proxyContext.HistoryDirectory);

            if (existingFilenames != null && existingFilenames.Count() >= _proxyContext.HowManyFilesToKeepInDirectory)
            {
                var filesWithLastWrite = GetFilesOrderedByLastWriteDescending(existingFilenames);
                DeleteOldestFilesUntilUnderThreshold(filesWithLastWrite);
            }
        }

        private void DeleteOldestFilesUntilUnderThreshold(IEnumerable<Tuple<string, DateTime>> filesWithLastWrite)
        {
            for (var i = filesWithLastWrite.Count(); i >= _proxyContext.HowManyFilesToKeepInDirectory; i--)
            {
                _proxyFileSystem.DeleteFile(filesWithLastWrite.ElementAt(i - 1).Item1);
            }
        }

        private IEnumerable<Tuple<string, DateTime>> GetFilesOrderedByLastWriteDescending(IEnumerable<string> existingFilenames)
        {
            var filesWithLastWrite = existingFilenames.Select(filename => Tuple.Create(filename, _proxyFileSystem.GetLastWriteTime(filename)));

            return filesWithLastWrite.OrderByDescending(x => x.Item2);
        }

        private bool DestinationFileNameIsValid(string destinationFilename)
        {
            if (_proxyFileSystem.FileExists(destinationFilename))
            {
                FailCurrentBuild(string.Format("The history (destination) filename already exists [{0}]", destinationFilename));
                return false;
            }
            return true;
        }

        private string BuildDestinationFilename()
        {
            return Path.Combine(_proxyContext.HistoryDirectory, _proxyContext.HistoryFileName);
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

        private void FailCurrentBuild(string msg)
        {
            _proxyContext.BuildDetail.Status = BuildStatus.Failed;
            _proxyContext.BuildDetail.Save();
            _proxyContext.LogBuildError(msg);
        }

        private void InitializeDependencies(IActivityContextProxy proxyContext, IFileSystemProxy proxyFileSystem)
        {
            InitializeDependencies(proxyContext, proxyFileSystem, new ParametersValidations(proxyContext, proxyFileSystem));
        }

        private void InitializeDependencies(IActivityContextProxy proxyContext, IFileSystemProxy proxyFileSystem, IParametersValidations validations)
        {
            _proxyContext = proxyContext;
            _proxyFileSystem = proxyFileSystem;
            _validations = validations;
        }
    }
}
