using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Configuration dialog for LensCalibration.
    /// Note: this dialog doesn't need to update the filter in real time.
    /// OK/Cancel mechanics: we keep a backup of the original params and directly modify them here.
    /// - In case of cancel or close we restore from the memento.
    /// - In case of OK we don't have anything more to do.
    /// </summary>
    public partial class FormConfigureLensCalibration : Form
    {
        #region Members
        private VideoFilterLensCalibration lensCalibration;
        private LensCalibrationParameters memento;
        private LensCalibrationParameters parameters;
        private bool manualUpdate;
        #endregion

        public FormConfigureLensCalibration(VideoFilterLensCalibration lensCalibration)
        {
            this.lensCalibration = lensCalibration;
            memento = lensCalibration.Parameters.Clone();
            this.parameters = lensCalibration.Parameters;

            InitializeComponent();
            InitValues();
            InitCulture();
            FixNudScroll();
        }

        private void InitValues()
        {
            manualUpdate = true;
            nudMaxImages.Value = this.parameters.MaxImages;
            nudCols.Value = this.parameters.PatternSize.Width;
            nudRows.Value = this.parameters.PatternSize.Height;
            nudMaxIterations.Value = this.parameters.MaxIterations;
            manualUpdate = false;
        }

        private void InitCulture()
        {
            this.Text = ScreenManagerLang.FormConfigureLensCalibration_ConfigureLensCalibration;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblMaxImages.Text = ScreenManagerLang.FormConfigureLensCalibration_MaxImages;
            lblPatternSize.Text = ScreenManagerLang.FormConfigureLensCalibration_PatternSize;
            lblMaxIterations.Text = ScreenManagerLang.FormConfigureLensCalibration_MaxIterations;
            btnOK.Text = ScreenManagerLang.Generic_OK;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void FixNudScroll()
        {
            NudHelper.FixNudScroll(nudMaxImages);
            NudHelper.FixNudScroll(nudCols);
            NudHelper.FixNudScroll(nudRows);
            NudHelper.FixNudScroll(nudMaxIterations);
        }

        #region Event handlers
        private void patternSize_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int cols = (int)nudCols.Value;
            int rows = (int)nudRows.Value;
            parameters.PatternSize = new Size(cols, rows);
        }

        private void patternSize_KeyUp(object sender, KeyEventArgs e)
        {
            patternSize_ValueChanged(sender, EventArgs.Empty);
        }

        private void maxImages_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            parameters.MaxImages = (int)nudMaxImages.Value;
        }

        private void maxImages_KeyUp(object sender, KeyEventArgs e)
        {
            maxImages_ValueChanged(sender, EventArgs.Empty);
        }

        private void maxIterations_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            parameters.MaxIterations = (int)nudMaxIterations.Value;
        }

        private void maxIterations_KeyUp(object sender, KeyEventArgs e)
        {
            maxIterations_ValueChanged(sender, EventArgs.Empty);
        }

        #endregion

        #region OK/Cancel/Close
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
        }

        private void Cancel()
        {
            lensCalibration.Parameters = memento;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                return;

            lensCalibration.Parameters = memento;
        }
        #endregion
    }
}
