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
    /// Configuration dialog for CameraMotion.
    /// Note: this dialog doesn't need to update the filter in real time.
    /// OK/Cancel mechanics: we keep a backup of the original params and directly modify them here.
    /// - In case of cancel or close we restore from the memento.
    /// - In case of OK we don't have anything more to do.
    /// </summary>
    public partial class FormConfigureCameraMotion : Form
    {
        #region Members
        private VideoFilterCameraMotion cameraMotion;
        private CameraMotionParameters memento;
        private CameraMotionParameters parameters;
        private bool manualUpdate;
        #endregion

        public FormConfigureCameraMotion(VideoFilterCameraMotion cameraMotion)
        {
            this.cameraMotion = cameraMotion;
            memento = cameraMotion.Parameters.Clone();
            this.parameters = cameraMotion.Parameters;

            InitializeComponent();
            InitValues();
            InitCulture();
            FixNudScroll();
        }

        private void InitValues()
        {
            manualUpdate = true;

            cmbFeatureType.Items.Add("ORB");
            cmbFeatureType.Items.Add("SIFT");
            int featureType = (int)parameters.FeatureType;
            cmbFeatureType.SelectedIndex = ((int)featureType < cmbFeatureType.Items.Count) ? (int)featureType : 0;

            nudFeaturesPerFrame.Value = parameters.FeaturesPerFrame;

            // We set the min of the nud to 1 for UX purposes because otherwise 
            // it makes it hard to write a new number from scratch as it always
            // force to that min value. We cap the min value later before using it.
            nudFeaturesPerFrame.Minimum = 1;
            
            manualUpdate = false;
        }

        private void InitCulture()
        {
            this.Text = ScreenManagerLang.FormConfigureCameraMotion_ConfigureCameraMotionEstimation;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblFeatureType.Text = ScreenManagerLang.FormConfigureCameraMotion_FeatureType;
            lblFeaturesPerFrame.Text = ScreenManagerLang.FormConfigureCameraMotion_FeaturesPerFrame;
            btnOK.Text = ScreenManagerLang.Generic_OK;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void FixNudScroll()
        {
            NudHelper.FixNudScroll(nudFeaturesPerFrame);
        }

        #region Event handlers
        private void featuresPerFrame_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            parameters.FeaturesPerFrame = (int)nudFeaturesPerFrame.Value;
        }
        private void featuresPerFrame_KeyUp(object sender, KeyEventArgs e)
        {
            featuresPerFrame_ValueChanged(sender, EventArgs.Empty);
        }
        private void cmbFeatureType_SelectedIndexChanged(object sender, EventArgs e)
        {
            parameters.FeatureType = (CameraMotionFeatureType)cmbFeatureType.SelectedIndex;
        }
        #endregion

        #region OK/Cancel/Close
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            parameters.FeaturesPerFrame = Math.Max(100, parameters.FeaturesPerFrame);
        }

        private void Cancel()
        {
            cameraMotion.Parameters = memento;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                return;

            cameraMotion.Parameters = memento;
        }
        #endregion
    }
}
