//-----------------------------------------------------------------------
// <copyright file="NAntException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//---------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System;
    using TfsBuildExtensions.Activities;

    [Serializable]
    public sealed class NAntException : FailingBuildException
    {
        /// <summary>
        /// Initializes a new instance of the NAntException class.
        /// </summary>
        public NAntException()
        {
        }

        public NAntException(string message)
            : base(message)
        {
        }

        public NAntException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private NAntException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
