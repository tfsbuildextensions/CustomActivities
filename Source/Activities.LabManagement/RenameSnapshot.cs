//-----------------------------------------------------------------------
// <copyright file="RenameSnapshot.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.LabManagement
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Lab.Client;

    /*
    *  This Activity Represents a Work in progress and is subject to change without notice until the
    *  corresponding process template has been published.
    */

    /// <summary>
    /// An activity that renames a snapshot attached to a TFS Lab Management Lab Environment.  This activity will only rename a snapshot
    /// that is part of a lab environment that is in a Running state.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class RenameSnapshot : BaseCodeActivity
    {
        /// <summary>
        /// Specifies the name of the environment that contains the snapshot to be renamed. The environment is located 
        /// in the team project executing the build. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string> EnvironmentName { get; set; }

        /// <summary>
        /// Specifies the current name of the snapshot to be renamed. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string> CurrentSnapshotName { get; set; }

        /// <summary>
        /// Specifies the final name of the snapshot to be renamed. 
        /// </summary>
        [RequiredArgument]
        public InArgument<string> NewSnapshotName { get; set; }

        /// <summary>
        /// Execute the RenameSnapshot build activity.
        /// </summary>
        protected override void InternalExecute()
        {
            this.Rename();
        }

        private void Rename()
        {
            var tpc = this.ActivityContext.GetExtension<TfsTeamProjectCollection>();
            var labService = tpc.GetService<LabService>();
            var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            var environments = labService.QueryLabEnvironments(new LabEnvironmentQuerySpec { Project = buildDetail.TeamProject });
            bool foundSnapshot = false;

            var matchingName = this.ActivityContext.GetValue(this.EnvironmentName);
            var originalSnapshotName = this.ActivityContext.GetValue(this.CurrentSnapshotName);
            var finalSnapshotName = this.ActivityContext.GetValue(this.NewSnapshotName);

            this.LogBuildMessage(string.Format("Beginning search for snapshot '{0}' in environment '{1}'.", originalSnapshotName, matchingName));

            // Find our environment
            foreach (var environment in environments)
            {
                this.LogBuildMessage(string.Format("Looking for environment named '{0}'.", matchingName));

                if (environment.Name.ToUpper() == matchingName.ToUpper())
                {
                    this.LogBuildMessage(string.Format("  MATCH!  Found lab environment '{0}'", environment.Name));

                    // Check to make sure the found environment is Running and not Off or Stored in the Library
                    this.LogBuildMessage(string.Format("  The current state of this lab environment is: {0}", environment.StatusInfo.State.ToString()));
                    if (environment.StatusInfo.State == LabEnvironmentState.Running)
                    {
                        // Find the starting snapshot by name
                        List<LabEnvironmentSnapshot> snapshots = environment.QueryLabEnvironmentSnapshots();
                        foreach (LabEnvironmentSnapshot snapshot in snapshots)
                        {
                            this.LogBuildMessage(string.Format("    Looking for snapshot named '{0}'.", originalSnapshotName));
                            if (snapshot.Name.ToUpper() == originalSnapshotName.ToUpper())
                            {
                                this.LogBuildMessage(string.Format("      MATCH! Found snapshot '{0}' - Snapshot Id: {1}", snapshot.Name, snapshot.Id));
                                foundSnapshot = true;

                                // Rename our snapshot
                                environment.UpdateLabEnvironmentSnapshot(snapshot.Id, finalSnapshotName, snapshot.Description);
                                this.LogBuildMessage(string.Format("        Renamed snapshot '{0}' to '{1}' in the '{2}' environment.", snapshot.Name, finalSnapshotName, environment.Name));
                                
                                break;
                            }

                            this.LogBuildMessage(string.Format("      NO MATCH! Found snapshot '{0}'", snapshot.Name));
                        }

                        // Found and renamed snapshot, do not check any other environments.
                        if (foundSnapshot)
                        {
                            this.LogBuildMessage("Found and renamed matching snapshot. No further environments will be checked.");
                            break;
                        }
                    }
                    else
                    {
                            this.LogBuildMessage("  NO MATCH! Only RUNNING environments can be patched.");
                    }
                }
                else
                {
                    this.LogBuildMessage(string.Format("  NO MATCH!  Found lab environment '{0}'", environment.Name));
                }
            }
        }
    }
}