using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class TrackColorCycler
    {
        private static List<Color> colors = new List<Color>() 
        { 
            Color.CornflowerBlue,
            Color.Firebrick,
            Color.DarkMagenta,
            Color.Gold,
            Color.MidnightBlue,
            Color.DarkOrange,
            Color.SeaGreen,
            Color.DeepPink,
            Color.Teal,
            Color.White
        };

        private static int index = -1;

        public static Color Next()
        {
            index = (index + 1) % colors.Count;
            return colors[index];
        }
    }
}
