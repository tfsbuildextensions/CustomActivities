// -----------------------------------------------------------------------
// <copyright file="FileSystemProxy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace TfsBuildExtensions.Activities.CodeQuality.Proxy
{
    using System;
    using System.Collections.Generic;

    public interface IFileSystemProxy
    {
        /// <summary>
        /// Redirect to <see cref="Directory.EnumerateFiles(string, string)"/>
        /// </summary>
        IEnumerable<string> EnumerateFiles(String path, String searchPattern);

        /// <summary>
        /// Redirect to <see cref="File.Exists"/>
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Redirect to <see cref="Directory.Exists"/>
        /// </summary>
        bool DirectoryExists(string path);

        /// <summary>
        /// Redirect to <see cref="Directory.EnumerateFiles(string)"/>
        /// </summary>
        IEnumerable<string> EnumerateFiles(String path);

        /// <summary>
        /// Redirect to <see cref="Directory.GetLastWriteTime(string)"/>
        /// </summary>
        DateTime GetLastWriteTime(String path);

        /// <summary>
        /// Redirect to <see cref="File.Delete"/>
        /// </summary>
        void DeleteFile(string path);

        /// <summary>
        /// Redirect to <see cref="File.Copy(string, string)"/>
        /// </summary>
        void CopyFile(string sourceFileName, string destFileName);
    }

    /// <summary>
    /// Provides access to the file system (for decoupling concerns)
    /// </summary>
    public class FileSystemProxy : IFileSystemProxy
    {
        /// <summary>
        /// Redirect to <see cref="Directory.EnumerateFiles(string, string)"/>
        /// </summary>
        public IEnumerable<string> EnumerateFiles(String path, String searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern);
        }

        /// <summary>
        /// Redirect to <see cref="Directory.EnumerateFiles(string)"/>
        /// </summary>
        public IEnumerable<string> EnumerateFiles(String path)
        {
            return Directory.EnumerateFiles(path);
        }

        /// <summary>
        /// Redirect to <see cref="Directory.GetLastWriteTime(string)"/>
        /// </summary>
        public DateTime GetLastWriteTime(String path)
        {
            return Directory.GetLastWriteTime(path);
        }

        /// <summary>
        /// Redirect to <see cref="File.Delete"/>
        /// </summary>
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Redirect to <see cref="File.Copy(string, string)"/>
        /// </summary>
        public void CopyFile(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        /// <summary>
        /// Redirect to <see cref="File.Exists"/>
        /// </summary>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Redirect to <see cref="Directory.Exists"/>
        /// </summary>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
