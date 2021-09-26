using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
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
    /// <summary>
    /// Form to export the Kinogram to an image.
    /// </summary>
    public partial class FormExportKinogram : Form
    {

        private VideoFilterKinogram kinogram;
        private float aspect;
        private bool manualUpdate;
        private int width = 1920;
        private int height = 1080;
        private long timestamp;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public FormExportKinogram(VideoFilterKinogram kinogram, long timestamp)
        {
            InitializeComponent();

            this.kinogram = kinogram;
            this.aspect = kinogram.GetAspectRatio();
            this.timestamp = timestamp;
            InitValues();
            InitCulture();
        }

        private void InitValues()
        {
            nudWidth.Value = width;
        }

        private void InitCulture()
        {
            this.Text = "Save image";
            lblImageSize.Text = "Image size:";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Obtain a render of the Kinogram + whatever annotations are at the current time stamp.
            Size outputSize = new Size(width, height);
            Bitmap bmp = kinogram.Export(outputSize, timestamp);

            // Save the image.
            try
            {
                SaveFileDialog dlgSave = new SaveFileDialog();
                dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
                dlgSave.RestoreDirectory = true;
                dlgSave.Filter = FilesystemHelper.SaveImageFilter();
                dlgSave.FilterIndex = FilesystemHelper.GetFilterIndex(dlgSave.Filter, PreferencesManager.PlayerPreferences.ImageFormat);

                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    ImageHelper.Save(dlgSave.FileName, bmp);
            
                    PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(dlgSave.FileName);
                    PreferencesManager.Save();
                }
            }
            catch (Exception exp)
            {
                log.Error(exp.StackTrace);
            }

            bmp.Dispose();
        }

        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            width = (int)nudWidth.Value;
            height = (int)(width / aspect);

            manualUpdate = true;
            nudHeight.Value = Math.Max(Math.Min(height, nudHeight.Maximum), nudHeight.Minimum);
            manualUpdate = false;
        }

        private void nudHeight_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            height = (int)nudHeight.Value;
            width = (int)(height * aspect);

            manualUpdate = true;
            nudWidth.Value = Math.Max(Math.Min(width, nudWidth.Maximum), nudWidth.Minimum);
            manualUpdate = false;
        }
    }
}
