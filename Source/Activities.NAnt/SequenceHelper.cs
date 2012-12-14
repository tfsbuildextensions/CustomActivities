//-----------------------------------------------------------------------
// <copyright file="SequenceHelper.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//---------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.NAnt
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;

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
