using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.Drawing.Drawing2D;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class FormVDM : Form
    {
        private CalibrationHelper calibrationHelper;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormVDM(CalibrationHelper calibrationHelper)
        {
            this.calibrationHelper = calibrationHelper;
            InitializeComponent();
            LocalizeForm();

            ComputeCameraPosition();
        }

        private void LocalizeForm()
        {
            this.Text = "Video distance measurement";
            
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            grpIntrinsics.Text = "Camera position";

            bool hasLensCalibration = calibrationHelper.DistortionHelper != null && calibrationHelper.DistortionHelper.Initialized;
            bool hasPlaneCalibration = calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Plane;
            bool ready = hasLensCalibration && hasPlaneCalibration;

            string lensCalibrationStatus = hasLensCalibration ? "found" : "missing";
            string planeCalibrationStatus = hasPlaneCalibration ? "found" : "missing";

            lblSensorWidth.Text = string.Format("Lens calibration: {0}", lensCalibrationStatus);
            lblFocalLength.Text = string.Format("Plane calibration: {0}", planeCalibrationStatus);

            lblSensorWidth.ForeColor = hasLensCalibration ? Color.Green : Color.Red;
            lblFocalLength.ForeColor = hasPlaneCalibration ? Color.Green : Color.Red;

            label1.Enabled = ready;
            label1.Text = "Camera position";
        }

        private void ComputeCameraPosition()
        {
            // Call camera solver with distortion and plane homography matrix.
            // Get camera position.

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
        }
    }
}
