//-----------------------------------------------------------------------
// <copyright file="ScriptItem.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Virtualization.Extended
{
    using System;

    /// <summary>
    /// Possible actions that can be included in a script 
    /// </summary>
    public enum ScriptAction
    {
        /// <summary>
        /// Send a left mouse click
        /// </summary>
        ClickLeft,

        /// <summary>
        /// Send a right mouse click
        /// </summary>
        ClickRight,

        /// <summary>
        /// Send a center mouse click
        /// </summary>
        ClickCenter,

        /// <summary>
        /// Test to type
        /// </summary>
        TypeAsciiText,

        /// <summary>
        /// Windows automation key sequences
        /// </summary>
        TypeKeySequence
    }

    /// <summary>
    /// A step in a VirtualPC scriptItem
    /// </summary>
    public class ScriptItem
    {
        /// <summary>
        /// Initializes a new instance of the ScriptItem class
        /// </summary>
        /// <param name="action">The type of action</param>
        /// <param name="text">The parameters for the action, null is a mouse click </param>
        public ScriptItem(ScriptAction action, string text)
        {
            this.Action = action;
            this.Text = text;
        }

        /// <summary>
        /// The action type to perform
        /// </summary>
        public ScriptAction Action { get; set; }

        /// <summary>
        /// The parameters for the action
        /// </summary>
        public string Text { get; set; }
    }
}
