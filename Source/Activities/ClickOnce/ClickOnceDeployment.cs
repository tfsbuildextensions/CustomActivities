﻿//-----------------------------------------------------------------------
// <copyright file="ClickOnceDeployment.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Microsoft.TeamFoundation.Build.Client;

namespace TfsBuildExtensions.Activities.ClickOnce
{
    /// <summary>
    /// ClickOnceDeployment
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class ClickOnceDeployment : BaseCodeActivity
    {
        private InArgument<string> processor = "x86";

        /// <summary>
        /// Publisher
        /// </summary>
        public InArgument<string> Publisher { get; set; }

        /// <summary>
        /// Create a desktop shortcut for the Application
        /// </summary>
        public InArgument<bool> CreateDesktopShortcut { get; set; }

        /// <summary>
        /// Name of Icon File
        /// </summary>
        public InArgument<string> IconFile { get; set; }

        /// <summary>
        /// Timestamp Server Location
        /// </summary>
        public InArgument<string> TimestampUri { get; set; }

        /// <summary>
        /// Sets the Processor to use. Default is x86.
        /// </summary>
        public InArgument<string> Processor
        {
            get { return this.processor; }
            set { this.processor = value; }
        }

        /// <summary>
        /// Full Path and Filename to Mage.exe (Located in .NET SDK)
        /// </summary>
        [RequiredArgument]
        public InArgument<string> MageFilePath { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Version { get; set; }

        /// <summary>
        /// BinLocation
        /// </summary>
        [RequiredArgument]
        public InArgument<string> BinLocation { get; set; }

        /// <summary>
        /// Application Name
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ApplicationName { get; set; }

        /// <summary>
        /// Get File Path
        /// </summary>
        public InArgument<string> CertFilePath { get; set; }

        /// <summary>
        /// Certificate Password
        /// </summary>
        public InArgument<string> CertPassword { get; set; }

        /// <summary>
        /// Manifest Certificate Thumbprint / Hash
        /// </summary>
        public InArgument<string> ManifestCertificateThumbprint { get; set; }

        /// <summary>
        /// Publish Location
        /// </summary>
        [RequiredArgument]
        public InArgument<string> PublishLocation { get; set; }

        /// <summary>
        /// Install location
        /// </summary>
        [RequiredArgument]
        public InArgument<string> InstallLocation { get; set; }

        /// <summary>
        /// TargetFrameworkVersion
        /// </summary>
        [RequiredArgument]
        public InArgument<string> TargetFrameworkVersion { get; set; }

        /// <summary>
        /// Internal Execute method to excute the process within the Activity
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        protected override void InternalExecute()
        {
            string mageFilePath = this.MageFilePath.Get(this.ActivityContext);
            string version = this.Version.Get(this.ActivityContext);
            string binLocation = this.BinLocation.Get(this.ActivityContext);
            string applicationName = this.ApplicationName.Get(this.ActivityContext);
            string certFilePath = this.CertFilePath.Get(this.ActivityContext);
            string certPassword = this.CertPassword.Get(this.ActivityContext);
            string publishLocation = this.PublishLocation.Get(this.ActivityContext);
            string installLocation = this.InstallLocation.Get(this.ActivityContext);
            string targetFrameworkVersion = this.TargetFrameworkVersion.Get(this.ActivityContext);
            string manifestCertificateThumbprint = this.ManifestCertificateThumbprint.Get(this.ActivityContext);
            string timestampUri = this.TimestampUri.Get(this.ActivityContext);
            bool createDesktopShortcut = this.CreateDesktopShortcut.Get(this.ActivityContext);
            string publisher = this.Publisher.Get(this.ActivityContext);
            string iconFile = this.IconFile.Get(this.ActivityContext);

            try
            {
                // Set ToFile location
                string toFile = publishLocation + "\\Application Files\\" + applicationName + "_" + version;

                if (Directory.Exists(toFile))
                {
                    Directory.Delete(toFile, true);
                }

                Directory.CreateDirectory(toFile);

                // Copy Files from bin folder to publish location
                CopyDirectory(binLocation, toFile, true);

                string manifestCertificateThumbprintArg = !string.IsNullOrEmpty(manifestCertificateThumbprint) ? "-CertHash " + manifestCertificateThumbprint : string.Empty;
                string certFilePathArg = !string.IsNullOrEmpty(certFilePath) ? "-CertFile " + certFilePath : string.Empty;
                string certPasswordArg = !string.IsNullOrEmpty(certPassword) ? "-Password " + certPassword : string.Empty;
                string timestampUriArg = !string.IsNullOrEmpty(timestampUri) ? "-TimestampUri " + timestampUri : string.Empty;
                string iconFileArg = !string.IsNullOrEmpty(iconFile) ? " -IconFile " + iconFile : string.Empty;

                // Create Application Manifest
                string args = "-New Application -Processor " + this.Processor.Get(this.ActivityContext) + " -ToFile \"" + toFile + "\\" + applicationName + ".exe.manifest\" -name " + applicationName + " -Version " + version + " -FromDirectory \"" + toFile + "\"" + iconFileArg;
                RunMage(mageFilePath, args);

                // Sign Application Manifest
                //args = "-Sign \"" + toFile + "\\" + applicationName + ".exe.manifest\" " + manifestCertificateThumbprintArg + " " + certFilePathArg + " " + certPasswordArg;
                //RunMage(mageFilePath, args);

                // rename all files to have a .deploy
                RenameFiles(toFile);

                // Sign Application Manifest
                args = "-Sign \"" + toFile + "\\" + applicationName + ".exe.manifest\" " + manifestCertificateThumbprintArg + " " + certFilePathArg + " " + certPasswordArg + " " + timestampUriArg;
                
                RunMage(mageFilePath, args);

                CreateDeploymentManifest(version, applicationName, publishLocation, installLocation, targetFrameworkVersion, createDesktopShortcut, publisher);

                // Sign Deployment Manifest
                args = "-Sign \"" + publishLocation + "\\" + applicationName + ".application\" " + manifestCertificateThumbprintArg + " " + certFilePathArg + " " + certPasswordArg + " " + timestampUriArg;
                RunMage(mageFilePath, args);

                // Copy Deploy Manifest to parent folder
                File.Copy(publishLocation + "\\" + applicationName + ".application", toFile + "\\" + applicationName + ".application", true);

                // Create Bootstrapper
                CreateBootstrapper(applicationName + ".application", applicationName, installLocation, publishLocation);
            }
            catch (Exception ex)
            {
                if (this.FailBuildOnError.Get(this.ActivityContext))
                {
                    this.LogBuildError(string.Format(CultureInfo.InvariantCulture, ex.ToString()));
                }
                else
                {
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, ex.ToString()), BuildMessageImportance.High);
                }
            }
        }

        private static void CopyDirectory(string sourcePath, string destPath, bool overwrite)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                if (file.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".application", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, overwrite);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest, overwrite);
            }
        }

        private static void RunMage(string mageFilePath, string args)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = mageFilePath;

                // process.StartInfo.WorkingDirectory = SearchPathRoot.Get(context);                    
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.Arguments = args;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(output);
                }
            }
        }

        private static void CreateDeploymentManifest(string version, string applicationName, string publishLocation, string installLocation, string targetFrameworkVersion, bool createDesktopShortcut, string publisher)
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("TargetPath", "Application Files\\" + applicationName + "_" + version + "\\" + applicationName + ".exe.manifest");

            GenerateDeploymentManifest generateDeploymentManifest = new GenerateDeploymentManifest
            {
                AssemblyName = applicationName + ".application",
                AssemblyVersion = version,
                Product = applicationName,
                Publisher = publisher,

                DeploymentUrl = installLocation + applicationName + ".application",
                Install = true,
                UpdateEnabled = true,
                UpdateMode = "Foreground",
                OutputManifest = new TaskItem(publishLocation + "\\" + applicationName + ".application"),
                MapFileExtensions = true,
                EntryPoint = new TaskItem(publishLocation + @"\Application Files\" + applicationName + "_" + version + "\\" + applicationName + ".exe.manifest", metadata),
                CreateDesktopShortcut = createDesktopShortcut,
                TargetFrameworkVersion = targetFrameworkVersion,
                TargetFrameworkMoniker = ".NETFramework,Version=v" + targetFrameworkVersion,
                MinimumRequiredVersion = version
            };

            generateDeploymentManifest.Execute();
        }

        private static void CreateBootstrapper(string applicationFile, string applicationName, string installLocation, string publishDir)
        {
            GenerateBootstrapper bootStrapper = new GenerateBootstrapper();
            bootStrapper.ApplicationFile = applicationFile;
            bootStrapper.ApplicationName = applicationName;
            bootStrapper.ApplicationUrl = installLocation;

            // bootStrapper.BootstrapperItems="@(BootstrapperFile)";
            bootStrapper.OutputPath = publishDir;
            bootStrapper.Execute();
        }

        private static void RenameFiles(string sourcePath)
        {
            foreach (string file in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".application", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                File.Copy(file, file + ".deploy");
                File.Delete(file);
            }
        }
    }
}
