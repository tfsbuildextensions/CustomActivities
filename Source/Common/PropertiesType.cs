//-----------------------------------------------------------------------
// <copyright file="PropertiesType.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities
{
    /// <summary>
    /// Type of conversion to use
    /// </summary>
    public enum PropertiesType
    {
        /// <summary>
        /// MSBuild
        /// </summary>
        MSBuild,
        
        /// <summary>
        /// NTShell
        /// </summary>
        NTShell,

        /// <summary>
        /// PowerShell
        /// </summary>
        PowerShell,
    }
}
