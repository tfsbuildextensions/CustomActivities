//-----------------------------------------------------------------------
// <copyright file="CodeMetricsFilesToProcess.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TfsBuildExtensions.Activities.CodeQuality.Proxy;

namespace TfsBuildExtensions.Activities.CodeQuality
{

    /// <summary>
    /// Class to manage the files the CodeMetrics activity should process
    /// </summary>
    public class CodeMetricsFilesToProcess
    {
        private readonly IActivityContextProxy _activityProxy;
        private readonly IFileSystemProxy _fileSystemProxy;

        /// <summary>
        /// Constructor to build new instance within an activity context.
        /// </summary>
        public CodeMetricsFilesToProcess(CodeMetrics activity, CodeActivityContext context) : 
               this(new ActivityContextProxy(activity, context), new FileSystemProxy())
        {
        }

        /// <summary>
        /// Constructor to build new instance while receiving the dependencies (for decoupling concerns)
        /// </summary>
        public CodeMetricsFilesToProcess(IActivityContextProxy activityProxy, IFileSystemProxy fileSystemProxy)
        {
            _activityProxy = activityProxy;
            _fileSystemProxy = fileSystemProxy;
        }

        /// <summary>
        /// Get the files to process for metric analysis with the exclusion of those set to be ignored.
        /// </summary>
        public IEnumerable<string> Get()
        {
            IEnumerable<String> files = _activityProxy.FilesToProcess;

            if (IsNotEmpty(files))
                RemovesFilesToIgnore(ref files);
            else
                files = new List<string> {"*.dll", "*.exe"};
            LogFiles(files);
            return files;
        }

        private void RemovesFilesToIgnore(ref IEnumerable<string> files)
        {
            if (IsNotEmpty(_activityProxy.FilesToIgnore))
                files = GetFilesNotExcluded();
        }

        private void LogFiles(IEnumerable<string> files)
        {
            _activityProxy.LogBuildMessage("Files to process for CodeMetrics activity are those:");
            foreach (var file in files)
                _activityProxy.LogBuildMessage(file);
        }

        private static bool IsNotEmpty(IEnumerable<string> files)
        {
            return (files != null && files.Any());
        }

        private IEnumerable<string> GetFilesNotExcluded()
        {
            var filesToProcess = GetFilesToProcess();
            var filesToIgnore = GetFilesToIgnore();

            _activityProxy.LogBuildMessage("CodeMetrics / Removing files to ignore from those to process");
            return filesToProcess.Where(x => !filesToIgnore.Exists(y => y == x));
        }

        private List<string> GetFilesToIgnore()
        {
            return GetFiles(_activityProxy.FilesToIgnore);
        }

        private List<string> GetFilesToProcess()
        {
            return GetFiles(_activityProxy.FilesToProcess);
        }

        private List<string> GetFiles(IEnumerable<string> filenames)
        {
            var completeFileNames = new List<string>();

            foreach (var filename in filenames)
            {
                _activityProxy.LogBuildMessage(string.Format("Get ready to enumerate files from {0}", filename));
                var path = Path.Combine(_activityProxy.BinariesDirectory, Path.GetDirectoryName(filename));

                _activityProxy.LogBuildMessage(string.Format("Enumerates files from {0}", Path.Combine(path, Path.GetFileName(filename))));
                var files = EnumerateFiles(filename, path);

                completeFileNames.AddRange(files);
            }
            return completeFileNames;
        }

        private IEnumerable<string> EnumerateFiles(string filename, string path)
        {
            return _fileSystemProxy.EnumerateFiles(path, Path.GetFileName(filename));
        }
    }
}
