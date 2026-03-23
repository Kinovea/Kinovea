using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ExpTreeLib2
{
    public enum StartDir : int
    {
        Desktop             = 0x0,
        Programs            = 0x2,
        Controls            = 0x3,
        Printers            = 0x4,
        Personal            = 0x5,
        Favorites           = 0x6,
        Startup             = 0x7,
        Recent              = 0x8,
        SendTo              = 0x9,
        StartMenu           = 0xB,
        MyDocuments         = 0xC,
        // MyMusic          = 0xD,
        // MyVideo          = 0xE,
        DesktopDirectory    = 0x10,
        MyComputer          = 0x11,
        My_Network_Places   = 0x12,
        // NETHOOD          = 0x13,
        // FONTS            = 0x14,
        ApplicatationData   = 0x1A,
        // PRINTHOOD        = 0x1B,
        Internet_Cache      = 0x20,
        Cookies             = 0x21,
        History             = 0x22,
        Windows             = 0x24,
        System              = 0x25,
        Program_Files       = 0x26,
        MyPictures          = 0x27,
        Profile             = 0x28,
        Systemx86           = 0x29,
        AdminTools          = 0x30,
        Special             = 0xFF
    }
}
