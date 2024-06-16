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

namespace Kinovea.ScreenManager
{
    public partial class FormCalibrationValidation : Form
    {
        private Metadata metadata;
        private CalibrationHelper calibrationHelper;
        private List<DrawingCrossMark> markers = new List<DrawingCrossMark>(); 
        private List<PointF> pointsOnGrid = new List<PointF>();     // 2D points extracted from drawings.
        private List<NamedPoint> namedPoints = new List<NamedPoint>();   // 3D points + drawing name.
        private List<int> fixedComponent = new List<int>(); // Index of fixed component.
        private Vector3 eye;
        private bool ready;
        private Font fontRegular = new Font("Consolas", 9, FontStyle.Regular);
        private Font fontBold = new Font("Consolas", 9, FontStyle.Bold);
        private Font fontItalic = new Font("Consolas", 9, FontStyle.Italic);
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

            fixedComponent[index] = e.SubItemIndex - 1;

            NamedPoint np = (NamedPoint)e.RowObject;
            np.X = p.X;
            np.Y = p.Y;
            np.Z = p.Z;

            //namedPoints[e.ListViewItem.Index] = np;
            olvControlPoints.RefreshObject(np);
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
            log.DebugFormat("format row");
        }
    }
}
