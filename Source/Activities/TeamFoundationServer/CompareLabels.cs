//-----------------------------------------------------------------------
// <copyright file="CompareLabels.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;

    /// <summary>
    /// Get a changeset for the changes between two labels.
    /// </summary>
    [System.ComponentModel.Description("Activity to calculate a changeset between two labels.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class CompareLabels : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the first version control label.
        /// </summary>
        public InArgument<VersionControlLabel> Label1 { get; set; }

        /// <summary>
        /// Gets or sets the second version control label.
        /// </summary>
        public InArgument<VersionControlLabel> Label2 { get; set; }

        /// <summary>
        /// Gets or sets the version control server to use.
        /// </summary>
        public InArgument<VersionControlServer> VersionControlServer { get; set; }

        /// <summary>
        /// Gets or sets the version control server to use.
        /// </summary>
        public OutArgument<IEnumerable<Changeset>> Changeset { get; set; }

        /// <summary>
        /// Get the label details.
        /// </summary>
        protected override void InternalExecute()
        {
            var label1 = this.Label1.Get(this.ActivityContext);
            var label2 = this.Label2.Get(this.ActivityContext);

            var label1Spec = new LabelVersionSpec(label1.Name, label1.Scope);
            var label2Spec = new LabelVersionSpec(label2.Name, label2.Scope);
            var vcs = this.VersionControlServer.Get(this.ActivityContext);

            var queryHistoryResult = vcs.QueryHistory("$/", VersionSpec.Latest, 0, RecursionType.Full, null, label1Spec, label2Spec, int.MaxValue, false, false).Cast<Changeset>();

            this.Changeset.Set(this.ActivityContext, queryHistoryResult);
        }
    }
}
