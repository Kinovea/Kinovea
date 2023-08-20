using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public partial class FormConfigureExportImageSideBySide : Form
    {
        /// <summary>
        /// Whether we should composite images horizontally or vertically.
        /// </summary>
        public bool Horizontal
        {
            get { return rbHorizontal.Checked; }
        }

        public FormConfigureExportImageSideBySide()
        {
            InitializeComponent();
            InitializeCulture();
            Populate();
        }

        private void InitializeCulture()
        {
            this.Text = "Export side by side";

            rbHorizontal.Text = "Horizontal";
            rbVertical.Text = "Vertical";

            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void Populate()
        {
            // Get the default orientation from preferences.
            bool horizontal = PreferencesManager.PlayerPreferences.SideBySideHorizontal;
            rbHorizontal.Checked = horizontal;
            rbVertical.Checked = !horizontal;
        }
    }
}
