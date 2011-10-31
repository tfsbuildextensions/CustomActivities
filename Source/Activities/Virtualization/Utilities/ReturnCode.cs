//-----------------------------------------------------------------------
// <copyright file="ReturnCode.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Virtualization.Utilities
{
    internal static class ReturnCode
    {
        public const uint Completed = 0;
        public const uint Started = 4096;
        public const uint Failed = 32768;
        public const uint AccessDenied = 32769;
        public const uint NotSupported = 32770;
        public const uint Unknown = 32771;
        public const uint Timeout = 32772;
        public const uint InvalidParameter = 32773;
        public const uint SystemInUse = 32774;
        public const uint InvalidState = 32775;
        public const uint IncorrectDataType = 32776;
        public const uint SystemNotAvailable = 32777;
        public const uint OutofMemory = 32778;
    }
}
