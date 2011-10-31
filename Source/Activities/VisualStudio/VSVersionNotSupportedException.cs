//-----------------------------------------------------------------------
// <copyright file="VSVersionNotSupportedException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    using System;

    /// <summary>
    /// Exception to be thrown if an action is asked to be performed with an unsupported VS
    /// version
    /// </summary>
    [Serializable]
    public class VSVersionNotSupportedException : Exception
    {
            /// <summary>
        /// Initializes a new instance of the VSVersionNotSupportedException class
        /// </summary>
        public VSVersionNotSupportedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the VSVersionNotSupportedException class
        /// </summary>
        /// <param name="message">Message to send</param>
        public VSVersionNotSupportedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the VSVersionNotSupportedException class
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="innerException">Inner exception details</param>
        public VSVersionNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the VSVersionNotSupportedException class
        /// </summary>
        /// <param name="info">Serialization information for the exception</param>
        /// <param name="context">The streaming context for the exception</param>
        protected VSVersionNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
