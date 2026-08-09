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
    public partial class FormConfigureExportVideoSideBySide : Form
    {
        /// <summary>
        /// Whether we should composite videos horizontally or vertically.
        /// </summary>
        public bool Horizontal
        {
            get { return rbHorizontal.Checked; }
        }

        /// <summary>
        /// Export profile with encoding options, only used for video export.
        /// </summary>
        public ExportProfile ExportProfile
        {
            get
            {
                return encodingSettings.GetExportProfile();
            }
        }


        public FormConfigureExportVideoSideBySide(string filename)
        {
            InitializeComponent();
            InitializeCulture();
            Populate(filename);
        }

        private void InitializeCulture()
        {
            this.Text = ScreenManagerLang.formConfigureExport_SBS;

            rbHorizontal.Text = ScreenManagerLang.formConfigureExport_SBS_Horizontal;
            rbVertical.Text = ScreenManagerLang.formConfigureExport_SBS_Vertical;

            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void Populate(string filename)
        {
            // Get the default orientation from preferences.
            bool horizontal = PreferencesManager.PlayerPreferences.SideBySideHorizontal;
            rbHorizontal.Checked = horizontal;
            rbVertical.Checked = !horizontal;

            ExportProfile exportProfile = PreferencesManager.PlayerPreferences.ExportProfile;
            encodingSettings.FillValues(exportProfile, filename);
        }
    }
}
