//-----------------------------------------------------------------------
// <copyright file="WorkItems.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.TfsUtilities
{
    using System;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Performs various actions with work items.
    /// </summary>
    public static class WorkItems
    {
        /// <summary>
        /// Returns a work item by ID
        /// </summary>
        /// <param name="projectCollection">Tfs Project Collection that contains the work item.</param>
        /// <param name="workItemId">Id for the work item</param>
        /// <returns>The found work item.</returns>
        /// <exception cref="ArgumentException">Occurs when the work item id is not valid.</exception>
        public static WorkItem GetWorkItemById(TfsTeamProjectCollection projectCollection, int workItemId)
        {
            ArgumentValidation.ValidateObjectIsNotNull(projectCollection, "projectCollection"); 
            if (workItemId <= 0)
            {
                // This is not a valid work item. 
                throw new ArgumentException("Work Item Id is not valid", "workItemId");
            }

            WorkItemStore store = projectCollection.GetService<WorkItemStore>();
            var workItem = store.GetWorkItem(workItemId);
            return workItem;
        }
    }
}
