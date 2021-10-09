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
    /// Configuration dialog for Kinogram.
    /// OK/Cancel mechanics: we work with a copy of the parameters object.
    /// In case of cancel or close we simply don't inject it back in the Kinogram.
    /// </summary>
    public partial class FormConfigureKinogram : Form
    {
        
        public KinogramParameters Parameters 
        { 
            get{ return parameters; } 
        }

        #region Members
        private VideoFilterKinogram kinogram;
        private KinogramParameters parameters;
        private bool manualUpdate;
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        #endregion

        public FormConfigureKinogram(VideoFilterKinogram kinogram)
        {
            InitializeComponent();

            this.kinogram = kinogram;
            this.parameters = kinogram.Parameters.Clone();

            SetupStyle();
            SetupStyleControls();
            InitValues();
            InitCulture();
            UpdateFrameInterval();
        }

        private void InitValues()
        {
            manualUpdate = true;
            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            nudCols.Value = cols;
            nudRows.Value = parameters.Rows;
            nudCropWidth.Value = parameters.CropSize.Width;
            nudCropHeight.Value = parameters.CropSize.Height;
            cbRTL.Checked = !parameters.LeftToRight;
            cbBorderVisible.Checked = parameters.BorderVisible;
            manualUpdate = false;
        }

        private void InitCulture()
        {
            this.Text = "Configure Kinogram";
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblColumns.Text = "Columns:";
            lblRows.Text = "Rows:";
            lblCropSize.Text = "Crop size:";
            cbRTL.Text = "Right to left";
            
            grpAppearance.Text = ScreenManagerLang.Generic_Appearance;
            cbBorderVisible.Text = "Show border";
        }

        private void SetupStyle()
        {
            style = new DrawingStyle();
            style.Elements.Add("border color", new StyleElementColor(parameters.BorderColor));

            styleHelper.Color = Color.Red;
            style.Bind(styleHelper, "Color", "border color");
        }

        private void SetupStyleControls()
        {
            int btnLeft = cbBorderVisible.Left;
            int editorsLeft = 200;
            int lastEditorBottom = cbBorderVisible.Bottom;
            Size editorSize = new Size(60, 20);

            foreach (KeyValuePair<string, AbstractStyleElement> pair in style.Elements)
            {
                AbstractStyleElement styleElement = pair.Value;

                Button btn = new Button();
                btn.Image = styleElement.Icon;
                btn.Size = new Size(20, 20);
                btn.Location = new Point(btnLeft, lastEditorBottom + 15);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;

                Label lbl = new Label();
                lbl.Text = styleElement.DisplayName;
                lbl.AutoSize = true;
                lbl.Location = new Point(btn.Right + 10, lastEditorBottom + 20);

                Control miniEditor = styleElement.GetEditor();
                miniEditor.Size = editorSize;
                miniEditor.Location = new Point(editorsLeft, btn.Top);

                lastEditorBottom = miniEditor.Bottom;

                grpAppearance.Controls.Add(btn);
                grpAppearance.Controls.Add(lbl);
                grpAppearance.Controls.Add(miniEditor);
            }
        }

        private void UpdateFrameInterval()
        {
            int tileCount = kinogram.GetTileCount(parameters.TileCount);
            lblTotal.Text = string.Format("Total: {0}", tileCount);
            float interval = kinogram.GetFrameInterval(parameters.TileCount);
            lblFrameInterval.Text = string.Format("Frame interval: {0:0.000} ms", interval * 1000.0f);
        }

        #region Event handlers
        private void grid_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int cols = (int)nudCols.Value;
            int rows = (int)nudRows.Value;
            parameters.TileCount = cols * rows;
            parameters.Rows = rows;

            UpdateFrameInterval();
        }

        private void grid_KeyUp(object sender, KeyEventArgs e)
        {
            grid_ValueChanged(sender, EventArgs.Empty);
        }

        private void cropSize_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width = (int)nudCropWidth.Value;
            int height = (int)nudCropHeight.Value;
            parameters.CropSize = new Size(width, height);
        }

        private void cropSize_KeyUp(object sender, KeyEventArgs e)
        {
            cropSize_ValueChanged(sender, EventArgs.Empty);
        }

        private void cbRTL_CheckedChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            bool rtl = cbRTL.Checked;
            parameters.LeftToRight = !rtl;
        }

        private void cbBorderVisible_CheckedChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            parameters.BorderVisible = cbBorderVisible.Checked;
        }
        #endregion

        #region OK/Cancel/Close
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Import style values and commit the parameters object.
            parameters.BorderColor = styleHelper.Color;
        }
        #endregion
    }
}
