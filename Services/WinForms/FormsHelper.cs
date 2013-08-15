using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Kinovea.Services
{
    public static class FormsHelper
    {
        private static Form mainForm;

        public static void SetMainForm(Form form)
        {
            mainForm = form;
        }

        public static void MakeTopmost(Form form)
        {
            if (mainForm == null)
                throw new Exception("Forms helper not initialized.");

            form.Owner = mainForm;
        }

        public static void BeforeShow()
        {
            NotificationCenter.RaiseDisableKeyboardHandler(null);
        }

        public static void AfterShow()
        {
            NotificationCenter.RaiseEnableKeyboardHandler(null);
        }

        /// <summary>
        /// Locate the form under the mouse or center of screen if too close to border.
        /// </summary>
        /// <param name="_form"></param>
        public static void Locate(Form form)
        {
            if (Cursor.Position.X + (form.Width / 2) >= SystemInformation.PrimaryMonitorSize.Width ||
                Cursor.Position.Y + form.Height >= SystemInformation.PrimaryMonitorSize.Height)
                form.StartPosition = FormStartPosition.CenterScreen;
            else
                form.Location = new Point(Cursor.Position.X - (form.Width / 2), Cursor.Position.Y - 20);
        }
    }
}
