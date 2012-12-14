//-----------------------------------------------------------------------
// <copyright file="NAntException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//---------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
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
