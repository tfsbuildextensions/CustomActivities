//-----------------------------------------------------------------------
// <copyright file="SequenceHelper.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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

    internal static class SequenceHelper
    {
        public static Sequence Append(this Sequence sequence, IEnumerable<Activity> activities)
        {
            foreach (var activity in activities)
            {
                sequence.Activities.Add(activity);
            }

            return sequence;
        }
    }
}
