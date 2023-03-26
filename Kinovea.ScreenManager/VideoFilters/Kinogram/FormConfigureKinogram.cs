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
    /// OK/Cancel mechanics: we keep a backup of the original state and directly modify the object.
    /// - In case of cancel or close we perform an undo manually from here.
    /// - In case of OK we commit the original state to the undo stack.
    /// </summary>
    public partial class FormConfigureKinogram : Form
    {
        #region Members
        private VideoFilterKinogram kinogram;
        private HistoryMementoModifyVideoFilter memento;
        private KinogramParameters parameters;
        private bool manualUpdate;
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private IDrawingHostView hostView;
        #endregion

        public FormConfigureKinogram(VideoFilterKinogram kinogram, IDrawingHostView hostView)
        {
            this.kinogram = kinogram;
            this.hostView = hostView;

            memento = new HistoryMementoModifyVideoFilter(kinogram.ParentMetadata, VideoFilterType.Kinogram, kinogram.FriendlyName); ;
            this.parameters = kinogram.Parameters;

            InitializeComponent();
            SetupStyle();
            SetupStyleControls();
            InitValues();
            InitCulture();
            UpdateFrameInterval();
            FixNudScroll();
        }

        private void InitValues()
        {
            manualUpdate = true;
            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            nudCols.Value = cols;
            nudRows.Value = parameters.Rows;
            nudCropWidth.Value = parameters.CropSize.Width;
            nudCropHeight.Value = parameters.CropSize.Height;
            manualUpdate = false;
        }

        private void InitCulture()
        {
            this.Text = "Configure Kinogram";
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblColumns.Text = "Table:";
            lblCropSize.Text = "Crop size:";
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
            int btnLeft = lblColumns.Left;
            int editorsLeft = nudCropWidth.Left;
            int lastEditorBottom = lblCropSize.Bottom;
            Size editorSize = new Size(60, 20);

            foreach (KeyValuePair<string, AbstractStyleElement> pair in style.Elements)
            {
                AbstractStyleElement styleElement = pair.Value;

                Button btn = new Button();
                btn.Image = styleElement.Icon;
                btn.Size = new Size(20, 20);
                btn.Location = new Point(btnLeft, lastEditorBottom + 20);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;

                Label lbl = new Label();
                lbl.Text = styleElement.DisplayName;
                lbl.AutoSize = true;
                lbl.Location = new Point(btn.Right + 10, lastEditorBottom + 25);

                Control miniEditor = styleElement.GetEditor();
                miniEditor.Size = editorSize;
                miniEditor.Location = new Point(editorsLeft, btn.Top);

                lastEditorBottom = miniEditor.Bottom;

                grpConfig.Controls.Add(btn);
                grpConfig.Controls.Add(lbl);
                grpConfig.Controls.Add(miniEditor);
            }
        }

        private void UpdateFrameInterval()
        {
            int tileCount = kinogram.GetTileCount(parameters.TileCount);
            lblTotal.Text = string.Format("Frames: {0}", tileCount);
            float interval = kinogram.GetFrameInterval(parameters.TileCount);
            lblFrameInterval.Text = string.Format("Frame interval: {0:0.000} ms", interval * 1000.0f);
        }

        private void FixNudScroll()
        {
            NudHelper.FixNudScroll(nudCols);
            NudHelper.FixNudScroll(nudRows);
            NudHelper.FixNudScroll(nudCropWidth);
            NudHelper.FixNudScroll(nudCropHeight);
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
            UpdateKinogram();
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

        private void btnApply_Click(object sender, EventArgs e)
        {
            UpdateKinogram();
        }

        private void UpdateKinogram()
        {
            parameters.BorderColor = styleHelper.Color;
            kinogram.ConfigurationChanged(true);
            hostView?.InvalidateFromMenu();
        }
        #endregion

        #region OK/Cancel/Close
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Import style values and commit the parameters object.
            parameters.BorderColor = styleHelper.Color;

            // Commit the original state to the undo history stack.
            kinogram.ParentMetadata.HistoryStack.PushNewCommand(memento);
        }

        private void Cancel()
        {
            memento.PerformUndo();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                return;

            memento.PerformUndo();
        }
        #endregion
    }
}
