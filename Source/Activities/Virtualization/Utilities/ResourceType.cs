//-----------------------------------------------------------------------
// <copyright file="ResourceType.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization.Utilities
{
    internal static class ResourceType
    {
        public const ushort Other = 1;
        public const ushort ComputerSystem = 2;
        public const ushort Processor = 3;
        public const ushort Memory = 4;
        public const ushort IDEController = 5;
        public const ushort ParallelSCSIHBA = 6;
        public const ushort FCHBA = 7;
        public const ushort ISCSIHBA = 8;
        public const ushort IBHCA = 9;
        public const ushort EthernetAdapter = 10;
        public const ushort OtherNetworkAdapter = 11;
        public const ushort IOSlot = 12;
        public const ushort IODevice = 13;
        public const ushort FloppyDrive = 14;
        public const ushort CDDrive = 15;
        public const ushort DVDdrive = 16;
        public const ushort Serialport = 17;
        public const ushort Parallelport = 18;
        public const ushort USBController = 19;
        public const ushort GraphicsController = 20;
        public const ushort StorageExtent = 21;
        public const ushort Disk = 22;
        public const ushort Tape = 23;
        public const ushort OtherStorageDevice = 24;
        public const ushort FirewireController = 25;
        public const ushort PartitionableUnit = 26;
        public const ushort BasePartitionableUnit = 27;
        public const ushort PowerSupply = 28;
        public const ushort CoolingDevice = 29;

        public const ushort DisketteController = 1;
    }
}
