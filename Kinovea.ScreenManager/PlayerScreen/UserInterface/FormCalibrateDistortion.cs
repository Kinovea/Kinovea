using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibrateDistortion : Form
    {
        private CameraCalibrator calibrator;
        private CalibrationHelper calibrationHelper;

        public FormCalibrateDistortion(List<List<PointF>> points, CalibrationHelper calibrationHelper)
        {
            this.calibrationHelper = calibrationHelper;
            calibrator = new CameraCalibrator(points, calibrationHelper.ImageSize);
            
            InitializeComponent();

            Populate();
            
        }

        private void Populate()
        {
            if (calibrationHelper.DistortionHelper.Initialized)
            {
                DistortionParameters p = calibrationHelper.DistortionHelper.Parameters;
                lblK1.Text = string.Format("k1 : {0:0.000}", p.K1);
                lblK2.Text = string.Format("k2 : {0:0.000}", p.K2);
                lblK3.Text = string.Format("k3 : {0:0.000}", p.K3);
                lblP1.Text = string.Format("p1 : {0:0.000}", p.P1);
                lblP2.Text = string.Format("p2 : {0:0.000}", p.P2);

                lblFx.Text = string.Format("fx : {0:0.000}", p.Fx);
                lblFy.Text = string.Format("fy : {0:0.000}", p.Fy);
                lblCx.Text = string.Format("cx : {0:0.000}", p.Cx);
                lblCy.Text = string.Format("cy : {0:0.000}", p.Cy);

                // get image.
                //Bitmap bmp = calibrationHelper.DistortionHelper.GetDistortionGrid();
            }
            else
            {
                lblK1.Text = string.Format("k1 : {0:0.000}", 0);
                lblK2.Text = string.Format("k2 : {0:0.000}", 0);
                lblK3.Text = string.Format("k3 : {0:0.000}", 0);
                lblP1.Text = string.Format("p1 : {0:0.000}", 0);
                lblP2.Text = string.Format("p2 : {0:0.000}", 0);

                lblFx.Text = string.Format("fx : {0:0.000}", 1);
                lblFy.Text = string.Format("fy : {0:0.000}", 1);
                lblCx.Text = string.Format("cx : {0:0.000}", 0);
                lblCy.Text = string.Format("cy : {0:0.000}", 0);
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!calibrator.Valid)
                return;

            DistortionParameters parameters = calibrator.Calibrate();
            calibrationHelper.DistortionHelper.Initialize(parameters, calibrationHelper.ImageSize);
        }
    }
}
