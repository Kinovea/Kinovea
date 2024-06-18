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
using BrightIdeasSoftware;
using Kinovea.Services.Types;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibrationValidation : Form
    {
        private Metadata metadata;
        private CalibrationHelper calibrationHelper;
        private Action invalidator;
        private List<DrawingCrossMark> markers = new List<DrawingCrossMark>(); 
        private List<PointF> pointsOnGrid = new List<PointF>();     // 2D points extracted from drawings.
        private List<NamedPoint> namedPoints = new List<NamedPoint>();   // 3D points + drawing name.
        private List<int> fixedComponent = new List<int>(); // Index of fixed component.
        private CalibrationValidationMode validationMode = CalibrationValidationMode.Fix3D;
        private Vector3 eye;
        private bool ready;
        private Font fontRegular = new Font("Consolas", 9, FontStyle.Regular);
        private Font fontBold = new Font("Consolas", 9, FontStyle.Bold);
        private Font fontItalic = new Font("Consolas", 9, FontStyle.Italic);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormCalibrationValidation(Metadata metadata, CalibrationHelper calibrationHelper, Action invalidator)
        {
            this.metadata = metadata;
            this.calibrationHelper = calibrationHelper;
            this.invalidator = invalidator;
            InitializeComponent();
            LocalizeForm();
            PopulateControlPoints();

            if (ready)
            {
                ComputeCameraPosition();
                
                // Note: the resulting point is relative to the grid origin, not to
                // the user's custom origin if they moved the coordinate system axes, 
                // and it also doesn't take into account the custom value offset either.
                // It should already take into account the rotation and mirroring of the 
                // calibration plane as this is baked directly in the quadImage coordinates.
                label1.Text = string.Format("Camera position ({0}): X:{1:0.000}, Y:{2:0.000}, Z:{3:0.000}.", 
                    calibrationHelper.GetLengthAbbreviation(), eye.X, eye.Y, eye.Z);
            }
        }

        private void LocalizeForm()
        {
            this.Text = "Calibration validation";
            
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            grpIntrinsics.Text = "Camera";

            bool hasLensCalibration = calibrationHelper.DistortionHelper != null && calibrationHelper.DistortionHelper.Initialized;
            bool hasPlaneCalibration = calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Plane;
            ready = hasLensCalibration && hasPlaneCalibration;

            string lensCalibrationStatus = hasLensCalibration ? "found" : "missing";
            string planeCalibrationStatus = hasPlaneCalibration ? "found" : "missing";

            lblSensorWidth.Text = string.Format("Lens calibration: {0}", lensCalibrationStatus);
            lblFocalLength.Text = string.Format("Plane calibration: {0}", planeCalibrationStatus);

            lblSensorWidth.ForeColor = hasLensCalibration ? Color.Green : Color.Red;
            lblFocalLength.ForeColor = hasPlaneCalibration ? Color.Green : Color.Red;

            label1.Enabled = ready;
            label1.Text = "Camera position: unknown";

            gpControlPoints.Enabled = ready;
            gpControlPoints.Text = "Control points";
            gpValidationMode.Text = "Validation mode";
            rbFix3D.Text = "Fix all axes: verify the location of the marker in the image";
            rbFix1D.Text = "Fix one axis and the marker: verify the other two axes";
        }

        private void ComputeCameraPosition()
        {
            // Compute the 3D position of the camera in grid space.
            var calibrator = calibrationHelper.CalibratorPlane;
            var quadWorld = calibrator.QuadWorld;
            var mapper = calibrationHelper.CalibratorPlane.Mapper;
            var lensCalib = calibrationHelper.DistortionHelper.Parameters;
            eye = CameraPoser.Compute(quadWorld, mapper, lensCalib);
        }

        private void PopulateControlPoints()
        {
            rbFix1D.Checked = validationMode == CalibrationValidationMode.Fix1D;
            rbFix3D.Checked = validationMode == CalibrationValidationMode.Fix3D;

            // Allow formatting of single cells
            // ref: https://objectlistview.sourceforge.net/cs/recipes.html#recipe-formatter
            olvControlPoints.UseCellFormatEvents = true;
            olvControlPoints.Font = fontRegular;

            // Extract the relevant data from the marker objects.
            namedPoints.Clear();
            pointsOnGrid.Clear();
            markers = metadata.CrossMarks().ToList();
            foreach (var marker in markers)
            {
                // By default we assume the point is on the calibrated plane.
                // Also remember this value to always recompute from the same reference.
                var p = calibrationHelper.GetPoint(marker.Location);
                pointsOnGrid.Add(p);

                var namedPoint = new NamedPoint(marker.Name, p.X, p.Y, 0);
                namedPoints.Add(namedPoint);

                // Init with no fixed component.
                fixedComponent.Add(-1);
            }
            
            var colName = new OLVColumn();
            var colX = new OLVColumn();
            var colY = new OLVColumn();
            var colZ = new OLVColumn();

            SetupColumn(colName);
            SetupColumn(colX);
            SetupColumn(colY);
            SetupColumn(colZ);
            colName.IsEditable = false;
            colName.TextAlign = HorizontalAlignment.Left;
            colName.FillsFreeSpace = true;
            colName.FreeSpaceProportion = 1;

            // Name of the property used to get the data from the objects.
            colName.AspectName = "Name";
            colX.AspectName = "X";
            colY.AspectName = "Y";
            colZ.AspectName = "Z";
            
            // Displayed column name.
            colName.Text = "Name";
            colX.Text = "X";
            colY.Text = "Y";
            colZ.Text = "Z";

            olvControlPoints.AllColumns.AddRange(new OLVColumn[] {
                colName,
                colX,
                colY,
                colZ,
                });

            olvControlPoints.Columns.AddRange(new ColumnHeader[] {
                colName,
                colX,
                colY,
                colZ,
                });

            // Populate the grid.
            olvControlPoints.SetObjects(namedPoints);
            
            // This is required to trigger the formatting.
            foreach (var np in namedPoints)
                olvControlPoints.RefreshObject(np);
        }

        private void SetupColumn(OLVColumn col)
        {
            col.Groupable = false;
            col.Sortable = false;
            col.IsEditable = true;
            col.TextAlign = HorizontalAlignment.Center;
            col.MinimumWidth = 75;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
        }

        private void olvSections_CellEditFinished(object sender, CellEditEventArgs e)
        {
            var index = e.ListViewItem.Index;
            var componentIndex = e.SubItemIndex - 1;
            float newValue = (float)(double)e.NewValue;
            NamedPoint np = (NamedPoint)e.RowObject;

            if (validationMode == CalibrationValidationMode.Fix1D)
            {
                // The user fixed one coordinate, we recompute the other two,
                // and update the values in the table.
                Vector3 p = GetPoint(index, componentIndex, newValue);
                np.X = p.X;
                np.Y = p.Y;
                np.Z = p.Z;
                fixedComponent[index] = componentIndex;
            }
            else
            {
                // The user fixed one coordinate but all 3 are considered 
                // valid, we recompute the projection of the point on the 
                // plane at z=0, then we update the marker object with it.
                MoveMarker(index, np, componentIndex, newValue);
            }
            
            
            olvControlPoints.RefreshObject(np);
            
        }

        private Vector3 GetPoint(int index, int componentIndex, float newValue)
        {
            // We assume the marker value at z=0 is correct, so there is a ray
            // going from the camera point to the marker on the calibrated plane.
            // Intersect this ray with the plane specified by the user fixing
            // one coordinate.
            var target = new Vector3(pointsOnGrid[index].X, pointsOnGrid[index].Y, 0);
            var view = target - eye;
            var p = new Vector3(0, 0, 0);
            if (componentIndex == 0)
            {
                float x = newValue;
                float r = (x - eye.X) / view.X;
                float y = eye.Y + r * view.Y;
                float z = eye.Z + r * view.Z;
                p = new Vector3(x, y, z);
            }
            else if (componentIndex == 1)
            {
                float y = newValue;
                float r = (y - eye.Y) / view.Y;
                float x = eye.X + r * view.X;
                float z = eye.Z + r * view.Z;
                p = new Vector3(x, y, z);
            }
            else if (componentIndex == 2)
            {
                float z = newValue;
                float r = (z - eye.Z) / view.Z;
                float x = eye.X + r * view.X;
                float y = eye.Y + r * view.Y;
                p = new Vector3(x, y, z);
            }

            return p;
        }

        private void MoveMarker(int index, NamedPoint np, int componentIndex, float newValue)
        {
            // This time we assume the 3D point in the table is correct.
            // Trace the ray going from the camera to this point and intersect
            // it with the calibrated plane to get the new 2D coordinates.
            
            // Rebuild the new value.
            if (componentIndex == 0)
            {
                np.X = newValue;
            }
            else if (componentIndex == 1)
            {
                np.Y = newValue;
            }
            else if (componentIndex == 2)
            {
                np.Z = newValue;
            }

            // Project the point on the calibrated grid.
            float r = (np.Z - eye.Z) / (-eye.Z);
            float x = eye.X + (np.X - eye.X) / r;
            float y = eye.Y + (np.Y - eye.Y) / r;
            PointF pointOnGrid = new PointF(x, y);

            // Transform to image space (including radial distortion).
            PointF p = calibrationHelper.GetImagePoint(pointOnGrid);
            
            // Move the object and refresh the view.
            markers[index].MovePoint(p);
            if (invalidator != null)
                invalidator();

            // Update our local copy.
            pointsOnGrid[index] = pointOnGrid;
        }

        private void olvControlPoints_FormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                // Paint the name according to the drawing object color.
                var marker = markers[e.RowIndex];
                var bicolor = new Bicolor(marker.Color);
                e.SubItem.ForeColor = bicolor.Foreground;
                e.SubItem.BackColor = bicolor.Background;
                e.SubItem.Font = new Font(e.SubItem.Font, FontStyle.Bold);
            }
            else
            {
                bool isFixed = fixedComponent[e.RowIndex] == e.ColumnIndex - 1;
                e.SubItem.Font = new Font(e.SubItem.Font, isFixed ? FontStyle.Bold : FontStyle.Italic);
            }
            
        }

        private void olvControlPoints_FormatRow(object sender, FormatRowEventArgs e)
        {
        }

        private void validationMode_Changed(object sender, EventArgs e)
        {
            validationMode = rbFix1D.Checked ? CalibrationValidationMode.Fix1D : CalibrationValidationMode.Fix3D;
        }
    }
}
