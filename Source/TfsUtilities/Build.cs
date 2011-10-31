//-----------------------------------------------------------------------
// <copyright file="Build.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.TfsUtilities
{
    using System;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Performs tasks related to TF Build.
    /// </summary>
    public static class Build
    {
        /// <summary>
        /// Adds a new workspace mapping to an existing build definition
        /// </summary>
        /// <param name="buildDefinition">Build definition for the new mapping</param>
        /// <param name="localItem">Local path for the mapping</param>
        /// <param name="serverItem">Server path for the mapping</param>
        /// <returns>The created workspace mapping</returns>
        /// <remarks>The build definition is not saved after the mapping is added.</remarks>
        public static IWorkspaceMapping AddWorkspaceMapping(IBuildDefinition buildDefinition,  string localItem, string serverItem)
        {
            return AddWorkspaceMapping(buildDefinition, localItem, serverItem, WorkspaceMappingType.Map, WorkspaceMappingDepth.Full); 
        }

        /// <summary>
        /// Adds a new workspace mapping to an existing build definition
        /// </summary>
        /// <param name="buildDefinition">Build definition for the new mapping</param>
        /// <param name="localItem">Local path for the mapping</param>
        /// <param name="serverItem">Server path for the mapping</param>
        /// <param name="mappingType">Type of mapping (map or cloak)</param>
        /// <param name="depth">The Mapping Depth</param>
        /// <returns>The created workspace mapping</returns>
        /// <remarks>The build definition is not saved after the mapping is added.</remarks>
        public static IWorkspaceMapping AddWorkspaceMapping(IBuildDefinition buildDefinition, string localItem, string serverItem, WorkspaceMappingType mappingType, WorkspaceMappingDepth depth)
        {
            // Validation
            ArgumentValidation.ValidateObjectIsNotNull(buildDefinition, "buildDefinition");
            ArgumentValidation.ValidateStringIsNotEmpty(localItem, "localPath");
            ArgumentValidation.ValidateStringIsNotEmpty(serverItem, "serverPath");

            var mapping = buildDefinition.Workspace.AddMapping(serverItem, localItem, mappingType, depth);
            return mapping; 
        }

        /// <summary>
        /// Clears all workspace mappings from a build definition
        /// </summary>
        /// <param name="buildDefinition">Build definition to clear.</param>
        /// <remarks>The build definition is not saved after the mappings are cleared.</remarks>
        public static void ClearWorkspaceMappings(IBuildDefinition buildDefinition)
        {
            // Validation
            ArgumentValidation.ValidateObjectIsNotNull(buildDefinition, "buildDefinition");
            var buildWorkspaceMappings = buildDefinition.Workspace.Mappings; 
            while (buildWorkspaceMappings.Count > 0)
            {
                buildWorkspaceMappings.Remove(buildWorkspaceMappings[0]); 
            }
        }

        /// <summary>
        /// Removes a specified server mapping from a build definition
        /// </summary>
        /// <param name="buildDefinition">Build definition to remove the mapping from.</param>
        /// <param name="serverItem">Server mapping to remove.</param>
        /// <remarks>The build definition is not saved after the mappings are cleared.</remarks>
        public static void RemoveServerMapping(IBuildDefinition buildDefinition, string serverItem)
        {
            // Validation
            ArgumentValidation.ValidateObjectIsNotNull(buildDefinition, "buildDefinition");
            ArgumentValidation.ValidateStringIsNotEmpty(serverItem, "serverMapping"); 

            var buildWorkspaceMappings = buildDefinition.Workspace.Mappings;

            foreach (var mapping in buildWorkspaceMappings)
            {
                if (mapping.ServerItem.Equals(serverItem, StringComparison.OrdinalIgnoreCase))
                {
                    buildWorkspaceMappings.Remove(mapping);
                    break; 
                }
            }
        }

        /// <summary>
        /// Removes a specified local mapping from a build definition
        /// </summary>
        /// <param name="buildDefinition">Build definition to remove the mapping from.</param>
        /// <param name="localItem">Local mapping to remove.</param>
        /// <remarks>The build definition is not saved after the mappings are cleared.</remarks>
        public static void RemoveLocalMapping(IBuildDefinition buildDefinition, string localItem)
        {
            // Validation
            ArgumentValidation.ValidateObjectIsNotNull(buildDefinition, "buildDefinition");
            ArgumentValidation.ValidateStringIsNotEmpty(localItem, "localItem");

            var buildWorkspaceMappings = buildDefinition.Workspace.Mappings;

            foreach (var mapping in buildWorkspaceMappings)
            {
                if (mapping.LocalItem.Equals(localItem, StringComparison.OrdinalIgnoreCase))
                {
                    buildWorkspaceMappings.Remove(mapping);
                    break;
                }
            }
        }
    }
}
