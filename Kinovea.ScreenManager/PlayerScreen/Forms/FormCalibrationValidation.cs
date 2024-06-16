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
using DocumentFormat.OpenXml.Drawing;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibrationValidation : Form
    {
        private Metadata metadata;
        private CalibrationHelper calibrationHelper;
        private List<NamedPoint> points = new List<NamedPoint>();
        private List<PointF> pointsOnGrid = new List<PointF>();
        private Vector3 eye;
        private bool ready;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormCalibrationValidation(Metadata metadata, CalibrationHelper calibrationHelper)
        {
            this.metadata = metadata;
            this.calibrationHelper = calibrationHelper;
            InitializeComponent();
            LocalizeForm();
            PopulateControlPoints();

            if (ready)
                ComputeCameraPosition();
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
            label1.Text = "Camera position in 3D: unknown";
        }

        private void ComputeCameraPosition()
        {
            // Compute the 3D position of the camera in grid space.
            var calibrator = calibrationHelper.CalibratorPlane;
            var quadWorld = calibrator.QuadWorld;
            var mapper = calibrationHelper.CalibratorPlane.Mapper;
            var lensCalib = calibrationHelper.DistortionHelper.Parameters;
            eye = CameraPoser.Compute(quadWorld, mapper, lensCalib);

            // Note: the resulting point is relative to the grid origin, not to
            // the user's custom origin if they moved the coordinate system axes, 
            // and it also doesn't take into account the custom value offset either.
            // It should take into account the rotation and mirroring of the 
            // calibration plane as this is baked directly in the quadImage coordinates.

            label1.Text = string.Format("Camera position in 3D: {0}.", eye);
        }

        private void PopulateControlPoints()
        {
            // Extract the relevant data from the marker objects.
            points.Clear();
            pointsOnGrid.Clear();
            foreach (var marker in metadata.CrossMarks())
            {
                // By default we assume the point is on the calibrated plane.
                // Also remember this value to always recompute from the same reference.
                var p = calibrationHelper.GetPoint(marker.Location);
                pointsOnGrid.Add(p);
                var namedPoint = new NamedPoint(marker.Name, p.X, p.Y, 0);
                points.Add(namedPoint);
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

            olvSections.AllColumns.AddRange(new OLVColumn[] {
                colName,
                colX,
                colY,
                colZ,
                });

            olvSections.Columns.AddRange(new ColumnHeader[] {
                colName,
                colX,
                colY,
                colZ,
                });

            // Populate the grid.
            olvSections.SetObjects(points);
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
            log.DebugFormat("cell edit finished.");

            var index = e.ListViewItem.Index;
            var target = new Vector3(pointsOnGrid[index].X, pointsOnGrid[index].Y, 0);

            // When the user fixes a component they are saying the point is at
            // the intersection of the ray and the plane they are fixing.
            var p = new Vector3(0, 0, 0);
            var view = target - eye;
            if (e.SubItemIndex == 1)
            {
                float x = (float)(double)e.NewValue;
                float r = (x - eye.X) / view.X;
                float y = eye.Y + r * view.Y;
                float z = eye.Z + r * view.Z;
                p = new Vector3(x, y, z);
            }
            else if (e.SubItemIndex == 2)
            {
                float y = (float)(double)e.NewValue;
                float r = (y - eye.Y) / view.Y;
                float x = eye.X + r * view.X;
                float z = eye.Z + r * view.Z;
                p = new Vector3(x, y, z);
            }
            else if (e.SubItemIndex == 3)
            {
                float z = (float)(double)e.NewValue;
                float r = (z - eye.Z) / view.Z;
                float x = eye.X + r * view.X;
                float y = eye.Y + r * view.Y;
                p = new Vector3(x, y, z);
            }

            NamedPoint np = (NamedPoint)e.RowObject;
            np.X = p.X;
            np.Y = p.Y;
            np.Z = p.Z;

            olvSections.RefreshObject(np);
            points[e.ListViewItem.Index] = np;
        }
    }
}
