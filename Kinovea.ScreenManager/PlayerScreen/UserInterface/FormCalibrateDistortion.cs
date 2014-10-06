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
    public partial class FormCalibrateDistortion : Form
    {
        private CameraCalibrator calibrator;
        private DistortionHelper distorter = new DistortionHelper();
        private Bitmap currentImage;
        private Bitmap bmpUndistorted;
        private CalibrationHelper calibrationHelper;
        private bool displayOriginal;

        public FormCalibrateDistortion(Bitmap currentImage, List<List<PointF>> points, CalibrationHelper calibrationHelper)
        {
            this.currentImage = currentImage;
            this.calibrationHelper = calibrationHelper;

            if (calibrationHelper.DistortionHelper == null || !calibrationHelper.DistortionHelper.Initialized)
            {
                distorter.Initialize(DistortionParameters.Default, calibrationHelper.ImageSize);
                bmpUndistorted = currentImage;
            }
            else
            {
                distorter.Initialize(calibrationHelper.DistortionHelper.Parameters, calibrationHelper.ImageSize);
            }

            calibrator = new CameraCalibrator(points, calibrationHelper.ImageSize);

            InitializeComponent();
            LocalizeForm();

            mnuOpen.Click += (s, e) => Open();
            mnuSave.Click += (s, e) => Save();
            mnuImportAgisoft.Click += (s, e) => ImportAgisoft();
            mnuDefault.Click += (s, e) => RestoreDefaults();
            mnuQuit.Click += (s, e) => Close();

            if (currentImage == null)
                tabPages.TabPages.Remove(tabImage);

            btnCalibrate.Enabled = calibrator.Valid;

            Populate();
        }

        private void LocalizeForm()
        {
            this.Text = ScreenManagerLang.dlgCameraCalibration_Title;
            this.grpDistortionCoefficients.Text = ScreenManagerLang.dlgCameraCalibration_DistortionCoefficients;
            this.grpIntrinsics.Text = ScreenManagerLang.dlgCameraCalibration_CameraIntrinsics;
            this.btnCalibrate.Text = ScreenManagerLang.dlgCameraCalibration_CalibrateCamera;
            this.btnOK.Text = ScreenManagerLang.Generic_Apply;
            this.btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            this.mnuFile.Text = ScreenManagerLang.Generic_File;
            this.mnuOpen.Text = ScreenManagerLang.Generic_Open;
            this.mnuSave.Text = ScreenManagerLang.Generic_Save;
            this.mnuImport.Text = ScreenManagerLang.Generic_Import;
            this.mnuDefault.Text = ScreenManagerLang.Generic_Restore;
            this.mnuQuit.Text = ScreenManagerLang.Generic_Quit;
            tabPages.TabPages["tabDistortion"].Text = ScreenManagerLang.dlgCameraCalibration_Distortion;
            tabPages.TabPages["tabImage"].Text = ScreenManagerLang.Generic_Image;

            this.mnuImportAgisoft.Text = "Agisoft Lens";
        }

        private void Populate()
        {
            DistortionParameters p = distorter.Parameters;
            lblK1.Text = string.Format("k1 : {0:0.000}", p.K1);
            lblK2.Text = string.Format("k2 : {0:0.000}", p.K2);
            lblK3.Text = string.Format("k3 : {0:0.000}", p.K3);
            lblP1.Text = string.Format("p1 : {0:0.000}", p.P1);
            lblP2.Text = string.Format("p2 : {0:0.000}", p.P2);

            lblFx.Text = string.Format("fx : {0:0.000}", p.Fx);
            lblFy.Text = string.Format("fy : {0:0.000}", p.Fy);
            lblCx.Text = string.Format("cx : {0:0.000}", p.Cx);
            lblCy.Text = string.Format("cy : {0:0.000}", p.Cy);

            Color background = Color.FromArgb(255, 42, 42, 42);
            Color foreground = Color.White;
            int steps = 20;
            Bitmap bmpDistortionGrid = distorter.GetDistortionGrid(background, foreground, steps);
            RatioStretch(bmpDistortionGrid, pbDistortion);

            bmpUndistorted = distorter.GetUndistortedImage(currentImage);

            UpdateImages();
        }

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            if (!calibrator.Valid)
                return;

            DistortionParameters parameters = calibrator.Calibrate();
            distorter.Initialize(parameters, calibrationHelper.ImageSize);
            
            Populate();
        }

        private void RatioStretch(Bitmap bitmap, PictureBox pbImage)
        {
            float ratioHeight = (float)bitmap.Height / pbImage.Height;
            float ratioWidth = (float)bitmap.Width / pbImage.Width;
            float ratio = Math.Max(ratioHeight, ratioWidth);
            Bitmap stretched = new Bitmap((int)(bitmap.Width / ratio), (int)(bitmap.Height / ratio), bitmap.PixelFormat);
            Graphics g = Graphics.FromImage(stretched);
            g.InterpolationMode = InterpolationMode.Bilinear;

            g.DrawImage(bitmap, 0, 0, stretched.Width, stretched.Height);

            pbImage.BackgroundImage = stretched;
        }

        private void pbImage_Click(object sender, EventArgs e)
        {
            displayOriginal = !displayOriginal;
            UpdateImages();
        }

        private void UpdateImages()
        {
            if (currentImage == null || bmpUndistorted == null)
                return;
            
            if (displayOriginal)
                RatioStretch(currentImage, pbImage);
            else
                RatioStretch(bmpUndistorted, pbImage);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            calibrationHelper.DistortionHelper.Initialize(distorter.Parameters, calibrationHelper.ImageSize);
            calibrationHelper.AfterDistortionUpdated();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_OpenDialogTitle;
            openFileDialog.Filter = ScreenManagerLang.FileFilter_XML;
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            DistortionParameters parameters = DistortionImporterKinovea.Import(openFileDialog.FileName, calibrationHelper.ImageSize);

            if (parameters != null)
            {
                distorter.Initialize(parameters, calibrationHelper.ImageSize);
                Populate();
            }
        }

        private void btnResetToDefault_Click(object sender, EventArgs e)
        {
            RestoreDefaults();
        }

        private void RestoreDefaults()
        {
            DistortionParameters parameters = DistortionParameters.Default;
            distorter.Initialize(parameters, calibrationHelper.ImageSize);
            Populate();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_SaveDialogTitle;
            saveFileDialog.Filter = ScreenManagerLang.FileFilter_XML;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            DistortionImporterKinovea.Export(saveFileDialog.FileName, distorter.Parameters, calibrationHelper.ImageSize);
        }

        private void ImportAgisoft()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_OpenDialogTitle;
            openFileDialog.Filter = ScreenManagerLang.FileFilter_XML;
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            DistortionParameters parameters = DistortionImporterAgisoft.Import(openFileDialog.FileName, calibrationHelper.ImageSize);

            if (parameters != null)
            {
                distorter.Initialize(parameters, calibrationHelper.ImageSize);
                Populate();
            }
        }
    }
}
