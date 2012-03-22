// -----------------------------------------------------------------------
// <copyright file="FileSystemProxy.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// -----------------------------------------------------------------------
#pragma warning disable 1591
namespace TfsBuildExtensions.Activities.CodeQuality.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public interface IFileSystemProxy
    {
        IEnumerable<string> EnumerateFiles(string path, string searchPattern);

        bool FileExists(string path);

        bool DirectoryExists(string path);

        IEnumerable<string> EnumerateFiles(string path);

        DateTime GetLastWriteTime(string path);

        void DeleteFile(string path);

        void CopyFile(string sourceFileName, string destinationFileName);
    }

    /// <summary>
    /// Provides access to the file system (for decoupling concerns)
    /// </summary>
    public class FileSystemProxy : IFileSystemProxy
    {
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public DateTime GetLastWriteTime(string path)
        {
            return Directory.GetLastWriteTime(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void CopyFile(string sourceFileName, string destinationFileName)
        {
            File.Copy(sourceFileName, destinationFileName);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
