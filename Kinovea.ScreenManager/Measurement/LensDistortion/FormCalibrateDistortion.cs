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
        private DistortionHelper distorter = new DistortionHelper();
        private Bitmap bmpCurrentImage;
        private Bitmap bmpGrid;
        private CalibrationHelper calibrationHelper;
        private Rectangle rect;
        private bool manualUpdate;
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private DistortionParameters distortionParameters;
        private string pathSpecial = "::Manual"; // Special path to indicate manual changes.
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
            
            InitializeComponent();
            LocalizeForm();

            SetupStyle();
            PopulateStyleElements();

            mnuOpen.Click += (s, e) => Open();
            mnuSave.Click += (s, e) => Save();
            mnuDefault.Click += (s, e) => RestoreDefaults();
            mnuQuit.Click += (s, e) => Close();

            AfterImport();
            PopulatePhysicalParameters();
            PopulateValues();
            UpdateDistortionGrid();
        }

        private void LocalizeForm()
        {
            this.Text = ScreenManagerLang.dlgCameraCalibration_Title;
            
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            
            mnuFile.Text = ScreenManagerLang.Generic_File;
            mnuOpen.Text = ScreenManagerLang.Generic_Open;
            mnuSave.Text = ScreenManagerLang.Generic_Save;
            mnuDefault.Text = ScreenManagerLang.Generic_Restore;
            mnuQuit.Text = ScreenManagerLang.Generic_Quit;
            
            grpDistortionCoefficients.Text = ScreenManagerLang.dlgCameraCalibration_DistortionCoefficients;
            grpIntrinsics.Text = ScreenManagerLang.dlgCameraCalibration_CameraIntrinsics;
            grpAppearance.Text = ScreenManagerLang.Generic_Appearance;

            lblSensorWidth.Text = ScreenManagerLang.dlgCameraCalibration_lblSensorWidth;
            lblFocalLength.Text = ScreenManagerLang.dlgCameraCalibration_lblFocalLength;
        }

        private void SetupStyle()
        {
            // Helper with typed accessors.
            styleData.Color = Color.Red;
            styleData.LineSize = 2;
            styleData.GridCols = 10;
            styleData.GridRows = 10;

            // The collection of UI elements.
            styleElements.Elements.Add("color", new StyleElementColor(Color.Red));
            styleElements.Elements.Add("line size", new StyleElementLineSize(2));
            styleElements.Elements.Add("cols", new StyleElementInt(1, 50, 10, "Columns"));
            styleElements.Elements.Add("rows", new StyleElementInt(1, 50, 10, "Rows"));

            // Binding.
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "line size");
            styleElements.Bind(styleData, "GridCols", "cols");
            styleElements.Bind(styleData, "GridRows", "rows");
        }
        
        private void PopulateStyleElements()
        {
            int editorsLeft = 100;
            Size editorSize = new Size(70, 20);
            int lastEditorBottom = 10;
            foreach (AbstractStyleElement styleElement in styleElements.Elements.Values)
            {
                styleElement.ValueChanged += styleElement_ValueChanged;

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            calibrationHelper.DistortionHelper.Initialize(distorter.Parameters, calibrationHelper.ImageSize);
            calibrationHelper.AfterDistortionUpdated();
        }

        private void Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_OpenDialogTitle;
            openFileDialog.Filter = FilesystemHelper.OpenXMLFilter();
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            DistortionParameters dp = DistortionImporterKinovea.Import(openFileDialog.FileName, calibrationHelper.ImageSize);
            if (dp != null)
            {
                distortionParameters = dp;
                dp.Path = openFileDialog.FileName;
                distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);

                AfterImport();
                PopulatePhysicalParameters();
                PopulateValues();
                UpdateDistortionGrid();
            }
        }

        private void RestoreDefaults()
        {
            distortionParameters = new DistortionParameters(calibrationHelper.ImageSize);
            distorter.Initialize(distortionParameters, calibrationHelper.ImageSize);

            AfterImport();
            PopulatePhysicalParameters();
            PopulateValues();
            UpdateDistortionGrid();
        }

        private void Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_SaveDialogTitle;
            saveFileDialog.Filter = FilesystemHelper.SaveXMLFilter();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            distorter.Parameters.Path = saveFileDialog.FileName;
            DistortionImporterKinovea.Export(saveFileDialog.FileName, distorter.Parameters, calibrationHelper.ImageSize);
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

            // Fx is a special case as it is linked to Focal length in mm, it has its own handler.

            distortionParameters.Fy = (double)nudFy.Value;
            distortionParameters.Cx = (double)nudCx.Value;
            distortionParameters.Cy = (double)nudCy.Value;

            distortionParameters.Path = pathSpecial;
            UpdateDistortionGrid();
        }

        private void styleElement_ValueChanged(object sender, EventArgs e)
        {
            UpdateDistortionGrid();
        }
        
        private void physicalParameters_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            double sensorWidth = (double)nudSensorWidth.Value;
            double focalLength = (double)nudFocalLength.Value;
            distortionParameters.PixelsPerMillimeter = calibrationHelper.ImageSize.Width / sensorWidth;

            distortionParameters.Fx = focalLength * distortionParameters.PixelsPerMillimeter;
            distortionParameters.Fy = distortionParameters.Fx;

            manualUpdate = true;
            nudFx.Value = (decimal)distortionParameters.Fx;
            nudFy.Value = (decimal)distortionParameters.Fy;
            manualUpdate = false;

            distortionParameters.Path = pathSpecial;
            UpdateDistortionGrid();
        }

        private void nudFx_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            distortionParameters.Fx = (double)nudFx.Value;
            focalLength = distortionParameters.Fx / distortionParameters.PixelsPerMillimeter;

            PopulatePhysicalParameters();
            distortionParameters.Path = pathSpecial;
            UpdateDistortionGrid();
        }

        private void AfterImport()
        {
            // Restore physical parameters.
            sensorWidth = calibrationHelper.ImageSize.Width / distortionParameters.PixelsPerMillimeter;
            focalLength = distortionParameters.Fx / distortionParameters.PixelsPerMillimeter;
        }

        private void PopulatePhysicalParameters()
        { 
            manualUpdate = true;
            nudSensorWidth.Value = (decimal)sensorWidth;
            nudFocalLength.Value = (decimal)focalLength;
            manualUpdate = false;
        }

        private void UpdateDistortionGrid()
        {
            // Update the grid bitmap and the preview.
            bmpGrid = distorter.GetDistortionGrid(
                styleData.Color, 
                styleData.LineSize, 
                styleData.GridCols, 
                styleData.GridRows);
            pnlPreview.Invalidate();
        }
    }
}
