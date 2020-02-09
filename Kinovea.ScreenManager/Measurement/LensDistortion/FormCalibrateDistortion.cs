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
        private DistortionParameters distortionParameters;
        private double sensorWidth = DistortionParameters.defaultSensorWidth;
        private double focalLength = DistortionParameters.defaultFocalLength;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormCalibrateDistortion(Bitmap currentImage, List<List<PointF>> points, CalibrationHelper calibrationHelper)
        {
            this.bmpCurrentImage = currentImage;
            this.calibrationHelper = calibrationHelper;

            if (calibrationHelper.DistortionHelper == null || !calibrationHelper.DistortionHelper.Initialized)
                distortionParameters = new DistortionParameters(calibrationHelper.ImageSize);
            else
                distortionParameters = calibrationHelper.DistortionHelper.Parameters;
                
            distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);
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
            RecomputePhysicalParameters();
            PopulateValues();
            UpdateDistortionGrid();
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

        private void PopulateValues()
        {
            manualUpdate = true;
            try
            {
                nudSensorWidth.Value = (decimal)sensorWidth;
                nudFocalLength.Value = (decimal)focalLength;

                nudFx.Value = (decimal)distortionParameters.Fx;
                nudFy.Value = (decimal)distortionParameters.Fy;
                nudCx.Value = (decimal)distortionParameters.Cx;
                nudCy.Value = (decimal)distortionParameters.Cy;
                
                nudK1.Value = (decimal)distortionParameters.K1;
                nudK2.Value = (decimal)distortionParameters.K2;
                nudK3.Value = (decimal)distortionParameters.K3;
                nudP1.Value = (decimal)distortionParameters.P1;
                nudP2.Value = (decimal)distortionParameters.P2;
            }
            catch (Exception e)
            {
                // A value is out of range of the control.
                log.ErrorFormat("Error while populating lens distortion parameters. {0}", e.Message);
            }

            manualUpdate = false;
        }

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            if (!calibrator.Valid)
                return;

            distortionParameters = calibrator.Calibrate();
            distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);
            RecomputePhysicalParameters();
            UpdateDistortionGrid();
            PopulateValues();
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

            DistortionParameters dp = DistortionImporterKinovea.Import(openFileDialog.FileName, calibrationHelper.ImageSize);
            if (dp != null)
            {
                distortionParameters = dp;
                distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);
                RecomputePhysicalParameters();
                PopulateValues();
                UpdateDistortionGrid();
            }
        }

        private void RestoreDefaults()
        {
            distortionParameters = new DistortionParameters(calibrationHelper.ImageSize);
            distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);
            RecomputePhysicalParameters();
            PopulateValues();
            UpdateDistortionGrid();
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

            DistortionParameters dp = DistortionImporterAgisoft.Import(openFileDialog.FileName, calibrationHelper.ImageSize);
            if (dp != null)
            {
                distortionParameters = dp;
                distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);
                RecomputePhysicalParameters();
                PopulateValues();
                UpdateDistortionGrid();
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

            distortionParameters.K1 = (double)nudK1.Value;
            distortionParameters.K2 = (double)nudK2.Value;
            distortionParameters.K3 = (double)nudK3.Value;
            distortionParameters.P1 = (double)nudP1.Value;
            distortionParameters.P2 = (double)nudP2.Value;

            distortionParameters.Fx = (double)nudFx.Value;
            distortionParameters.Fy = (double)nudFy.Value;
            distortionParameters.Cx = (double)nudCx.Value;
            distortionParameters.Cy = (double)nudCy.Value;

            UpdateDistortionGrid();
        }

        private void element_ValueChanged(object sender, EventArgs e)
        {
            UpdateDistortionGrid();
        }
        
        private void physicalParameters_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            double sensorWidth = (double)nudSensorWidth.Value;
            double focalLength = (double)nudFocalLength.Value;
            double pixelsPerMillimeter = calibrationHelper.ImageSize.Width / sensorWidth;

            distortionParameters.Fx = focalLength * pixelsPerMillimeter;
            distortionParameters.Fy = distortionParameters.Fx;

            manualUpdate = true;
            nudFx.Value = (decimal)distortionParameters.Fx;
            nudFy.Value = (decimal)distortionParameters.Fy;
            manualUpdate = false;

            UpdateDistortionGrid();
        }

        private void nudFx_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            distortionParameters.Fx = (double)nudFx.Value;

            RecomputePhysicalParameters();
            UpdateDistortionGrid();
        }

        private void RecomputePhysicalParameters()
        { 
            // Recompute physical focal length based on sensor width.
            double pixelsPerMillimeter = calibrationHelper.ImageSize.Width / sensorWidth;
            focalLength = distortionParameters.Fx / pixelsPerMillimeter;
            
            manualUpdate = true;
            nudFocalLength.Value = (decimal)focalLength;
            manualUpdate = false;
        }

        private void UpdateDistortionGrid()
        {
            // Update the grid bitmap and the preview.
            bmpGrid = distorter.GetDistortionGrid(styleHelper.Color, styleHelper.LineSize, styleHelper.GridDivisions);
            pnlPreview.Invalidate();
        }
    }
}
