//-----------------------------------------------------------------------
// <copyright file="WebAccess.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.TfsUtilities
{
    using System;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Clase that provides utility methods for working with Team Foundation Web Access.
    /// </summary>
    public static class WebAccess
    {
        /// <summary>
        /// Gets the TSWA Linking Service for a project collection
        /// </summary>
        /// <param name="projectCollection">TFS Project Collection</param>
        /// <returns>TswaClientHyperlinkService</returns>
        public static TswaClientHyperlinkService GetHyperlinkService(TfsTeamProjectCollection projectCollection)
        {
            ArgumentValidation.ValidateObjectIsNotNull(projectCollection, "projectCollection");
            return projectCollection.GetService<TswaClientHyperlinkService>();
        }

        /// <summary>
        /// Gets the TSWA build details uri
        /// </summary>
        /// <param name="projectCollection">Team Project collection</param>
        /// <param name="buildUri">Build Uri</param>
        /// <returns>Url to view build details in TSWA</returns>
        public static Uri GetBuildDetailsUri(TfsTeamProjectCollection projectCollection, Uri buildUri)
        {
            ArgumentValidation.ValidateObjectIsNotNull(projectCollection, "projectCollection");
            ArgumentValidation.ValidateTfsUri(buildUri, "buildUri", TfsUriType.Build);

            var linkingService = GetHyperlinkService(projectCollection);
            return linkingService.GetViewBuildDetailsUrl(buildUri);
        }

        /// <summary>
        /// Gets the TSWA work item editor
        /// </summary>
        /// <param name="projectCollection">Team Project collection</param>
        /// <param name="workItemId">Work Item Id</param>
        /// <returns>Url to view the work item editor in TSWA</returns>
        public static Uri GetWorkItemEditorUri(TfsTeamProjectCollection projectCollection, int workItemId)
        {
            ArgumentValidation.ValidateObjectIsNotNull(projectCollection, "projectCollection");
            var linkingService = GetHyperlinkService(projectCollection);
            return linkingService.GetWorkItemEditorUrl(workItemId);
        }
    }
}
