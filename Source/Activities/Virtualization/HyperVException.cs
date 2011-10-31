//-----------------------------------------------------------------------
// <copyright file="HyperVException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization.Extended
{
    using System;

    /// <summary>
    /// HyperV Activity exception handler
    /// </summary>
    [Serializable]
    public class HyperVException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the HyperVException class
        /// </summary>
        public HyperVException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the HyperVException class
        /// </summary>
        /// <param name="message">Message to send</param>
        public HyperVException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HyperVException class
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="innerException">Inner exception details</param>
        public HyperVException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HyperVException class
        /// </summary>
        /// <param name="info">Serialization information for the exception</param>
        /// <param name="context">The streaming context for the exception</param>
        protected HyperVException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
