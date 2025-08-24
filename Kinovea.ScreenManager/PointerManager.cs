using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This manager is unique to both screens and only handles the 
    /// manipulation/pointing pointers, not drawing tools pointers.
    /// The screen specific manager (ScreenPointerManager) handles switching and tools pointers.
    /// </summary>
    public static class PointerManager
    {
        public static Cursor Cursor
        {
            get { return cursor; }
        }

        private static Dictionary<string, Bitmap> pointers = new Dictionary<string, Bitmap>();
        private static Cursor cursor;
        private static IntPtr iconHandle;

        public static void LoadPointers()
        {
            // Built-in pointers.
            pointers.Add("::default", Properties.Drawings.handopen24c);
            pointers.Add("::bigHand", Properties.Resources.big_hand_128);
            pointers.Add("::bigArrow", Properties.Resources.big_arrow_128);

            // Custom pointers.
            if (!Directory.Exists(Software.PointersDirectory))
                return;

            foreach (string file in Directory.GetFiles(Software.PointersDirectory))
            {
                if (Bitmap.FromFile(file) is Bitmap bitmap)
                    pointers.Add(Path.GetFileNameWithoutExtension(file), bitmap);
            }

            string pointerKey = PreferencesManager.GeneralPreferences.PointerKey;
            SetCursor(pointerKey, true);
        }
        
        public static void SetCursor(string key, bool init = false)
        {
            IntPtr previousIconHandle = iconHandle;
            var bitmap = pointers.ContainsKey(key) ? pointers[key] : pointers["::default"];
            
            iconHandle = bitmap.GetHicon();
            cursor = new Cursor(iconHandle);

            if (previousIconHandle != IntPtr.Zero)
                NativeMethods.DestroyIcon(previousIconHandle);

            if (!init)
                PreferencesManager.GeneralPreferences.PointerKey = key;
        }
    }
}
