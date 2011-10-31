//-----------------------------------------------------------------------
// <copyright file="Utility.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
// Based on http://msdn.microsoft.com/en-us/library/cc723875(v=VS.85).aspx
// basically only namespace changed and enought to pass stylecop rules
namespace TfsBuildExtensions.Activities.Virtualization.Utilities
{
    using System;
    using System.Globalization;
    using System.Management;

    internal static class Utility
    {
        private enum ValueRole
        {
            /// <summary>
            /// Value role state
            /// </summary>
            Default = 0,

            /// <summary>
            /// Value role state
            /// </summary>
            Minimum = 1,

            /// <summary>
            /// Value role state
            /// </summary>
            Maximum = 2,

            /// <summary>
            /// Value role state
            /// </summary>
            Increment = 3
        }

        private enum ValueRange
        {
            /// <summary>
            /// Value range state
            /// </summary>
            Default = 0,

            /// <summary>
            /// Value range state
            /// </summary>
            Minimum = 1,

            /// <summary>
            /// Value range state
            /// </summary>
            Maximum = 2,

            /// <summary>
            /// Value range state
            /// </summary>
            Increment = 3
        }

        /// <summary>
        /// Common utility function to get a service object
        /// </summary>
        /// <param name="scope">The scope, in effect the HyperV server</param>
        /// <param name="serviceName">The name of the VM</param>
        /// <returns>A service object</returns>
        public static ManagementObject GetServiceObject(ManagementScope scope, string serviceName)
        {
            scope.Connect();
            ManagementPath wmiPath = new ManagementPath(serviceName);
            ManagementClass serviceClass = new ManagementClass(scope, wmiPath, null);
            ManagementObjectCollection services = serviceClass.GetInstances();

            ManagementObject serviceObject = null;

            foreach (ManagementObject service in services)
            {
                serviceObject = service;
            }

            return serviceObject;
        }

        public static ManagementObject GetHostSystemDevice(string deviceClassName, string deviceObjectElementName, ManagementScope scope)
        {
            string hostName = System.Environment.MachineName;
            ManagementObject systemDevice = GetSystemDevice(deviceClassName, deviceObjectElementName, hostName, scope);
            return systemDevice;
        }

        public static ManagementObject GetSystemDevice(
            string deviceClassName,
            string deviceObjectElementName,
            string virtualMachineName,
            ManagementScope scope)
        {
            ManagementObject systemDevice = null;
            ManagementObject computerSystem = Utility.GetTargetComputer(virtualMachineName, scope);

            ManagementObjectCollection systemDevices = computerSystem.GetRelated(
                deviceClassName,
                "Msvm_SystemDevice",
                null,
                null,
                "PartComponent",
                "GroupComponent",
                false,
                null);

            foreach (ManagementObject device in systemDevices)
            {
                if (device["ElementName"].ToString().ToLower() == deviceObjectElementName.ToLower())
                {
                    systemDevice = device;
                    break;
                }
            }

            return systemDevice;
        }

        public static bool JobCompleted(ManagementBaseObject outParams, ManagementScope scope)
        {
            bool jobCompleted = true;

            // Retrieve msvc_StorageJob path. This is a full wmi path
            string jobPath = (string)outParams["Job"];

            ManagementObject job = new ManagementObject(scope, new ManagementPath(jobPath), null);

            // Try to get storage job information
            job.Get();
            while ((ushort)job["JobState"] == JobState.Starting
                || (ushort)job["JobState"] == JobState.Running)
            {
                Console.WriteLine("In progress... {0}% completed.", job["PercentComplete"]);
                System.Threading.Thread.Sleep(1000);
                job.Get();
            }

            // Figure out if job failed
            ushort jobState = (ushort)job["JobState"];
            if (jobState != JobState.Completed)
            {
                ushort jobErrorCode = (ushort)job["ErrorCode"];
                Console.WriteLine("Error Code:{0}", jobErrorCode);
                Console.WriteLine("ErrorDescription: {0}", (string)job["ErrorDescription"]);
                jobCompleted = false;
            }

            return jobCompleted;
        }

        public static ManagementObject GetTargetComputer(string virtualMachineElementName, ManagementScope scope)
        {
            string query = string.Format(CultureInfo.InvariantCulture, "select * from Msvm_ComputerSystem Where ElementName = '{0}'", virtualMachineElementName);

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));

            ManagementObjectCollection computers = searcher.Get();

            ManagementObject computer = null;

            foreach (ManagementObject instance in computers)
            {
                computer = instance;
                break;
            }

            return computer;
        }

        public static ManagementObject GetVirtualSystemSettingData(ManagementObject vm)
        {
            ManagementObject virtualMachineSetting = null;
            ManagementObjectCollection virtualMachineSettings = vm.GetRelated(
                "Msvm_VirtualSystemSettingData",
                "Msvm_SettingsDefineState",
                null,
                null,
                "SettingData",
                "ManagedElement",
                false,
                null);

            if (virtualMachineSettings.Count != 1)
            {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0} instance of Msvm_VirtualSystemSettingData was found", virtualMachineSettings.Count));
            }

            foreach (ManagementObject instance in virtualMachineSettings)
            {
                virtualMachineSetting = instance;
                break;
            }

            return virtualMachineSetting;
        }

        // Get RASD definitions
        public static ManagementObject GetResourceAllocationsettingDataDefault(
            ManagementScope scope,
            ushort resourceType,
            string resourceSubType,
            string otherResourceType)
        {
            ManagementObject managementObjectRASD = null;

            string query = string.Format(
                CultureInfo.InvariantCulture,
                "select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType ='{1}' and OtherResourceType = '{2}'",
                resourceType,
                resourceSubType,
                otherResourceType);

            if (resourceType == ResourceType.Other)
            {
                query = string.Format(
                    CultureInfo.InvariantCulture,
                    "select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType = null and OtherResourceType = {1}",
                    resourceType,
                    otherResourceType);
            }
            else
            {
                query = string.Format(
                    CultureInfo.InvariantCulture,
                    "select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType ='{1}' and OtherResourceType = null",
                    resourceType,
                    resourceSubType);
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));

            ManagementObjectCollection poolResources = searcher.Get();

            // Get pool resource allocation ability
            if (poolResources.Count == 1)
            {
                foreach (ManagementObject poolResource in poolResources)
                {
                    ManagementObjectCollection allocationCapabilities = poolResource.GetRelated("Msvm_AllocationCapabilities");
                    foreach (ManagementObject allocationCapability in allocationCapabilities)
                    {
                        ManagementObjectCollection settingDatas = allocationCapability.GetRelationships("Msvm_SettingsDefineCapabilities");
                        foreach (ManagementObject settingData in settingDatas)
                        {
                            if (Convert.ToInt16(settingData["ValueRole"], CultureInfo.InvariantCulture) == (ushort)ValueRole.Default)
                            {
                                managementObjectRASD = new ManagementObject(settingData["PartComponent"].ToString());
                                break;
                            }
                        }
                    }
                }
            }

            return managementObjectRASD;
        }

        public static ManagementObject GetResourceAllocationsettingData(
            ManagementObject vm,
            ushort resourceType,
            string resourceSubType,
            string otherResourceType)
        {
            // vm->vmsettings->RASD for IDE controller
            ManagementObject managementObjectRASD = null;
            ManagementObjectCollection settingDatas = vm.GetRelated("Msvm_VirtualSystemsettingData");
            foreach (ManagementObject settingData in settingDatas)
            {
                // retrieve the rasd
                ManagementObjectCollection collectionOfRASDs = settingData.GetRelated("Msvm_ResourceAllocationsettingData");
                foreach (ManagementObject rasdInstance in collectionOfRASDs)
                {
                    if (Convert.ToInt16(rasdInstance["ResourceType"], CultureInfo.InvariantCulture) == resourceType)
                    {
                        // found the matching type
                        if (resourceType == ResourceType.Other)
                        {
                            if (rasdInstance["OtherResourceType"].ToString() == otherResourceType)
                            {
                                managementObjectRASD = rasdInstance;
                                break;
                            }
                        }
                        else
                        {
                            if (rasdInstance["ResourceSubType"].ToString() == resourceSubType)
                            {
                                managementObjectRASD = rasdInstance;
                                break;
                            }
                        }
                    }
                }
            }

            return managementObjectRASD;
        }

        private static class JobState
        {
            public const ushort New = 2;
            public const ushort Starting = 3;
            public const ushort Running = 4;
            public const ushort Suspended = 5;
            public const ushort ShuttingDown = 6;
            public const ushort Completed = 7;
            public const ushort Terminated = 8;
            public const ushort Killed = 9;
            public const ushort Exception = 10;
            public const ushort Service = 11;
        }
    }
}
