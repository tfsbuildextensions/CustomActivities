//-----------------------------------------------------------------------
// <copyright file="CodeMetricsFilesToProcess.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TfsBuildExtensions.Activities.CodeQuality.Proxy;

    /// <summary>
    /// Class to manage the files the CodeMetrics activity should process
    /// </summary>
    public class CodeMetricsFilesToProcess
    {
        private readonly IActivityContextProxy activityProxy;
        private readonly IFileSystemProxy fileSystemProxy;

        internal CodeMetricsFilesToProcess(CodeMetrics activity, CodeActivityContext context) : this(new ActivityContextProxy(activity, context), new FileSystemProxy())
        {
        }

        internal CodeMetricsFilesToProcess(IActivityContextProxy activityProxy, IFileSystemProxy fileSystemProxy)
        {
            this.activityProxy = activityProxy;
            this.fileSystemProxy = fileSystemProxy;
        }

        internal IEnumerable<string> Get()
        {
            IEnumerable<string> files = this.activityProxy.FilesToProcess;

            if (IsNotEmpty(files))
            {
                this.RemovesFilesToIgnore(ref files);
            }
            else
            {
                files = new List<string> { "*.dll", "*.exe" };
            }

            this.LogFiles(files);
            return files;
        }
        
        private static bool IsNotEmpty(IEnumerable<string> files)
        {
            return files != null && files.Any();
        }

        private void RemovesFilesToIgnore(ref IEnumerable<string> files)
        {
            files = this.GetFilesNotExcluded();
        }

        private void LogFiles(IEnumerable<string> files)
        {
            this.activityProxy.LogBuildMessage("Files to process for CodeMetrics activity are those:");
            foreach (var file in files)
            {
                this.activityProxy.LogBuildMessage(file);
            }
        }

        private IEnumerable<string> GetFilesNotExcluded()
        {
            var filesToProcess = this.GetFilesToProcess();
            var filesToIgnore = this.GetFilesToIgnore();

            this.activityProxy.LogBuildMessage("CodeMetrics / Removing files to ignore from those to process");
            return filesToProcess.Where(x => !filesToIgnore.Exists(y => y == x));
        }

        private List<string> GetFilesToIgnore()
        {
            return this.GetFiles(this.activityProxy.FilesToIgnore);
        }

        private List<string> GetFilesToProcess()
        {
            return this.GetFiles(this.activityProxy.FilesToProcess);
        }

        private List<string> GetFiles(IEnumerable<string> filenames)
        {
            var completeFileNames = new List<string>();

            if (filenames == null)
            {
                return completeFileNames;
            }

            foreach (var filename in filenames)
            {
                this.activityProxy.LogBuildMessage(string.Format("Get ready to enumerate files from {0}", filename));
                var path = Path.Combine(this.activityProxy.BinariesDirectory, Path.GetDirectoryName(filename));

                this.activityProxy.LogBuildMessage(string.Format("Enumerates files from {0}", Path.Combine(path, Path.GetFileName(filename))));
                var files = this.EnumerateFiles(filename, path);

                completeFileNames.AddRange(files);
            }

            return completeFileNames;
        }

        private IEnumerable<string> EnumerateFiles(string filename, string path)
        {
            return this.fileSystemProxy.EnumerateFiles(path, Path.GetFileName(filename));
        }
    }
}
