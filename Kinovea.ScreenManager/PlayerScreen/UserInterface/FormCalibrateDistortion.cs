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

        public FormCalibrateDistortion(List<List<PointF>> points, Size imageSize)
        {
            calibrator = new CameraCalibrator(points, imageSize);

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!calibrator.Valid)
                return;

            calibrator.Calibrate();
        }
    }
}
