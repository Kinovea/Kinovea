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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Dialog to configure foreground color.
    /// The foreground color controls the solid colored rectangle placed 
    /// between the video images and drawings.
    /// 
    /// Workflow: we update the actual value stored in the Metadata object and cause invalidation on any change, 
    /// to provide immediate feedback. The caller must backup the original value before starting this dialog, 
    /// and restore it if the dialog result is cancel.
    /// </summary>
    public partial class FormForegroundColor : Form
    {
        private Control surfaceScreen;
        private Metadata metadata;

        public FormForegroundColor(Metadata metadata, Control screen)
        {
            this.metadata = metadata;
            this.surfaceScreen = screen;

            InitializeComponent();
            Populate();
        }

        private void Populate()
        {
            this.Text = "Configure opacity and background color";
            lblOpaque.Text = "Opacity";

            int a = metadata.ForegroundColor.A;
            Color rgb = Color.FromArgb(metadata.ForegroundColor.R, metadata.ForegroundColor.G, metadata.ForegroundColor.B);

            int opacityPercentage = (int)((1.0f - (a / 255.0f)) * 100.0f);
            nudOpaque.Value = (decimal)opacityPercentage;

            Control colorEditor = new Control();
            colorEditor.Top = lblColor.Top;
            colorEditor.Left = nudOpaque.Left;
            colorEditor.Size = nudOpaque.Size;
            colorEditor.Click += new EventHandler(colorEditor_Click);
            colorEditor.Paint += new PaintEventHandler(colorEditor_Paint);
            grpConfig.Controls.Add(colorEditor);
        }

        private void nudOpaque_ValueChanged(object sender, EventArgs e)
        {
            int opacity = (int)nudOpaque.Value;
            int a = (int)((1.0f - (opacity / 100.0f)) * 255.0f);

            Color rgb = Color.FromArgb(metadata.ForegroundColor.R, metadata.ForegroundColor.G, metadata.ForegroundColor.B);
            metadata.ForegroundColor = Color.FromArgb(a, rgb);

            surfaceScreen.Invalidate();
        }

        private void colorEditor_Paint(object sender, PaintEventArgs e)
        {
            Color rgb = Color.FromArgb(metadata.ForegroundColor.R, metadata.ForegroundColor.G, metadata.ForegroundColor.B);
            using (SolidBrush b = new SolidBrush(rgb))
            {
                e.Graphics.FillRectangle(b, e.ClipRectangle);
                e.Graphics.DrawRectangle(Pens.LightGray, e.ClipRectangle.Left, e.ClipRectangle.Top, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
            }
        }
        private void colorEditor_Click(object sender, EventArgs e)
        {
            int a = metadata.ForegroundColor.A;
            Color rgb = Color.FromArgb(metadata.ForegroundColor.R, metadata.ForegroundColor.G, metadata.ForegroundColor.B);
            
            FormColorPicker picker = new FormColorPicker(rgb);
            FormsHelper.Locate(picker);
            if (picker.ShowDialog() == DialogResult.OK)
            {
                rgb = picker.PickedColor;
                ((Control)sender).Invalidate();
            }

            picker.Dispose();

            metadata.ForegroundColor = Color.FromArgb(a, rgb);
            surfaceScreen.Invalidate();
        }
    }
}
