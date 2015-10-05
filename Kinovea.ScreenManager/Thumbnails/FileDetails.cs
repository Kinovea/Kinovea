using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class FileDetails
    {
        public Dictionary<FileProperty, string> Details { get; private set; }

        public FileDetails()
        {
            Details = new Dictionary<FileProperty, string>();
        }
    }
}
