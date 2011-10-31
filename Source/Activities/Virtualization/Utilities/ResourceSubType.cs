//-----------------------------------------------------------------------
// <copyright file="ResourceSubType.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization.Utilities
{
    internal static class ResourceSubType
    {
        public const string DisketteController = null;
        public const string DisketteDrive = "Microsoft Synthetic Diskette Drive";
        public const string ParallelSCSIHBA = "Microsoft Synthetic SCSI Controller";
        public const string IDEController = "Microsoft Emulated IDE Controller";
        public const string DiskSynthetic = "Microsoft Synthetic Disk Drive";
        public const string DiskPhysical = "Microsoft Physical Disk Drive";
        public const string DVDPhysical = "Microsoft Physical DVD Drive";
        public const string DVDSynthetic = "Microsoft Synthetic DVD Drive";
        public const string CDROMPhysical = "Microsoft Physical CD Drive";
        public const string CDROMSynthetic = "Microsoft Synthetic CD Drive";
        public const string EthernetSynthetic = "Microsoft Synthetic Ethernet Port";

        // logical drive
        public const string DVDLogical = "Microsoft Virtual CD/DVD Disk";
        public const string ISOImage = "Microsoft ISO Image";
        public const string VHD = "Microsoft Virtual Hard Disk";
        public const string DVD = "Microsoft Virtual DVD Disk";
        public const string VFD = "Microsoft Virtual Floppy Disk";
        public const string VideoSynthetic = "Microsoft Synthetic Display Controller";
    }
}
