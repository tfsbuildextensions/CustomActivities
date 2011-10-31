//-----------------------------------------------------------------------
// <copyright file="WorkItem.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.TfsUtilities;
    using WITClient = Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// WorkItemTrackingAction
    /// </summary>
    public enum WorkItemTrackingAction
    {
        /// <summary>
        /// GetById
        /// </summary>
        GetById,

        /// <summary>
        /// Save
        /// </summary>
        Save,

        /// <summary>
        /// Reset
        /// </summary>
        Reset,

        /// <summary>
        /// GetFieldValue
        /// </summary>
        GetFieldValue,

        /// <summary>
        /// SetFieldValue
        /// </summary>
        SetFieldValue,
    }

    /// <summary>
    /// Performs operations on Team Foundation Work Items
    /// <b>Valid Action values are:</b>
    /// <para><i>GetById</i> - Returns a work item by id. <b>Required: </b>TeamProjectCollection, WorkItemId <b>Output: </b>WorkItem</para>
    /// <para><i>Save</i> - Saves a work item to the work item store. <b>Required: </b>TeamProjectCollection, WorkItem </para>
    /// <para><i>Resets</i> - Discards all changes and resets to current version. <b>Required: </b>TeamProjectCollection, WorkItem</para>
    /// <para><i>GetFieldValue</i> - Gets the value of a work item field. <b>Required: </b>TeamProjectCollection, WorkItem, FieldName <b>Output: </b> FieldValue</para>
    /// <para><i>SetFieldValue</i> - Sets the value of a work item field. <b>Required: </b>TeamProjectCollection, WorkItem, FieldName, FieldValue</para>
    /// </summary>
    [System.ComponentModel.Description("Activity to perform operations on a Team Foundation Work Item")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class WorkItemTracking : TFBaseCodeActivity
    {
        private WorkItemTrackingAction action = WorkItemTrackingAction.GetById;

        /// <summary>
        /// Specifies the action to perform. Default is Create.
        /// </summary>
        public WorkItemTrackingAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Target work item or returned work item.
        /// </summary>
        [System.ComponentModel.Description("Target work item or returned work item.")]
        public InOutArgument<WITClient.WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Target work item id or the returned work item id.
        /// </summary>
        [System.ComponentModel.Description("Target work item id or returned work item id.")]
        public InOutArgument<int> WorkItemId { get; set; }

        /// <summary>
        /// The value to set for a field/link or the returned field value.
        /// </summary>
        [System.ComponentModel.Description("Value to set for a field/link or returned field value.")]
        public InOutArgument<object> ItemValue { get; set; }

        /// <summary>
        /// The name of the field to set.
        /// </summary>
        [System.ComponentModel.Description("Name of the field to set.")]
        public InArgument<string> FieldName { get; set; }

        /// <summary>
        /// InternalExecute
        /// </summary>
        protected override void InternalExecute()
        {
            var workItem = this.WorkItem.Get(ActivityContext);
            var id = this.WorkItemId.Get(ActivityContext);
            var fieldValue = this.ItemValue.Get(ActivityContext);
            var fieldName = this.FieldName.Get(ActivityContext); 

            switch (this.Action)
            {
                case WorkItemTrackingAction.GetById:
                    var returnedWorkItem = WorkItems.GetWorkItemById(ProjectCollection, id);
                    returnedWorkItem.Reset();
                    returnedWorkItem.Open();
                    this.WorkItem.Set(ActivityContext, returnedWorkItem);
                    break;

                case WorkItemTrackingAction.Save:
                    ArgumentValidation.ValidateObjectIsNotNull(workItem, "WorkItem");

                    if (workItem.IsDirty)
                    {
                        try
                        {
                            workItem.Save();
                        }
                        catch (WITClient.ValidationException e)
                        {
                            throw new InvalidOperationException("Work item is not valid", e); 
                        }
                    }

                    break;

                case WorkItemTrackingAction.Reset:
                    ArgumentValidation.ValidateObjectIsNotNull(workItem, "WorkItem");
                    workItem.Reset(); 
                    
                    break;

                case WorkItemTrackingAction.GetFieldValue:
                    ArgumentValidation.ValidateObjectIsNotNull(workItem, "WorkItem");
                    ArgumentValidation.ValidateStringIsNotEmpty(fieldName, "FieldName");
                    if (workItem.Fields.Contains(fieldName))
                    {
                        fieldValue = workItem.Fields[fieldName].Value;
                        this.ItemValue.Set(ActivityContext, fieldValue);
                    }
                    else
                    {
                        throw new ArgumentException("Field name is not valid for this work item", fieldName); 
                    }

                    break; 

                case WorkItemTrackingAction.SetFieldValue:
                    ArgumentValidation.ValidateObjectIsNotNull(workItem, "WorkItem");
                    ArgumentValidation.ValidateStringIsNotEmpty(fieldName, "FieldName");
                    ArgumentValidation.ValidateObjectIsNotNull(fieldValue, "FieldValue");
                    if (workItem.Fields.Contains(fieldName))
                    {
                        workItem.Fields[fieldName].Value = fieldValue;
                    }
                    else
                    {
                        throw new ArgumentException("Field name is not valid for this work item", fieldName);
                    }

                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }
    }
}
