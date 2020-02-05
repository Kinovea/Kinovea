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
        private Bitmap bmpCurrentImage;
        private Bitmap bmpGrid;
        private CalibrationHelper calibrationHelper;
        private Rectangle rect;
        private bool manualUpdate;
        private DrawingStyle style = new DrawingStyle();
        private StyleHelper styleHelper = new StyleHelper();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormCalibrateDistortion(Bitmap currentImage, List<List<PointF>> points, CalibrationHelper calibrationHelper)
        {
            this.bmpCurrentImage = currentImage;
            this.calibrationHelper = calibrationHelper;

            if (calibrationHelper.DistortionHelper == null || !calibrationHelper.DistortionHelper.Initialized)
                distorter.Initialize(DistortionParameters.Default, calibrationHelper.ImageSize);
            else
                distorter.Initialize(calibrationHelper.DistortionHelper.Parameters, calibrationHelper.ImageSize);

            calibrator = new CameraCalibrator(points, calibrationHelper.ImageSize);

            InitializeComponent();
            LocalizeForm();

            SetupStyle();
            PopulateStyleElements();

            mnuOpen.Click += (s, e) => Open();
            mnuSave.Click += (s, e) => Save();
            mnuImportAgisoft.Click += (s, e) => ImportAgisoft();
            mnuDefault.Click += (s, e) => RestoreDefaults();
            mnuQuit.Click += (s, e) => Close();

            btnCalibrate.Enabled = calibrator.Valid;
            Populate();
        }

        private void LocalizeForm()
        {
            this.Text = ScreenManagerLang.dlgCameraCalibration_Title;
            
            btnCalibrate.Text = ScreenManagerLang.dlgCameraCalibration_CalibrateCamera;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            
            mnuFile.Text = ScreenManagerLang.Generic_File;
            mnuOpen.Text = ScreenManagerLang.Generic_Open;
            mnuSave.Text = ScreenManagerLang.Generic_Save;
            mnuImport.Text = ScreenManagerLang.Generic_Import;
            mnuDefault.Text = ScreenManagerLang.Generic_Restore;
            mnuQuit.Text = ScreenManagerLang.Generic_Quit;
            mnuImportAgisoft.Text = "Agisoft Lens";

            grpDistortionCoefficients.Text = ScreenManagerLang.dlgCameraCalibration_DistortionCoefficients;
            grpIntrinsics.Text = ScreenManagerLang.dlgCameraCalibration_CameraIntrinsics;
            grpAppearance.Text = ScreenManagerLang.Generic_Appearance;
        }

        private void SetupStyle()
        {
            // Helper with typed accessors.
            styleHelper.Color = Color.Red;
            styleHelper.LineSize = 2;
            styleHelper.GridDivisions = 10;

            // The collection of UI elements.
            style.Elements.Add("color", new StyleElementColor(Color.Red));
            style.Elements.Add("line size", new StyleElementLineSize(2));
            style.Elements.Add("divisions", new StyleElementGridDivisions(10));

            // Binding.
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "line size");
            style.Bind(styleHelper, "GridDivisions", "divisions");
        }
        
        private void PopulateStyleElements()
        {
            int editorsLeft = 100;
            Size editorSize = new Size(70, 20);
            int lastEditorBottom = 10;
            foreach (AbstractStyleElement styleElement in style.Elements.Values)
            {
                styleElement.ValueChanged += element_ValueChanged;

                Button btn = new Button();
                btn.Image = styleElement.Icon;
                btn.Size = new Size(20, 20);
                btn.Location = new Point(10, lastEditorBottom + 15);
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

        private void Populate()
        {
            manualUpdate = true;
            try
            {
                DistortionParameters p = distorter.Parameters;
                nudFx.Value = (decimal)p.Fx;
                nudFy.Value = (decimal)p.Fy;
                nudCx.Value = (decimal)p.Cx;
                nudCy.Value = (decimal)p.Cy;
                nudK1.Value = (decimal)p.K1;
                nudK2.Value = (decimal)p.K2;
                nudK3.Value = (decimal)p.K3;
                nudP1.Value = (decimal)p.P1;
                nudP2.Value = (decimal)p.P2;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while populating lens distortion parameters. {0}", e.Message);
            }

            manualUpdate = false;
            UpdateDistortion();
        }

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            if (!calibrator.Valid)
                return;

            DistortionParameters parameters = calibrator.Calibrate();
            distorter.Initialize(parameters, calibrationHelper.ImageSize);
            
            Populate();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            calibrationHelper.DistortionHelper.Initialize(distorter.Parameters, calibrationHelper.ImageSize);
            calibrationHelper.AfterDistortionUpdated();
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

        private void RestoreDefaults()
        {
            DistortionParameters parameters = DistortionParameters.Default;
            distorter.Initialize(parameters, calibrationHelper.ImageSize);
            Populate();
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

        private void pnlPreview_Paint(object sender, PaintEventArgs e)
        {
            if (bmpCurrentImage == null || bmpGrid == null)
                return;

            rect = UIHelper.RatioStretch(calibrationHelper.ImageSize, pnlPreview.Size);

            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.DrawImage(bmpCurrentImage, rect);
            g.DrawImage(bmpGrid, rect);
        }

        private void pnlPreview_Resize(object sender, EventArgs e)
        {
            pnlPreview.Invalidate();
        }

        private void parameters_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            UpdateDistortion();
        }

        private void element_ValueChanged(object sender, EventArgs e)
        {
            UpdateDistortion();
        }

        private void UpdateDistortion()
        { 
            // Update distorter.
            double k1 = (double)nudK1.Value;
            double k2 = (double)nudK2.Value;
            double k3 = (double)nudK3.Value;
            double p1 = (double)nudP1.Value;
            double p2 = (double)nudP2.Value;

            double fx = (double)nudFx.Value;
            double fy = (double)nudFy.Value;
            double cx = (double)nudCx.Value;
            double cy = (double)nudCy.Value;

            DistortionParameters parameters = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy);
            distorter.Initialize(parameters, calibrationHelper.ImageSize);

            // Update the grid bitmap and the preview.
            bmpGrid = distorter.GetDistortionGrid(styleHelper.Color, styleHelper.LineSize, styleHelper.GridDivisions);
            pnlPreview.Invalidate();
        }
    }
}
