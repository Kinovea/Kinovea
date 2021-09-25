using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class FormConfigureKinogram : Form
    {

        public bool GridChanged { get; set; }

        private VideoFilterKinogram kinogram;
        private KinogramParameters parameters;
        private bool manualUpdate;

        public FormConfigureKinogram(VideoFilterKinogram kinogram, KinogramParameters parameters)
        {
            this.kinogram = kinogram;
            this.parameters = parameters;

            InitializeComponent();
            InitializeValues();
        }

        private void InitializeValues()
        {
            manualUpdate = true;
            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            nudCols.Value = cols;
            nudRows.Value = parameters.Rows;
            nudCropWidth.Value = parameters.CropSize.Width;
            nudCropHeight.Value = parameters.CropSize.Height;
            cbRTL.Checked = !parameters.LeftToRight;
            manualUpdate = false;
        }

        private void grid_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int cols = (int)nudCols.Value;
            int rows = (int)nudRows.Value;
            parameters.TileCount = cols * rows;
            parameters.Rows = rows;
            GridChanged = true;
        }

        private void cropSize_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width = (int)nudCropWidth.Value;
            int height = (int)nudCropHeight.Value;
            parameters.CropSize = new Size(width, height);
        }

        private void cbRTL_CheckedChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            bool rtl = cbRTL.Checked;
            parameters.LeftToRight = !rtl;
        }
    }
}
