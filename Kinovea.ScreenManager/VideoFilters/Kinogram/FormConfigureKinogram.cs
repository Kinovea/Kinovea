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
    /// Note: this dialog needs to update the filter in real time when changing params.
    /// OK/Cancel mechanics: 
    /// - keep a backup of the original state and directly modify the object.
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
        private StyleData styleHelper = new StyleData();
        private StyleElements style;
        private IDrawingHostView hostView;
        #endregion

        public FormConfigureKinogram(VideoFilterKinogram kinogram, IDrawingHostView hostView)
        {
            this.kinogram = kinogram;
            this.hostView = hostView;

            memento = new HistoryMementoModifyVideoFilter(kinogram.ParentMetadata, VideoFilterType.Kinogram, kinogram.FriendlyNameResource);
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
            this.Text = ScreenManagerLang.formConfigureKinogram_Title;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblColumns.Text = ScreenManagerLang.formConfigureKinogram_Table;
            lblCropSize.Text = ScreenManagerLang.formConfigureKinogram_CropSize;
            btnApply.Text = ScreenManagerLang.Generic_Apply;
            btnOK.Text = ScreenManagerLang.Generic_OK;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void SetupStyle()
        {
            style = new StyleElements();
            style.Elements.Add("borderColor", new StyleElementColor(parameters.BorderColor, ScreenManagerLang.StyleElement_Color_BorderColor));
            style.Elements.Add("labelColor", new StyleElementColor(parameters.LabelColor, ScreenManagerLang.StyleElement_Color_LabelColor));
            style.Elements.Add("labelSize", new StyleElementFontSize(parameters.LabelSize, ScreenManagerLang.StyleElement_FontSize_LabelSize));

            styleHelper.Color = Color.Red;
            styleHelper.BackgroundColor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", 16, FontStyle.Bold);

            style.Bind(styleHelper, "Color", "borderColor");
            style.Bind(styleHelper, "Bicolor", "labelColor");
            style.Bind(styleHelper, "Font", "labelSize");
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
                styleElement.ValueChanged += styleElement_ValueChanged;

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

        private void styleElement_ValueChanged(object sender, EventArgs e)
        {
            UpdateKinogram();
        }

        private void UpdateFrameInterval()
        {
            int tileCount = kinogram.GetTileCount(parameters.TileCount);
            lblTotal.Text = string.Format(ScreenManagerLang.formConfigureKinogram_lblTotal, tileCount);
            float interval = kinogram.GetFrameInterval(parameters.TileCount);
            lblFrameInterval.Text = string.Format(ScreenManagerLang.formConfigureKinogram_lblFrameInterval, interval * 1000.0f);
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
            parameters.LabelColor = styleHelper.GetBackgroundColor();
            parameters.LabelSize = (int)styleHelper.Font.Size;
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
            parameters.LabelColor = styleHelper.GetBackgroundColor();
            parameters.LabelSize = (int)styleHelper.Font.Size;

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
