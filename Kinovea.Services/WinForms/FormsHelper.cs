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

        /// <summary>
        /// Make the given form a child of the main Kinovea window.
        /// </summary>
        public static void MakeTopmost(Form form)
        {
            if (mainForm == null)
                throw new Exception("Forms helper not initialized.");

            form.Owner = mainForm;
        }

        /// <summary>
        /// Locate the form under the mouse or at center of screen if too close to border.
        /// </summary>
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
