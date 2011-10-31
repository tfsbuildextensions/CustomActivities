////////-----------------------------------------------------------------------
//////// <copyright file="TemplateNewActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
////////-----------------------------------------------------------------------
//////namespace TfsBuildExtensions.Activities
//////{
//////    using System;
//////    using System.Activities;
//////    using System.Globalization;
//////    using Microsoft.TeamFoundation.Build.Client;
//////    using Microsoft.TeamFoundation.Build.Workflow.Activities;
//////    using TfsBuildExtensions.Activities;

//////    /// <summary>
//////    /// ActivityXXXAction
//////    /// </summary>
//////    public enum ActivityXXXAction
//////    {
//////        /// <summary>
//////        /// xxxAction
//////        /// </summary>
//////        xxxAction
//////    }

//////    /// <summary>
//////    /// <b>Valid Action values are:</b>
//////    /// </summary>
//////    [BuildActivity(HostEnvironmentOption.All)]
//////    public sealed class TemplateNewActivity : BaseCodeActivity
//////    {
//////        // Set a Default action
//////        private ActivityXXXAction action = ActivityXXXAction.xxxAction;

//////        /// <summary>
//////        /// Specifies the action to perform
//////        /// </summary>
//////        public ActivityXXXAction Action
//////        {
//////            get { return this.action; }
//////            set { this.action = value; }
//////        }

//////        /// <summary>
//////        /// Executes the logic for this workflow activity
//////        /// </summary>
//////        protected override void InternalExecute()
//////        {
//////            switch (this.Action)
//////            {
//////                case ActivityXXXAction.xxxAction:
//////                    // Your call here
//////                    break;
//////                default:
//////                    throw new ArgumentException("Action not supported");
//////            }
//////        }
//////    }
//////}