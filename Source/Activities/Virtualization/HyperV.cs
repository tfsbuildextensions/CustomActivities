//-----------------------------------------------------------------------
// <copyright file="HyperV.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Globalization;
    using System.Management;
    using Microsoft.TeamFoundation.Build.Client;

    using TfsBuildExtensions.Activities.Virtualization.Extended;
    using TfsBuildExtensions.Activities.Virtualization.Utilities;
    
    /// <summary>
    ///  Possible action for the activity
    /// </summary>
    public enum HyperVAction
    {
        /// <summary>
        /// Starts the VM
        /// </summary>
        Start = 0,

        /// <summary>
        /// Shutdown the VM Guest OS
        /// </summary>
        Shutdown,

        /// <summary>
        /// Stwich off the VM
        /// </summary>
        Turnoff,

        /// <summary>
        /// Pause the VM
        /// </summary>
        Pause,

        /// <summary>
        /// Save the state of the VM to disk
        /// </summary>
        Suspend,

        /// <summary>
        /// Restarts the VM
        /// </summary>
        Restart,

        /// <summary>
        /// Takes a snapshot of the VM
        /// </summary>
        Snapshot,

        /// <summary>
        /// Restores last snapshot of the VM
        /// </summary>
        ApplyLastSnapshot,

        /// <summary>
        /// Restores named snapshot of the VM
        /// </summary>
        ApplyNamedSnapshot,
    }

    /// <summary>
    /// An activity to perform HyperV operations within a TFS 2010 build<para/>
    /// <b>Valid Action values are:</b>
    /// <para><i>Start</i></para>
    /// <para><i>Shutdown</i></para>
    /// <para><i>Turnoff</i></para>
    /// <para><i>Pause</i></para>
    /// <para><i>Suspend</i></para>
    /// <para><i>Restart</i></para>
    /// <para><i>Snapshot</i></para>
    /// <para><i>ApplyLastSnapshot</i></para>
    /// <para><i>ApplyNamedSnapshot</i></para>
    /// </summary>
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class HyperV : TfsBuildExtensions.Activities.BaseCodeActivity
    {
        /// <summary>
        /// The action to perform
        /// </summary>
        private HyperVAction action = HyperVAction.Start;

        /// <summary>
        /// The name of the server host the VM. '.' can be used for the localhost.
        /// </summary>
        [RequiredArgument]
        [Description("The name of the server host the VM. '.' can be used for the localhost.")]
        public InArgument<string> ServerName { get; set; }

        /// <summary>
        /// The name of the VM to perform operations on.
        /// </summary>
        [RequiredArgument]
        [Description("The name of the VM to perform operations on.")]
        public InArgument<string> VMName { get; set; }

        /// <summary>
        /// The name of the snapshot to restore to the VM 
        /// </summary>
        [Description("The name of the snapshot to restore to the VM.")]
        public InArgument<string> SnapshotName { get; set; }

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public HyperVAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Gets whether the action succeeded.
        /// </summary>
        public OutArgument<bool> Succeeded { get; set; }

        /// <summary>
        /// Calls the actual method
        /// </summary>
        protected override void InternalExecute()
        {
            var returnValue = false;

            // the [RequiredArguement] does not check for zero length strings
            if (string.IsNullOrEmpty(this.ServerName.Get(this.ActivityContext)))
            {
                throw new ArgumentException("No Hyper-V server name cannot be zero length, use name for . for local host");
            }

            if (string.IsNullOrEmpty(this.VMName.Get(this.ActivityContext)))
            {
                throw new ArgumentException("The VM name cannot be zero length");
            }

            this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "[{1}] to be performed on VM [{0}]", this.VMName.Get(this.ActivityContext), this.action));
            switch (this.action)
            {
                case HyperVAction.Start:
                case HyperVAction.Turnoff:
                case HyperVAction.Pause:
                case HyperVAction.Suspend:
                case HyperVAction.Restart:
                    returnValue = this.RequestStateChange();
                    break;
                case HyperVAction.Shutdown:
                    returnValue = this.RequestShutdown();
                    break;
                case HyperVAction.Snapshot:
                    returnValue = this.CreateVirtualSystemSnapshot();
                    break;
                case HyperVAction.ApplyLastSnapshot:
                    returnValue = this.ApplyLastVirtualSystemSnapshot();
                    break;
                case HyperVAction.ApplyNamedSnapshot:
                    returnValue = this.ApplyNamedVirtualSystemSnapshot();
                    break;
                default:
                    this.LogBuildError(string.Format(CultureInfo.InvariantCulture, "Non Implemented action requested [{0}]", this.action));
                    break;
            }

            this.Succeeded.Set(this.ActivityContext, returnValue);
        }

        /// <summary>
        /// Gets the requested VM
        /// </summary>
        /// <param name="scope">The HyperV Server scope</param>
        /// <param name="vmname">The name of the target VM</param>
        /// <returns>The VM management object</returns>
        private static ManagementObject GetVm(ManagementScope scope, string vmname)
        {
            ManagementObject vm = Utility.GetTargetComputer(vmname, scope);

            if (null == vm)
            {
                throw new HyperVException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "The virtual machine '{0}' could not be found.",
                    vmname));
            }

            return vm;
        }

        /// <summary>
        /// Shutdown the names guest VM
        /// </summary>
        /// <returns>True if succeeds</returns>
        private bool RequestShutdown()
        {
            // based on the sample at http://blogs.msdn.com/b/taylorb/archive/2008/05/21/hyper-v-wmi-using-powershell-part-4-and-negative-1.aspx
            var jobSuccesful = false;

            // Connect to the Remote Machines Management Scope
            ManagementScope scope = this.GetScope();

            // Get the msvm_computersystem for the given VM (Vista)         
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Msvm_ComputerSystem WHERE ElementName = '{0}'", this.VMName.Get(this.ActivityContext)))))
            {
                try
                {
                    // Select the first object in the Searcher collection
                    var enumr = searcher.Get().GetEnumerator();
                    enumr.MoveNext();
                    ManagementObject msvm_computersystem = (ManagementObject)enumr.Current;

                    // Use the association to get the msvm_shutdowncomponent for the msvm_computersystem
                    ManagementObjectCollection collection = msvm_computersystem.GetRelated("Msvm_ShutdownComponent");
                    ManagementObjectCollection.ManagementObjectEnumerator enumerator = collection.GetEnumerator();
                    enumerator.MoveNext();
                    ManagementObject msvm_shutdowncomponent = (ManagementObject)enumerator.Current;

                    // Get the InitiateShudown Parameters
                    ManagementBaseObject inParams = msvm_shutdowncomponent.GetMethodParameters("InitiateShutdown");
                    inParams["Force"] = true;
                    inParams["Reason"] = "Need to Shutdown";

                    // Invoke the Method
                    ManagementBaseObject outParams = msvm_shutdowncomponent.InvokeMethod("InitiateShutdown", inParams, null);
                    uint returnValue = (uint)outParams["ReturnValue"];

                    // Zero indicates success
                    if (returnValue != 0)
                    {
                        this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "SHUTDOWN of {0} Failed", this.VMName.Get(this.ActivityContext)));
                    }
                    else
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "{0} state was shutdown successfully.", this.VMName.Get(this.ActivityContext)));
                        jobSuccesful = true;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "SHUTDOWN of {0} Failed due to {1}", this.VMName.Get(this.ActivityContext), ex.Message));
                }
            }

            return jobSuccesful;
        }

        /// <summary>
        /// Gets the scope of the target HyperV server 
        /// </summary>
        /// <returns>A hyperV server scope</returns>
        private ManagementScope GetScope()
        {
            return new ManagementScope(string.Format(CultureInfo.InvariantCulture, @"\\{0}\root\virtualization", this.ServerName.Get(this.ActivityContext)), null);
        }

        /// <summary>
        /// Requests that the state of the running VM is changed, these are HyperV operations
        /// </summary>
        /// <returns>True if the action was completed</returns>
        private bool RequestStateChange()
        {
            // based on http://msdn.microsoft.com/en-us/library/cc723875(v=VS.85).aspx
            var jobSuccesful = false;
            ManagementScope scope = this.GetScope();
            ManagementObject vm = GetVm(scope, this.VMName.Get(this.ActivityContext));

            ManagementBaseObject inParams = vm.GetMethodParameters("RequestStateChange");

            const int Enabled = 2;  // Turns the VM on
            const int Disabled = 3; // Turns the VM off
            const int Reboot = 10; // A hard reset of the VM.
            const int Pause = 32768; // Pauses the VM.
            const int Suspend = 32769; // Saves the state of the VM.

            switch (this.action)
            {
                case HyperVAction.Start:
                    inParams["RequestedState"] = Enabled;
                    break;
                case HyperVAction.Turnoff:
                    inParams["RequestedState"] = Disabled;
                    break;
                case HyperVAction.Pause:
                    inParams["RequestedState"] = Pause;
                    break;
                case HyperVAction.Suspend:
                    inParams["RequestedState"] = Suspend;
                    break;
                case HyperVAction.Restart:
                    inParams["RequestedState"] = Reboot;
                    break;
                default:
                    throw new HyperVException(string.Format(CultureInfo.InvariantCulture, "Non-implemented action type {0} is specified", this.action));
            }

            ManagementBaseObject outParams = vm.InvokeMethod(
                "RequestStateChange",
                inParams,
                null);

            switch ((uint)outParams["ReturnValue"])
            {
                case ReturnCode.Started:
                    if (Utility.JobCompleted(outParams, scope))
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "[{0}] state was changed successfully.", this.VMName.Get(this.ActivityContext)));
                        jobSuccesful = true;
                    }
                    else
                    {
                        this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "State change for VM [{0}].", this.VMName.Get(this.ActivityContext)));
                    }

                    break;
                case ReturnCode.Completed:
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "[{0}] state was changed successfully.", this.VMName.Get(this.ActivityContext)));
                    jobSuccesful = true;
                    break;
                default:
                    this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "State change for VM [{0}].", this.VMName.Get(this.ActivityContext)));
                    break;
            }

            return jobSuccesful;
        }

        /// <summary>
        /// Creates a new snapshot of the named VM
        /// </summary>
        /// <returns>True if completed without error</returns>
        private bool CreateVirtualSystemSnapshot()
        {
            // based on http://msdn.microsoft.com/en-us/library/cc723875(v=VS.85).aspx 
            var jobSuccesful = false;
            ManagementScope scope = this.GetScope();
            ManagementObject vm = GetVm(scope, this.VMName.Get(this.ActivityContext));

            ManagementObject virtualSystemService = Utility.GetServiceObject(scope, "Msvm_VirtualSystemManagementService");
            ManagementBaseObject inParams = virtualSystemService.GetMethodParameters("CreateVirtualSystemSnapshot");
            inParams["SourceSystem"] = vm.Path.Path;

            ManagementBaseObject outParams = virtualSystemService.InvokeMethod("CreateVirtualSystemSnapshot", inParams, null);

            if ((uint)outParams["ReturnValue"] == ReturnCode.Started)
            {
                if (Utility.JobCompleted(outParams, scope))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Snapshot of [{0}] was taken successfully.", this.VMName.Get(this.ActivityContext)));
                    jobSuccesful = true;
                }
                else
                {
                    this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "Failed to create snapshot for VM [{0}].", this.VMName.Get(this.ActivityContext)));
                }
            }
            else if ((uint)outParams["ReturnValue"] == ReturnCode.Completed)
            {
                this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Snapshot of [{0}] was taken successfully.", this.VMName.Get(this.ActivityContext)));
                jobSuccesful = true;
            }
            else
            {
                this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "Failed to create snapshot for VM [{0}]. with error {1}", this.VMName.Get(this.ActivityContext), outParams["ReturnValue"]));
            }

            inParams.Dispose();
            outParams.Dispose();
            vm.Dispose();
            virtualSystemService.Dispose();

            return jobSuccesful;
        }

        /// <summary>
        /// Gets the last snapshot of the named VM snapshot tree
        /// </summary>
        /// <param name="vm">The current management object for the VM</param>
        /// <returns>The object containing the details of the snapshot</returns>
        private ManagementObject GetLastVirtualSystemSnapshot(ManagementObject vm)
        {
            ManagementObjectCollection settings = vm.GetRelated(
                "Msvm_VirtualSystemsettingData",
                "Msvm_PreviousSettingData",
                null,
                null,
                "SettingData",
                "ManagedElement",
                false,
                null);

            ManagementObject virtualSystemsetting = null;
            foreach (ManagementObject setting in settings)
            {
                this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Getting last snapshot from server {0} of name [{1}]", setting.Path.Path, setting["ElementName"]));
                virtualSystemsetting = setting;
            }

            return virtualSystemsetting;
        }

        private ManagementObject GetSnapshotByUniqueName(ManagementObject vm, string snapshotName)
        {
            ManagementObjectCollection settings = vm.GetRelated(
                 "Msvm_VirtualSystemsettingData",
                 null,
                 null,
                 null,
                 "SettingData",
                 "ManagedElement",
                 false,
                 null);

            foreach (ManagementObject item in settings)
            {
                // the 5 is a bit of a magic number, it is the ID of a  VirtualSystemSnapshot
                if ((ushort)item.Properties["SettingType"].Value == 5 &&
                    item.Properties["ElementName"].Value.ToString().StartsWith(snapshotName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Getting named snapshot from server {0} of name [{1}]", item.Path.Path, item["ElementName"]));

                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Reverts the VM to the previous snapshop
        /// </summary>
        /// <returns>True of operation completed without error</returns>
        private bool ApplyNamedVirtualSystemSnapshot()
        {
            var retValue = false;
            ManagementScope scope = this.GetScope();
            ManagementObject vm = GetVm(scope, this.VMName.Get(this.ActivityContext));
            if (string.IsNullOrEmpty(this.SnapshotName.Get(this.ActivityContext)))
            {
                this.LogBuildError(string.Format(CultureInfo.InvariantCulture, "Could not apply snapshot as no name provided"));
            }
            else
            {
                retValue = this.ApplyVirtualSystemSnapshot(
                    vm,
                    this.GetSnapshotByUniqueName(vm, this.SnapshotName.Get(this.ActivityContext)));
            }

            return retValue;
        }

        /// <summary>
        /// Reverts the VM to the previous shapshop
        /// </summary>
        /// <returns>True of operation completed without error</returns>
        private bool ApplyLastVirtualSystemSnapshot()
        {
            ManagementScope scope = this.GetScope();
            ManagementObject vm = GetVm(scope, this.VMName.Get(this.ActivityContext));

            return this.ApplyVirtualSystemSnapshot(
                vm,
                this.GetLastVirtualSystemSnapshot(vm));
        }

        /// <summary>
        /// Reverts the VM to the previous shapshop
        /// </summary>
        /// <param name="vm">The vm to apply the snapstop to</param>
        /// <param name="snapshot">The snapshot image</param>
        /// <returns>True of operation completed without error</returns>
        private bool ApplyVirtualSystemSnapshot(ManagementObject vm, ManagementObject snapshot)
        {
            var jobSuccesful = false;

            ManagementScope scope = vm.Scope;

            ManagementObject virtualSystemService = Utility.GetServiceObject(scope, "Msvm_VirtualSystemManagementService");
            ManagementBaseObject inParams = virtualSystemService.GetMethodParameters("ApplyVirtualSystemSnapshot");
            inParams["SnapshotSettingData"] = snapshot.Path.Path;

            inParams["ComputerSystem"] = vm.Path.Path;

            ManagementBaseObject outParams = virtualSystemService.InvokeMethod("ApplyVirtualSystemSnapshot", inParams, null);

            if ((uint)outParams["ReturnValue"] == ReturnCode.Started)
            {
                if (Utility.JobCompleted(outParams, scope))
                {
                    this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Snapshot of [{0}] was applied successfully.", this.VMName.Get(this.ActivityContext)));
                    jobSuccesful = true;
                }
                else
                {
                    this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "Failed to apply snapshot for VM [{0}].", this.VMName.Get(this.ActivityContext)));
                }
            }
            else if ((uint)outParams["ReturnValue"] == ReturnCode.Completed)
            {
                this.LogBuildMessage(string.Format(CultureInfo.InvariantCulture, "Snapshot of [{0}] was applied successfully.", this.VMName.Get(this.ActivityContext)));
                jobSuccesful = true;
            }
            else
            {
                this.LogBuildWarning(string.Format(CultureInfo.InvariantCulture, "Snapshot of [{0}] was applied but returned error {1}", this.VMName.Get(this.ActivityContext), outParams["ReturnValue"]));
            }

            inParams.Dispose();
            outParams.Dispose();
            snapshot.Dispose();
            vm.Dispose();
            virtualSystemService.Dispose();

            return jobSuccesful;
        }
    }
}
