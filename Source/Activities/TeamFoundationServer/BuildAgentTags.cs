//-----------------------------------------------------------------------
// <copyright file="BuildAgentTags.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// TagAction
    /// </summary>
    public enum TagAction
    {
        /// <summary>
        /// Add
        /// </summary>
        Add,
        
        /// <summary>
        /// Remove
        /// </summary>
        Remove
    }

    /// <summary>
    /// BuildAgent Tags
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class BuildAgentTags : BaseCodeActivity
    {
        private TagAction action = TagAction.Add;

        /// <summary>
        /// Action
        /// </summary>
        [RequiredArgument]
        public TagAction Action
        {
            get { return this.action; } 
            set { this.action = value; }
        }

        /// <summary>
        /// BuildAgent
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<IBuildAgent> BuildAgent { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<string> Tag { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>        
        protected override void InternalExecute()
        {
            IBuildAgent buildAgent = this.BuildAgent.Get(this.ActivityContext);
            string tag = this.Tag.Get(this.ActivityContext);

            switch (this.action)
            {
                case TagAction.Add:
                    buildAgent.Tags.Add(tag);
                    buildAgent.Save();
                    break;
                case TagAction.Remove:
                    buildAgent.Tags.Remove(tag);
                    buildAgent.Save();
                    break;
            }
        }
    }
}
