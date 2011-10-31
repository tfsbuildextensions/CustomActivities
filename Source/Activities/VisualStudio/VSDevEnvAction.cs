//-----------------------------------------------------------------------
// <copyright file="VSDevEnvAction.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{    
    /// <summary>
    /// The action to be performed while using devenv to build a solution/project
    /// </summary>
    public enum VSDevEnvAction
    {
        /// <summary>
        /// Build the solution/project
        /// </summary>
        Build,

        /// <summary>
        /// Clean and then build solution/project
        /// </summary>
        Rebuild,

        /// <summary>
        /// Clean the solution/project
        /// </summary>
        Clean,

        /// <summary>
        /// Build the solution/project and then deploy it.
        /// </summary>
        Deploy
    }
}
