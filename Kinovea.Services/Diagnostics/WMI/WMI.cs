using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace Kinovea.Services
{
    public static class WMI
    {
        private static UInt64 gigabyte = 1024 * 1024 * 1024;

        public static List<string> Processor_MaxClockSpeed()
        {
            return PerformQuery("maxclockspeed", "Win32_Processor");
        }

        public static List<string> Processor_Name()
        {
            return PerformQuery("name", "Win32_Processor");
        }

        public static float PhysicalMemory_Capacity()
        {
            string query = "SELECT Capacity FROM Win32_PhysicalMemory";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();

            UInt64 total = 0;
            foreach (ManagementObject result in results)
            {
                total += (UInt64)result["Capacity"];
            }

            float capacity = (float)((double)total / gigabyte);
            return capacity;
        }

        public static List<LogicalDisk> LogicalDisks()
        {
            string query = "SELECT Caption, DriveType, FileSystem, FreeSpace, Size FROM Win32_LogicalDisk";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();

            List<LogicalDisk> disks = new List<LogicalDisk>();

            foreach (ManagementObject result in results)
            {
                string caption = result["Caption"].ToString();
                DriveType type = ConvertDriveType((UInt32)result["DriveType"]);
                string filesystem = "";
                if (result["FileSystem"] != null)
                    filesystem = result["FileSystem"].ToString();

                UInt64 freespace = 0;
                if (result["FreeSpace"] != null)
                    freespace = (UInt64)result["FreeSpace"];
                
                UInt64 size = 0;
                if (result["Size"] != null)
                    size = (UInt64)result["Size"];

                float freespaceGB = (float)((double)freespace / gigabyte);
                float sizeGB = (float)((double)size / gigabyte);

                disks.Add(new LogicalDisk(caption, type, filesystem, freespaceGB, sizeGB));
            }

            return disks;
        }

        private static DriveType ConvertDriveType(UInt32 driveType)
        {
            DriveType type = DriveType.Other;
            switch (driveType)
            {
                case 0:
                case 1:
                case 4:
                case 5:
                default:
                    type = DriveType.Other;
                    break;
                case 2:
                    type = DriveType.Removable;
                    break;
                case 3:
                    type = DriveType.Local;
                    break;
                case 6:
                    type = DriveType.RAM;
                    break;
            }

            return type;
        }

        public static List<PhysicalDisk> PhysicalDisks()
        {
            string query = "SELECT Model, MediaType, InterfaceType, Size FROM Win32_DiskDrive";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();

            List<PhysicalDisk> disks = new List<PhysicalDisk>();

            foreach (ManagementObject result in results)
            {
                string model = result["Model"].ToString();
                string type = result["MediaType"].ToString();
                string interfaceType = result["InterfaceType"].ToString();
                UInt64 size = (UInt64)result["Size"];

                float sizeGB = (float)((double)size / gigabyte);

                disks.Add(new PhysicalDisk(model, type, interfaceType, sizeGB));
            }

            return disks;
        }

        private static List<string> PerformQuery(string field, string table)
        {
            string query = string.Format("SELECT {0} FROM {1}", field, table);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection objCollection = searcher.Get();

            List<string> result = new List<string>();
            foreach (ManagementObject o in objCollection)
                result.Add(o[field].ToString());

            return result;
        }

        public static void Test()
        {
            string query = "SELECT * FROM Win32_LogicalDisk";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();

            //List<string> properties = new List<string>() { "MediaType", "Model", "InterfaceType", "Size", "Partitions" };
            List<string> properties = new List<string>() { "Caption", "Compressed", "Description", "DriveType", "FileSystem", "FreeSpace", "MediaType", "Name", "Size", "Status", "SystemName", "VolumeName" };

            foreach (ManagementObject result in results)
            {
                foreach(string property in properties)
                {
                    Console.WriteLine(result[property].ToString());
                }
            }
        }
    }
}
