//-----------------------------------------------------------------------
// <copyright file="GetVersionControlLabel.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.VersionControl.Common;

    /// <summary>
    /// Get version control information about a build label.
    /// </summary>
    [System.ComponentModel.Description("Activity to get version control information about a build label.")]
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class GetVersionControlLabel : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the version control label to look for.
        /// </summary>
        public InArgument<string> Label { get; set; }

        /// <summary>
        /// Gets or sets the version control server to query.
        /// </summary>
        public InArgument<VersionControlServer> VersionControlServer { get; set; }

        /// <summary>
        /// Gets or sets the full version control label.
        /// </summary>
        public OutArgument<VersionControlLabel> VersionControlLabel { get; set; }

        /// <summary>
        /// Get the label details.
        /// </summary>
        protected override void InternalExecute()
        {
            var label = this.Label.Get(this.ActivityContext);
            var vcs = this.VersionControlServer.Get(this.ActivityContext);

            VersionControlLabel vclabel = null;
            if (!string.IsNullOrEmpty(label))
            {
                string str;
                string str2;
                LabelSpec.Parse(label, null, false, out str, out str2);

                if (!string.IsNullOrEmpty(str))
                {
                    var labels = vcs.QueryLabels(str, str2, null, true);
                    if (labels.Length > 0)
                    {
                        vclabel = labels[0];
                    }
                }
            }

            this.VersionControlLabel.Set(this.ActivityContext, vclabel);
        }
    }
}
