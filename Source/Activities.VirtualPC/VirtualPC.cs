//-----------------------------------------------------------------------
// <copyright file="VirtualPC.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.VirtualPC.Interop;

    using TfsBuildExtensions.Activities.Virtualization.Extended;

    /// <summary>
    ///  Possible action for the activity
    /// </summary>
    public enum VirtualPCAction
    {
        /// <summary>
        /// Add a VHD
        /// </summary>
        AddHardDiskConnection,

        /// <summary>
        /// Discard the save state
        /// </summary>
        DiscardSavedState,

        /// <summary>
        /// Discard the undo disks
        /// </summary>
        DiscardUndoDisks,

        /// <summary>
        /// Checks of the VM is runnng
        /// </summary>
        IsHeartBeating,

        /// <summary>
        /// Checks if the VM is locked
        /// </summary>
        IsScreenLocked,

        /// <summary>
        /// Lists the VMs on host
        /// </summary>
        List,

        /// <summary>
        /// Logs of the guest OS
        /// </summary>
        LogOff,

        /// <summary>
        /// Merges the undo disks
        /// </summary>
        MergeUndoDisks,

        /// <summary>
        /// Pauses the VM
        /// </summary>
        Pause,

        /// <summary>
        /// Removed a VHD
        /// </summary>
        RemoveHardDiskConnection,

        /// <summary>
        /// Resets the VM
        /// </summary>
        Reset,

        /// <summary>
        /// Retarts the VM
        /// </summary>
        Restart,

        /// <summary>
        /// Resumes a paused VM
        /// </summary>
        Resume,

        /// <summary>
        /// Saves the state of VM
        /// </summary>
        Save,

        /// <summary>
        /// Shuts down the guest OS
        /// </summary>
        Shutdown,

        /// <summary>
        /// Starts a VM
        /// </summary>
        Startup,

        /// <summary>
        /// Takes a BMP screenshot
        /// </summary>
        TakeScreenshot,

        /// <summary>
        /// Turns off the VM
        /// </summary>
        Turnoff,

        /// <summary>
        /// Pauses execution unitl the CPU is at the given level
        /// </summary>
        WaitForLowCpuUtilization,

        /// <summary>
        /// Runs a series of commands on te guest OS
        /// </summary>
        RunScript,
    }

    /// <summary>
    /// A set of tools to manage VirtualPCs<para/>
    /// <b>Valid Action values are:</b>
    /// <para><i>AddHardDiskConnection</i></para>
    /// <para><i>DiscardSavedState</i></para>
    /// <para><i>DiscardUndoDisks</i></para>
    /// <para><i>IsHeartBeating</i></para>
    /// <para><i>IsScreenLocked</i></para>
    /// <para><i>List</i></para>
    /// <para><i>LogOff</i></para>
    /// <para><i>MergeUndoDisks</i></para>
    /// <para><i>Pause</i></para>
    /// <para><i>RemoveHardDiskConnection</i></para>
    /// <para><i>Reset</i></para>
    /// <para><i>Restart</i></para>
    /// <para><i>Resume</i></para>
    /// <para><i>Save</i></para>
    /// <para><i>Shutdown</i></para>
    /// <para><i>Startup</i></para>
    /// <para><i>TakeScreenshot</i></para>
    /// <para><i>Turnoff</i></para>
    /// <para><i>WaitForLowCpuUtilization</i></para>
    /// <para><i>RunScript</i></para>
    /// </summary>
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class VirtualPC : TfsBuildExtensions.Activities.BaseCodeActivity
    {
        /// <summary>
        /// The instance of a virtualPC
        /// </summary>
        private VMVirtualPC virtualPC;

        /// <summary>
        /// The guest VM 
        /// </summary>
        private VMVirtualMachine virtualMachine;

        /// <summary>
        /// The action to perform
        /// </summary>
        private VirtualPCAction action = VirtualPCAction.Startup;

        /// <summary>
        /// The name of the VM to perform operations on.
        /// </summary>
        [Description("The name of the VM to perform operations on.")]
        public InArgument<string> VMName { get; set; }

        /// <summary>
        /// The task to perform.
        /// </summary>
        [RequiredArgument]
        [Description("The task to perform.")]
        public VirtualPCAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The result of Is... status actions
        /// </summary>
        [Description("The result of Is... status actions")]
        public OutArgument<bool> Result { get; set; }

        /// <summary>
        /// The command executed without error, does not mean VM completed the actions e.g. the state may not have actually changed
        /// </summary>
        [Description("The command executed without error, does not mean VM completed the actions e.g. the state may not have actually changed")]
        public OutArgument<bool> Success { get; set; }

        /// <summary>
        /// Gets the collection of Virtual Machines. See the sample for available metadata
        /// </summary>
        [Description(" Gets the collection of Virtual Machines. See the sample for available metadata")]
        public OutArgument<string[]> VirtualMachines { get; set; }

        /// <summary>
        /// A list of key depressions and mouse clicks
        /// </summary>
        [Description("A list of key depressions and mouse clicks")]
        public InArgument<ScriptItem[]> Script { get; set; }

        /// <summary>
        /// Sets the MaxCpuUsage in %. Default is 10
        /// </summary>
        [Description("Sets the MaxCpuUsage in %. Default is 10")]
        public InArgument<int> MaxCpuUsage { get; set; }

        /// <summary>
        /// Sets the MaxCpuThreshold in seconds. This is the period for which the virtual machine must be below the MaxCpuUsage. Default is 10.
        /// </summary>
        [Description("Sets the MaxCpuThreshold in seconds. This is the period for which the virtual machine must be below the MaxCpuUsage. Default is 10.")]
        public InArgument<int> MaxCpuThreshold { get; set; }

        /// <summary>
        /// Gets the number of virtual machines found
        /// </summary>
        [Description("Gets the number of virtual machines found")]
        public OutArgument<int> VirtualMachineCount { get; set; }

        /// <summary>
        /// Sets the FileName used for adding disks and taking screen shots
        /// </summary>
        [Description("Sets the FileName used for adding disks and taking screen shots")]
        public InArgument<string> FileName { get; set; }

        /// <summary>
        /// Sets the device to which the drive will be attached. 0 = The drive will be attached to the first device on the bus. 1 = The drive will be attached to the second device on the bus.
        /// </summary>
        [Description("Sets the device to which the drive will be attached. 0 = The drive will be attached to the first device on the bus. 1 = The drive will be attached to the second device on the bus.")]
        public InArgument<int> DeviceNumber { get; set; }

        /// <summary>
        /// Sets the bus to which the drive will be attached. 0 = The drive will be attached to the first bus. 1 = The drive will be attached to the second bus.
        /// </summary>
        [Description("Sets the bus to which the drive will be attached. 0 = The drive will be attached to the first bus. 1 = The drive will be attached to the second bus.")]
        public InArgument<int> BusNumber { get; set; }

        /// <summary>
        /// The time, in milliseconds, that this method will wait for task completion before returning control to the caller. A value of -1 specifies that method will wait until the task completes without timing out. Other valid timeout values range from 0 to 4,000,000 milliseconds.
        /// </summary>
        [Description("The time, in milliseconds, that this method will wait for task completion before returning control to the caller. A value of -1 specifies that method will wait until the task completes without timing out. Other valid timeout values range from 0 to 4,000,000 milliseconds.")]
        public int WaitForCompletion { get; set; }

        /// <summary>
        /// InternalExecute
        /// </summary>
        protected override void InternalExecute()
        {
            this.virtualPC = new VMVirtualPC();
            switch (this.action)
            {
                case VirtualPCAction.List:
                    this.GetListOfVMs();
                    break;
                case VirtualPCAction.AddHardDiskConnection:
                case VirtualPCAction.DiscardSavedState:
                case VirtualPCAction.DiscardUndoDisks:
                case VirtualPCAction.LogOff:
                case VirtualPCAction.MergeUndoDisks:
                case VirtualPCAction.Pause:
                case VirtualPCAction.RemoveHardDiskConnection:
                case VirtualPCAction.Reset:
                case VirtualPCAction.Restart:
                case VirtualPCAction.Resume:
                case VirtualPCAction.Save:
                case VirtualPCAction.Shutdown:
                case VirtualPCAction.Startup:
                case VirtualPCAction.Turnoff:
                    this.ControlVM();
                    break;
                case VirtualPCAction.IsScreenLocked:
                    this.IsScreenLocked();
                    break;
                case VirtualPCAction.IsHeartBeating:
                    this.IsHeartBeating();
                    break;
                case VirtualPCAction.WaitForLowCpuUtilization:
                    this.WaitForLowCpuUtilization();
                    break;
                case VirtualPCAction.TakeScreenshot:
                    this.TakeScreenshot();
                    break;
                case VirtualPCAction.RunScript:
                    this.RunScript();
                    break;
                default:
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Invalid action passed: {0}", this.action));
                    return;
            }
        }

        private static VMDetails GetVirtualMachineDetails(IVMVirtualMachine virtualMachine)
        {
            var newItem = new VMDetails(virtualMachine.Name);
            newItem.SetProperty("BaseBoardSerialNumber", virtualMachine.BaseBoardSerialNumber);
            newItem.SetProperty("BIOSGUID", virtualMachine.BIOSGUID);
            newItem.SetProperty("BIOSSerialNumber", virtualMachine.BIOSSerialNumber);
            newItem.SetProperty("ChassisAssetTag", virtualMachine.ChassisAssetTag);
            newItem.SetProperty("ChassisSerialNumber", virtualMachine.ChassisSerialNumber);
            newItem.SetProperty("Memory", virtualMachine.Memory.ToString(CultureInfo.InvariantCulture));
            newItem.SetProperty("Name", virtualMachine.Name);
            newItem.SetProperty("Notes", virtualMachine.Notes);
            newItem.SetProperty("Undoable", virtualMachine.Undoable.ToString(CultureInfo.InvariantCulture));

            if (virtualMachine.State == VMVMState.vmVMState_Running)
            {
                newItem.SetProperty("CanShutdown", virtualMachine.GuestOS.CanShutdown.ToString(CultureInfo.InvariantCulture));
                newItem.SetProperty("ComputerName", virtualMachine.GuestOS.ComputerName);
                newItem.SetProperty("IntegrationComponentsVersion", virtualMachine.GuestOS.IntegrationComponentsVersion);
                newItem.SetProperty("IsHeartbeating", virtualMachine.GuestOS.IsHeartbeating.ToString());
                newItem.SetProperty("IsHostTimeSyncEnabled", virtualMachine.GuestOS.IsHostTimeSyncEnabled.ToString());
                newItem.SetProperty("MultipleUserSessionsAllowed", virtualMachine.GuestOS.MultipleUserSessionsAllowed.ToString());
                newItem.SetProperty("OSBuildNumber", virtualMachine.GuestOS.OSBuildNumber);
                newItem.SetProperty("OSMajorVersion", virtualMachine.GuestOS.OSMajorVersion);
                newItem.SetProperty("OSMinorVersion", virtualMachine.GuestOS.OSMinorVersion);
                newItem.SetProperty("OSName", virtualMachine.GuestOS.OSName);
                newItem.SetProperty("OSPlatformId", virtualMachine.GuestOS.OSPlatformId);
                newItem.SetProperty("OSVersion", virtualMachine.GuestOS.OSVersion);
                newItem.SetProperty("ServicePackMajor", virtualMachine.GuestOS.ServicePackMajor);
                newItem.SetProperty("ServicePackMinor", virtualMachine.GuestOS.ServicePackMinor);
                newItem.SetProperty("TerminalServerPort", virtualMachine.GuestOS.TerminalServerPort.ToString(CultureInfo.InvariantCulture));
                newItem.SetProperty("TerminalServicesInitialized", virtualMachine.GuestOS.TerminalServicesInitialized.ToString(CultureInfo.InvariantCulture));
                newItem.SetProperty("UpTime", virtualMachine.Accountant.UpTime.ToString(CultureInfo.InvariantCulture));
            }

            return newItem;
        }

        private void GetListOfVMs()
        {
            this.LogBuildMessage("Listing Virtual Machines", BuildMessageImportance.Low);
            var virtualMachineNames = new List<string>();
            foreach (VMVirtualMachine vm in this.virtualPC.VirtualMachines)
            {
                virtualMachineNames.Add(GetVirtualMachineDetails(vm).Name);
            }

            this.VirtualMachines.Set(this.ActivityContext, virtualMachineNames.ToArray());
            this.VirtualMachineCount.Set(this.ActivityContext, virtualMachineNames.Count);
        }

        private void RunScript()
        {
            this.Success.Set(this.ActivityContext, false);
            if (this.GetVirtualMachine())
            {
                foreach (var item in this.Script.Get(this.ActivityContext))
                {
                    switch (item.Action)
                    {
                        case ScriptAction.ClickLeft:
                            this.LogBuildMessage("Left-click mouse", BuildMessageImportance.Low);
                            this.virtualMachine.Mouse.Click(VMMouseButton.vmMouseButton_Left);
                            break;
                        case ScriptAction.ClickRight:
                            this.LogBuildMessage("Right-click mouse", BuildMessageImportance.Low);
                            this.virtualMachine.Mouse.Click(VMMouseButton.vmMouseButton_Right);
                            break;
                        case ScriptAction.ClickCenter:
                            this.LogBuildMessage("Middle-click mouse", BuildMessageImportance.Low);
                            this.virtualMachine.Mouse.Click(VMMouseButton.vmMouseButton_Center);
                            break;
                        case ScriptAction.TypeAsciiText:
                            this.LogBuildMessage(string.Format("Type Ascii Text [{0}]", item.Text), BuildMessageImportance.Low);
                            this.virtualMachine.Keyboard.TypeAsciiText(item.Text);
                            break;
                        case ScriptAction.TypeKeySequence:
                            this.LogBuildMessage(string.Format("Type Key Sequence [{0}]", item.Text), BuildMessageImportance.Low);
                            this.virtualMachine.Keyboard.TypeKeySequence(item.Text);
                            break;
                        default:
                            this.LogBuildError(string.Format("Invalid script action called [{0}]", item.Action));
                            this.Success.Set(this.ActivityContext, false);
                            return;
                    }
                }

                this.Success.Set(this.ActivityContext, true);
            }
        }

        private void TakeScreenshot()
        {
            this.Success.Set(this.ActivityContext, false);
            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Taking screenshot of: {0}", this.VMName.Get(this.ActivityContext)));

            if (string.IsNullOrEmpty(this.FileName.Get(this.ActivityContext)))
            {
                this.LogBuildError("No filename provided");
                return;
            }

            if (!this.GetVirtualMachine())
            {
                return;
            }

            VMDisplay display = this.virtualMachine.Display;
            if (display != null)
            {
                object thumbnailObject = display.Thumbnail;
                object[] thumbnail = (object[])thumbnailObject;
                using (Bitmap bmp = new Bitmap(64, 48, PixelFormat.Format32bppRgb))
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            uint pixel = (uint)thumbnail[(y * bmp.Width) + x];

                            int b = (int)((pixel & 0xff000000) >> 24);
                            int g = (int)((pixel & 0x00ff0000) >> 16);
                            int r = (int)((pixel & 0x0000ff00) >> 8);

                            bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                        }
                    }

                    bmp.Save(this.FileName.Get(this.ActivityContext));
                    this.Success.Set(this.ActivityContext, true);
                }
            }
        }

        private bool GetVirtualMachine()
        {
            if (string.IsNullOrEmpty(this.VMName.Get(this.ActivityContext)))
            {
                this.LogBuildError("Name is required.");
                return false;
            }

            this.virtualMachine = this.virtualPC.FindVirtualMachine(this.VMName.Get(this.ActivityContext));
            if (this.virtualMachine == null)
            {
                this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Virtual Machine: {0} not found", this.VMName.Get(this.ActivityContext)));
                return false;
            }

            return true;
        }

        private void WaitForLowCpuUtilization()
        {
            this.Success.Set(this.ActivityContext, false);
            if (this.GetVirtualMachine())
            {
                var maxCpuUsage = 10;
                if ((this.MaxCpuUsage.Get(this.ActivityContext) > 0) && this.MaxCpuUsage.Get(this.ActivityContext) <= 100)
                {
                    maxCpuUsage = this.MaxCpuUsage.Get(this.ActivityContext);
                }
                else
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "MaxCpuUsage out of range set to default [{0}]", maxCpuUsage));
                }

                var maxCpuThreshold = 10;
                if (this.MaxCpuThreshold.Get(this.ActivityContext) > 0)
                {
                    maxCpuThreshold = this.MaxCpuThreshold.Get(this.ActivityContext);
                }
                else
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "MaxCpuThreshold out of range set to default [{0}]", maxCpuThreshold));
                }

                if (this.virtualMachine.State != VMVMState.vmVMState_Running)
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "Virtual Machine: {0} is not running: {1}", this.VMName.Get(this.ActivityContext), this.virtualMachine.State));
                }
                else
                {
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Waiting for CPU utilisation below [{1}%] on [{0}], with wait a max of [{2}] seconds", this.VMName.Get(this.ActivityContext), maxCpuUsage, maxCpuThreshold));
                    int belowMaxCount = 0;
                    while (belowMaxCount < maxCpuThreshold)
                    {
                        if (this.virtualMachine.Accountant.CPUUtilization < maxCpuUsage)
                        {
                            belowMaxCount++;
                        }
                        else
                        {
                            belowMaxCount = 0;
                        }

                        Thread.Sleep(1000);
                    }

                    this.Success.Set(this.ActivityContext, true);
                }
            }
        }

        private void IsHeartBeating()
        {
            if (this.GetVirtualMachine())
            {
                this.Result.Set(this.ActivityContext, this.virtualMachine.GuestOS.IsHeartbeating);
                this.Success.Set(this.ActivityContext, true);
            }
            else
            {
                this.Success.Set(this.ActivityContext, false);
            }
        }

        private void IsScreenLocked()
        {
            if (this.GetVirtualMachine())
            {
                this.Result.Set(this.ActivityContext, this.virtualMachine.GuestOS.ScreenLocked);
                this.Success.Set(this.ActivityContext, true);
            }
            else
            {
                this.Success.Set(this.ActivityContext, false);
            }
        }

        private void ControlVM()
        {
            this.Success.Set(this.ActivityContext, false);
            if (this.GetVirtualMachine())
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "{0} Virtual Machine: {1}", this.action, this.VMName.Get(this.ActivityContext)));

                try
                {
                    switch (this.action)
                    {
                        case VirtualPCAction.LogOff:
                            this.Logoff();
                            break;
                        case VirtualPCAction.Restart:
                            this.Restart();
                            break;
                        case VirtualPCAction.Startup:
                            this.Startup();
                            break;
                        case VirtualPCAction.Turnoff:
                            this.Turnoff();
                            break;
                        case VirtualPCAction.Shutdown:
                            this.Shutdown();
                            break;
                        case VirtualPCAction.DiscardUndoDisks:
                            if (!this.DiscardUndoDisk())
                            {
                                return;
                            }

                            break;
                        case VirtualPCAction.DiscardSavedState:
                            if (!this.DiscardSavedState())
                            {
                                return;
                            }

                            break;
                        case VirtualPCAction.MergeUndoDisks:
                            if (!this.MergeUndoDisk())
                            {
                                return;
                            }

                            break;
                        case VirtualPCAction.Pause:
                            this.Pause();

                            break;
                        case VirtualPCAction.Resume:
                            this.Resume();
                            break;
                        case VirtualPCAction.Reset:
                            this.Reset();

                            break;
                        case VirtualPCAction.Save:
                            this.Save();

                            break;
                        case VirtualPCAction.AddHardDiskConnection:
                            if (!this.AddDisk())
                            {
                                return;
                            }

                            break;
                        case VirtualPCAction.RemoveHardDiskConnection:
                            if (!this.RemoveDisk())
                            {
                                return;
                            }

                            break;
                    }

                    this.Success.Set(this.ActivityContext, true);
                }
                catch (COMException ex)
                {
                    // Occurs when the vm cannot be found
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "{0} Virtual Machine: {1}", this.VMName.Get(this.ActivityContext), ex.Message));
                }
            }
        }

        private bool MergeUndoDisk()
        {
            var completed = false;
            if ((this.virtualMachine.State == VMVMState.vmVMState_TurnedOff ||
                                            this.virtualMachine.State == VMVMState.vmVMState_Saved)
                                            && this.virtualMachine.Undoable)
            {
                var s = this.virtualMachine.MergeUndoDisks();
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }

                completed = true;
            }
            else
            {
                LogBuildError("Cannot merge undo disks as VM is not stopped or saved");
            }

            return completed;
        }

        private bool DiscardSavedState()
        {
            var completed = false;
            if (this.virtualMachine.State == VMVMState.vmVMState_Saved)
            {
                this.virtualMachine.DiscardSavedState();
                completed = true;
            }
            else
            {
                LogBuildError("Cannot discard saved state as VM is not stopped");
            }

            return completed;
        }

        private bool DiscardUndoDisk()
        {
            var completed = false;
            if (this.virtualMachine.State == VMVMState.vmVMState_TurnedOff)
            {
                this.virtualMachine.DiscardUndoDisks();
                completed = true;
            }
            else
            {
                LogBuildError("Cannot discard undo disks as VM is not stopped");
                completed = true;
            }

            return completed;
        }

        private bool AddDisk()
        {
            var completed = false;
            if (string.IsNullOrEmpty(this.FileName.Get(this.ActivityContext)))
            {
                this.LogBuildError("FileName not provided");
            }
            else if (this.virtualMachine.State != VMVMState.vmVMState_Running)
            {
                this.virtualMachine.AddHardDiskConnection(this.FileName.Get(this.ActivityContext), this.BusNumber.Get(this.ActivityContext), this.DeviceNumber.Get(this.ActivityContext));
                completed = true;
            }
            else
            {
                this.LogBuildError("Cannot add disk as virtual machine is running");
            }

            return completed;
        }

        private bool RemoveDisk()
        {
            var completed = false;
            if (string.IsNullOrEmpty(this.FileName.Get(this.ActivityContext)))
            {
                this.LogBuildError("FileName not provided");
            }
            else if (this.virtualMachine.State != VMVMState.vmVMState_Running)
            {
                foreach (VMHardDiskConnection vhd in this.virtualMachine.HardDiskConnections)
                {
                    if (vhd.HardDisk.File == this.FileName.Get(this.ActivityContext))
                    {
                        this.virtualMachine.RemoveHardDiskConnection(vhd);
                        completed = true;
                    }
                }
            }

            return completed;
        }

        private void Turnoff()
        {
            if (this.virtualMachine.State != (VMVMState.vmVMState_TurnedOff | VMVMState.vmVMState_TurningOff))
            {
                var s = this.virtualMachine.TurnOff();
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }

        private void Startup()
        {
            if (this.virtualMachine.State == VMVMState.vmVMState_TurnedOff)
            {
                var s = this.virtualMachine.Startup();
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }

        private void Restart()
        {
            if (this.virtualMachine.State != (VMVMState.vmVMState_TurnedOff | VMVMState.vmVMState_TurningOff))
            {
                var s = this.virtualMachine.GuestOS.Restart(true);
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }

        private void Shutdown()
        {
            if (this.virtualMachine.State != (VMVMState.vmVMState_TurnedOff | VMVMState.vmVMState_TurningOff))
            {
                var s = this.virtualMachine.GuestOS.Shutdown(true);
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }

        private void Save()
        {
            if (this.virtualMachine.State == VMVMState.vmVMState_Running)
            {
                var s = this.virtualMachine.Save();
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }

        private void Resume()
        {
            if (this.virtualMachine.State == VMVMState.vmVMState_Paused)
            {
                this.virtualMachine.Resume();
            }
        }

        private void Pause()
        {
            if (this.virtualMachine.State == VMVMState.vmVMState_Running)
            {
                this.virtualMachine.Pause();
            }
        }

        private void Reset()
        {
            if (this.virtualMachine.State != (VMVMState.vmVMState_TurnedOff | VMVMState.vmVMState_TurningOff))
            {
                var s = this.virtualMachine.Reset();
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }

        private void Logoff()
        {
            if (this.virtualMachine.State != (VMVMState.vmVMState_TurnedOff | VMVMState.vmVMState_TurningOff))
            {
                var s = this.virtualMachine.GuestOS.Logoff();
                if (this.WaitForCompletion > 0)
                {
                    s.WaitForCompletion(this.WaitForCompletion);
                }
            }
        }
    }
}
