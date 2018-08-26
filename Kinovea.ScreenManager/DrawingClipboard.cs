using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public static class DrawingClipboard
    {
        private static string content;
        private static PointF position;
        private static string name;

        public static bool HasContent
        {
            get { return !string.IsNullOrEmpty(content); }
        }

        public static string Content
        {
            get { return content; }
        }

        public static PointF Position
        {
            get { return position; }
        }

        public static string Name
        {
            get { return name; }
        }

        public static void Put(string _content, PointF _position, string _name)
        {
            content = _content;
            position = _position;
            name = _name;
        }
    }
}
