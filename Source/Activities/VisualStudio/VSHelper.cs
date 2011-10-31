//-----------------------------------------------------------------------
// <copyright file="VSHelper.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    using System;
    using System.Globalization;
    using Microsoft.Win32;

    /// <summary>
    /// Gets the installation path of a given VS version.
    /// <para></para>
    /// Supported versions 2005, 2008 and 2010, .Net 2003 and .Net 2002
    /// </summary>
    internal static class VSHelper
    {
        /// <summary>
        /// Gets the installation directory of a given Visual Studio Version
        /// </summary>
        /// <param name="version">Visual Studio Version</param>
        /// <returns>Null if not installed the installation directory otherwise</returns>
        internal static string GetVisualStudioInstallationDir(VSVersionInternal version)
        {
            string registryKeyString = string.Format(
                @"SOFTWARE{0}Microsoft\VisualStudio\{1}", 
                Environment.Is64BitProcess ? @"\Wow6432Node\" : "\\",
                GetVersionNumber(version));

            using (var localMachineKey = Registry.LocalMachine.OpenSubKey(registryKeyString))
            {
                if (localMachineKey == null)
                {
                    return null;
                }

                return localMachineKey.GetValue("InstallDir") as string;
            }
        }

        /// <summary>
        /// Predicate that indicates if we support getting the installation folder
        /// for it.
        /// </summary>
        /// <param name="version">Visual Studio Version</param>
        /// <returns>true if supported false otherwise</returns>
        internal static bool IsSupportedVersion(VSVersionInternal version)
        {
            if (version == VSVersionInternal.VSNet2002 ||
                version == VSVersionInternal.VSNet2003 ||
                version == VSVersionInternal.VS2005 ||
                version == VSVersionInternal.VS2008 ||
                version == VSVersionInternal.VS2010)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the version number as used by Visual Studio to identify it's version internally.
        /// <para></para>
        /// eg: Visual Studio 2010 is 10.0 and Visual Studio 2008 is 9.0
        /// </summary>
        /// <param name="version">Visual Studio Version</param>
        /// <returns>A string with the VS internal number version</returns>
        private static string GetVersionNumber(VSVersionInternal version)
        {
            if (IsSupportedVersion(version) == false)
            {
                throw new VSVersionNotSupportedException("Not a supported version of Visual Studio");
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:0.0}", (float)version / 10);
        }
    }
}
