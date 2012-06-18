//-----------------------------------------------------------------------
// <copyright file="ChangeConfigurationSettings.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Composite activity to change the configuration settings of a deployed service.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public partial class ChangeConfigurationSettings
    {
    }
}