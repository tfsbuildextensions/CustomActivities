//-----------------------------------------------------------------------
// <copyright file="FailingBuildException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities
{
    using System;

    /// <summary>
    /// FailingBuild Exception
    /// </summary>
    [Serializable]
    public class FailingBuildException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the FailingBuildException class
        /// </summary>
        public FailingBuildException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the FailingBuildException class
        /// </summary>
        /// <param name="message">Message to send</param>
        public FailingBuildException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FailingBuildException class
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="innerException">Inner exception details</param>
        public FailingBuildException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FailingBuildException class
        /// </summary>
        /// <param name="info">Serialization information for the exception</param>
        /// <param name="context">The streaming context for the exception</param>
        protected FailingBuildException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
