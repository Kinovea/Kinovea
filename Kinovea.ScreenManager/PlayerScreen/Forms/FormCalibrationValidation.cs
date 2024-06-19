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
using System.Globalization;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibrationValidation : Form
    {
        #region Members
        private Metadata metadata;
        private Metadata otherMetadata;
        private CalibrationHelper calibrationHelper;
        private CalibrationHelper otherCalibrationHelper;
        private Action invalidator;

        private List<DrawingCrossMark> markers = new List<DrawingCrossMark>(); 
        private List<PointF> pointsOnGrid = new List<PointF>();     // 2D points extracted from drawings.
        private List<NamedPoint> namedPoints = new List<NamedPoint>();   // 3D points + drawing name.
        private List<int> fixedComponent = new List<int>(); // Index of fixed component.
        private CalibrationValidationMode validationMode = CalibrationValidationMode.Fix3D;
        private Vector3 eye;
        private Vector3 eye2;
        private bool hasFullCalibration;
        private bool hasOtherFullCalibration = false;
        private Font fontRegular = new Font("Consolas", 9, FontStyle.Regular);
        private Font fontBold = new Font("Consolas", 9, FontStyle.Bold);
        private Font fontItalic = new Font("Consolas", 9, FontStyle.Italic);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FormCalibrationValidation(Metadata metadata, Metadata otherMetadata, Action invalidator)
        {
            this.metadata = metadata;
            this.otherMetadata = otherMetadata;
            this.calibrationHelper = metadata.CalibrationHelper;
            this.otherCalibrationHelper = otherMetadata?.CalibrationHelper;
            this.invalidator = invalidator;
            InitializeComponent();
            LocalizeForm();
            SetupTable();

            if (hasFullCalibration)
            {
                eye = ComputeCameraPosition(calibrationHelper);
                
                // Note: the resulting point is relative to the grid origin, not to
                // the user's custom origin if they moved the coordinate system axes, 
                // and it also doesn't take into account the custom value offset either.
                // It should already take into account the rotation and mirroring of the 
                // calibration plane as this is baked directly in the quadImage coordinates.
                label1.Text = string.Format("Camera position ({0}): X:{1:0.000}, Y:{2:0.000}, Z:{3:0.000}.", 
                    calibrationHelper.GetLengthAbbreviation(), eye.X, eye.Y, eye.Z);

                // Compute the camera position of the other screen if possible.
                if (otherMetadata != null && otherCalibrationHelper != null)
                {
                    bool hasLensCalibration = otherCalibrationHelper.DistortionHelper != null && otherCalibrationHelper.DistortionHelper.Initialized;
                    bool hasPlaneCalibration = otherCalibrationHelper.IsCalibrated && otherCalibrationHelper.CalibratorType == CalibratorType.Plane;
                    hasOtherFullCalibration = hasLensCalibration && hasPlaneCalibration;
                    if (hasOtherFullCalibration)
                    {
                        eye2 = ComputeCameraPosition(otherCalibrationHelper);
                    }
                }
            }

            PopulateControlPoints();
        }

        private void LocalizeForm()
        {
            this.Text = "Calibration validation";
            
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            grpIntrinsics.Text = "Camera";

            bool hasLensCalibration = calibrationHelper.DistortionHelper != null && calibrationHelper.DistortionHelper.Initialized;
            bool hasPlaneCalibration = calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Plane;
            hasFullCalibration = hasLensCalibration && hasPlaneCalibration;

            string lensCalibrationStatus = hasLensCalibration ? "found" : "missing";
            string planeCalibrationStatus = hasPlaneCalibration ? "found" : "missing";

            lblSensorWidth.Text = string.Format("Lens calibration: {0}", lensCalibrationStatus);
            lblFocalLength.Text = string.Format("Plane calibration: {0}", planeCalibrationStatus);

            lblSensorWidth.ForeColor = hasLensCalibration ? Color.Green : Color.Red;
            lblFocalLength.ForeColor = hasPlaneCalibration ? Color.Green : Color.Red;

            label1.Enabled = hasFullCalibration;
            label1.Text = "Camera position: unknown";

            gpControlPoints.Enabled = hasFullCalibration;
            gpControlPoints.Text = "Control points";
            gpValidationMode.Text = "Validation mode";
            rbFix3D.Text = "Change all axes and verify the location of the marker in the image";
            rbFix1D.Text = "Change one axis and verify the other two axes";
            rbCompute3D.Text = "3D positions computed from the two views";

            rbCompute3D.Enabled = otherMetadata != null;

            rbFix3D.Checked = validationMode == CalibrationValidationMode.Fix3D;
            rbFix1D.Checked = validationMode == CalibrationValidationMode.Fix1D;
            rbCompute3D.Checked = validationMode == CalibrationValidationMode.Compute3D;
        }

        /// <summary>
        /// Setup the control points table columns and formatting.
        /// This is independent of the data or validation mode.
        /// </summary>
        private void SetupTable()
        {
            // Allow formatting of single cells
            // ref: https://objectlistview.sourceforge.net/cs/recipes.html#recipe-formatter
            olvControlPoints.UseCellFormatEvents = true;
            olvControlPoints.Font = fontRegular;

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
        }
        
        /// <summary>
        /// Populate the table of control points.
        /// This is called whenever we change the validation mode to reset the values.
        /// </summary>
        private void PopulateControlPoints()
        {
            // Extract the relevant data from the marker objects.
            namedPoints.Clear();
            pointsOnGrid.Clear();
            fixedComponent.Clear();
            markers.Clear();
            AlphanumComparator alphaNumComparator = new AlphanumComparator();

            if (validationMode == CalibrationValidationMode.Fix1D || validationMode == CalibrationValidationMode.Fix3D)
            {
                // In these modes we collect the marker points and initialize the 3D positions with their 
                // location on the grid (z=0). The location may change later from user input and the cells
                // are bound to the NamedPoint objects.
                markers = metadata.CrossMarks().ToList();
                markers.Sort((a, b) => alphaNumComparator.Compare(a.Name, b.Name));

                foreach (var marker in markers)
                {
                    var p = calibrationHelper.GetPoint(marker.Location);
                    var p2 = Round3(new Vector3(p.X, p.Y, 0));

                    var namedPoint = new NamedPoint(marker.Name, p2.X, p2.Y, p2.Z);
                    namedPoints.Add(namedPoint);

                    // Remember the original data.
                    pointsOnGrid.Add(p);
                    
                    // Init with no fixed component. This is used for Fix1D mode.
                    fixedComponent.Add(-1);
                }
            }
            else
            {
                // Match markers in both views and only add these.
                var markers1 = metadata.CrossMarks().ToList();
                markers1.Sort((a, b) => alphaNumComparator.Compare(a.Name, b.Name));

                var markers2 = otherMetadata.CrossMarks().ToList();
                var matches = new Dictionary<int, int>();
                for (int i = 0; i < markers1.Count; i++) 
                {
                    for (int j = 0; j < markers2.Count; j++)
                    {
                        if (markers2[j].Name == markers1[i].Name)
                        {
                            matches.Add(i, j);
                        }
                    }
                }

                // Compute the 3D position of matched markers and associate
                // the rows with these.
                foreach (var m in matches.Keys)
                {
                    int i = m;
                    int j = matches[m];
                    
                    // The `markers` list is also used later to get the color 
                    // for the name cell formatting so we need to fill it correctly.
                    markers.Add(markers1[i]);

                    // Compute 3D position.
                    var p1 = calibrationHelper.GetPoint(markers1[i].Location);
                    var p2 = otherCalibrationHelper.GetPoint(markers2[j].Location);
                    var p = Compute3D(eye, p1, eye2, p2);
                    p = Round3(p);

                    // Build the named point, this is what's used by the table.
                    var namedPoint = new NamedPoint(markers1[i].Name, p.X, p.Y, p.Z);
                    namedPoints.Add(namedPoint);

                    // In this case the pointsOnGrid and fixedComponent aren't important since
                    // we don't allow value modification.
                    pointsOnGrid.Add(p1);
                    fixedComponent.Add(-1);
                }
            }

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

        #region Event handlers
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
            if (rbFix3D.Checked)
            {
                validationMode = CalibrationValidationMode.Fix3D;
            }
            else if (rbFix1D.Checked)
            {
                validationMode = CalibrationValidationMode.Fix1D;
            }
            else if (rbCompute3D.Checked)
            {
                validationMode = CalibrationValidationMode.Compute3D;
            }

            bool editable = validationMode == CalibrationValidationMode.Fix1D || validationMode == CalibrationValidationMode.Fix3D;
            foreach (var c in olvControlPoints.AllColumns)
            {
                c.IsEditable = editable;
            }

            // Repopulate the table from scratch.
            PopulateControlPoints();
        }

        private void btnCSV_Click(object sender, EventArgs e)
        {
            List<string> csv = GetCSV();
            CSVHelper.CopyToClipboard(csv);
        }
        #endregion

        #region Private helpers
        /// <summary>
        /// Compute the camera position for a given plane and lens calibration.
        /// </summary>
        private Vector3 ComputeCameraPosition(CalibrationHelper calibrationHelper)
        {
            // Compute the 3D position of the camera in grid space.
            var calibrator = calibrationHelper.CalibratorPlane;
            var quadWorld = calibrator.QuadWorld;
            var mapper = calibrationHelper.CalibratorPlane.Mapper;
            var lensCalib = calibrationHelper.DistortionHelper.Parameters;
            var eye = CameraPoser.Compute(quadWorld, mapper, lensCalib);
            return eye;
        }

        /// <summary>
        /// Get the 3D coordinate of the point at index, assuming the 
        /// coordinate of the marker at z=0 is correct and the user 
        /// provided a new value for one of the coordinates.
        /// </summary>
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

        /// <summary>
        /// Move the marker at index to the new location.
        /// Assuming the 3D position provided by the user in the table is correct.
        /// </summary>
        private void MoveMarker(int index, NamedPoint np, int componentIndex, float newValue)
        {
            // Trace the ray going from the camera to the user provided point and intersect
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

        /// <summary>
        /// Compute the location of a marker in 3D from its 2D locations in each view.
        /// </summary>
        private Vector3 Compute3D(Vector3 eye, PointF p1, Vector3 eye2, PointF p2)
        {
            // Math ref: "Closest point between two rays" 
            // https://palitri.com/vault/stuff/maths/Rays%20closest%20point.pdf
            // 
            // Algorithm:
            // - Trace rays from the cameras through the points,
            // - Find the point on each ray closest to the other ray,
            // - Find the mid-point of the closest points.

            // Direction vectors from cameras to the points.
            Vector3 a = new Vector3(p1.X - eye.X, p1.Y - eye.Y, - eye.Z);
            a.Normalize();
            Vector3 b = new Vector3(p2.X - eye2.X, p2.Y - eye2.Y, - eye2.Z);
            b.Normalize();

            Vector3 c = eye2 - eye;

            Vector3 ab = Vector3.Cross(a, b);
            Vector3 bc = Vector3.Cross(b, c);
            Vector3 ac = Vector3.Cross(a, c);
            Vector3 bb = Vector3.Cross(b, b);
            Vector3 aa = Vector3.Cross(a, a);
            
            Vector3 denom = aa * bb - ab * ab;
            Vector3 num1 = ab * bc + ac * bb;
            num1.Negate();
            Vector3 d = eye + a * num1 / denom;
            Vector3 num2 = ab * ac - bc * aa;
            Vector3 e = eye2 + b * num2 / denom;
            
            Vector3 result = (d + e) / 2.0f;
            return result;
        }

        private List<string> GetCSV()
        {
            List<string> csv = new List<string>();
            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);
            string lengthUnit = UnitHelper.LengthAbbreviation(metadata.CalibrationHelper.LengthUnit);
            string timeUnit = UnitHelper.TimeAbbreviation(PreferencesManager.PlayerPreferences.TimecodeFormat);

            List<string> headers = new List<string>();
            headers.Add(CSVHelper.WriteCell("Name"));
            headers.Add(CSVHelper.WriteCell("X"));
            headers.Add(CSVHelper.WriteCell("Y"));
            headers.Add(CSVHelper.WriteCell("Z"));
            csv.Add(CSVHelper.MakeRow(headers, listSeparator));

            foreach (NamedPoint np in namedPoints)
            {
                List<string> row = new List<string>();
                row.Add(CSVHelper.WriteCell(np.Name));
                row.Add(CSVHelper.WriteCell(np.X, nfi));
                row.Add(CSVHelper.WriteCell(np.Y, nfi));
                row.Add(CSVHelper.WriteCell(np.Z, nfi));
                csv.Add(CSVHelper.MakeRow(row, listSeparator));
            }

            return csv;
        }

        /// <summary>
        /// Round a vector3 to 3 decimal places.
        /// </summary>
        private Vector3 Round3(Vector3 p)
        {
            return new Vector3((float)Math.Round(p.X, 3), (float)Math.Round(p.Y, 3), (float)Math.Round(p.Z, 3));
        }
        #endregion
    }
}
