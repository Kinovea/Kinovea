using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public class LogicalDisk
    {
        public string Caption { get; private set; }
        public DriveType DriveType { get; private set; }
        public string FileSystem { get; private set; }
        public float Free { get; private set; }
        public float Size { get; private set; }

        public LogicalDisk(string caption, DriveType type, string filesystem, float free, float size)
        {
            this.Caption = caption;
            this.DriveType = type;
            this.FileSystem = filesystem;
            this.Free = free;
            this.Size = size;
        }
    }
}
