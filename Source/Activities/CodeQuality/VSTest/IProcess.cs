//-----------------------------------------------------------------------
// <copyright file="IProcess.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    /// <summary>
    /// The IProcess interface encapsulates a Process.
    /// </summary>
    internal interface IProcess
    {
        bool Execute(string executablePath, string commandLineArguments, string workingDirectory);
    }
}
