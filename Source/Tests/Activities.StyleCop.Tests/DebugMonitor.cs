//-----------------------------------------------------------------------
// <copyright file="DebugMonitor.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Tests
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A tracelisener that is used to count how many time a call has been made to the loging system 
    /// </summary>
    public class DebugMonitor : TraceListener
    {
        /// <summary>
        /// The string to match the messages against
        /// </summary>
        private string filter;

        /// <summary>
        /// Initializes a new instance of the DebugMonitor class.
        /// </summary>
        /// <param name="filter">The filter to match</param>
        public DebugMonitor(string filter)
            : base()
        {
            this.filter = filter;
        }

        /// <summary>
        /// The number of times a message is seen with the correct text
        /// </summary>
        public int Writes { get; private set; }

        /// <summary>
        /// Intercepts a write message
        /// </summary>
        /// <param name="message">The sent message</param>
        public override void Write(string message)
        {
            this.CheckIfMessageMatches(message);
        }

        /// <summary>
        /// Intercepts a writeline message
        /// </summary>
        /// <param name="message">The sent message</param>
        public override void WriteLine(string message)
        {
            this.CheckIfMessageMatches(message);
        }

        /// <summary>
        /// Counts the number of lines that contain a given string
        /// </summary>
        /// <param name="message">The input message</param>
        private void CheckIfMessageMatches(string message)
        {
            if (message.Contains(this.filter))
            {
                this.Writes++;
            }
        }
    }
}
