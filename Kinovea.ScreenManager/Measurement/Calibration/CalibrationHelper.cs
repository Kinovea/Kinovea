﻿#region License
/*
Copyright © Joan Charmant 2009.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// CalibrationHelper encapsulates informations used for pixels to real world calculations.
    /// The user can specify the real distance of a Line drawing and a coordinate system.
    /// We also keep the length units and the preferred unit for speeds.
    /// </summary>
    public class CalibrationHelper
    {
        #region Events
        public event EventHandler CalibrationChanged;
        #endregion
        
        #region Properties
        public bool IsCalibrated
        {
            get { return lengthUnit != LengthUnit.Pixels; }
        }
        
        public LengthUnit LengthUnit
        {
            get { return lengthUnit; }
            set { lengthUnit = value;}
        }
        
        public SpeedUnit SpeedUnit
        {
            get { return speedUnit; }
            set { speedUnit = value;}
        }

        public AccelerationUnit AccelerationUnit
        {
            get { return accelerationUnit; }
            set { accelerationUnit = value; }
        }

        public AngleUnit AngleUnit
        {
            get { return angleUnit; }
            set { angleUnit = value; }
        }

        public AngularVelocityUnit AngularVelocityUnit
        {
            get { return angularVelocityUnit; }
            set { angularVelocityUnit = value; }
        }

        public AngularAccelerationUnit AngularAccelerationUnit
        {
            get { return angularAccelerationUnit; }
            set { angularAccelerationUnit = value; }
        }

        public double CaptureFramesPerSecond
        {
            // Frames per second, as in real action reference. (takes high speed camera into account.)
            get { return captureFramesPerSecond; }
            set 
            {
                captureFramesPerSecond = value;
                AfterCalibrationChanged();
            }
        }
        
        public CalibratorType CalibratorType
        {
            get { return calibratorType;}
        }

        public DistortionHelper DistortionHelper
        {
            get { return distortionHelper; }
        }

        public Size ImageSize
        {
            get { return imageSize; }
        }

        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= distortionHelper.ContentHash;
                hash ^= GetOrigin().GetHashCode();
                return hash;
            }
        }
        #endregion
        
        #region Members
        private bool initialized;
        private CalibratorType calibratorType = CalibratorType.Line;
        private ICalibrator calibrator;
        private CalibrationLine calibrationLine = new CalibrationLine();
        private CalibrationPlane calibrationPlane = new CalibrationPlane();
        private DistortionHelper distortionHelper = new DistortionHelper();
        private Size imageSize;
        private CoordinateSystemGrid coordinateSystemGrid;
        private LengthUnit lengthUnit = LengthUnit.Pixels;
        private SpeedUnit speedUnit = SpeedUnit.PixelsPerSecond;
        private AccelerationUnit accelerationUnit = AccelerationUnit.PixelsPerSecondSquared;
        private AngleUnit angleUnit = AngleUnit.Degree;
        private AngularVelocityUnit angularVelocityUnit = AngularVelocityUnit.DegreesPerSecond;
        private AngularAccelerationUnit angularAccelerationUnit = AngularAccelerationUnit.DegreesPerSecondSquared;
        private double captureFramesPerSecond = 25;
        private Func<long, PointF> getCalibrationOrigin;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public CalibrationHelper()
        {
            speedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
            accelerationUnit = PreferencesManager.PlayerPreferences.AccelerationUnit;
            angleUnit = PreferencesManager.PlayerPreferences.AngleUnit;
            angularVelocityUnit = PreferencesManager.PlayerPreferences.AngularVelocityUnit;
            angularAccelerationUnit = PreferencesManager.PlayerPreferences.AngularAccelerationUnit;
            calibrator = calibrationLine;
        }
        #endregion
        
        public void Initialize(Size imageSize, Func<long, PointF> getCalibrationOrigin)
        {
            this.imageSize = imageSize;
            SetOrigin(imageSize.Center());
            this.getCalibrationOrigin = getCalibrationOrigin;
            initialized = true;
            ComputeCoordinateSystemGrid();
        }

        public void Reset()
        {
            SetOrigin(imageSize.Center());
            calibratorType = CalibratorType.Line;
            calibrationLine = new CalibrationLine();
            calibrator = calibrationLine;

            distortionHelper = new DistortionHelper();

            lengthUnit = LengthUnit.Pixels;
            speedUnit = SpeedUnit.PixelsPerSecond;
            accelerationUnit = AccelerationUnit.PixelsPerSecondSquared;

            ComputeCoordinateSystemGrid();
        }
        
        /// <summary>
        /// Returns the origin of the coordinate system in image coordinates.
        /// </summary>
        public PointF GetOrigin()
        {
            return distortionHelper.Distort(calibrator.Untransform(PointF.Empty));
        }

        /// <summary>
        /// Takes a point in image coordinates to act as the origin of the current coordinate system.
        /// </summary>
        public void SetOrigin(PointF p)
        {
            PointF u = distortionHelper.Undistort(p);
            calibrator.SetOrigin(u);
            AfterCalibrationChanged();
        }

        public void SetCalibratorFromType(CalibratorType type)
        {
            // Used by calibration dialogs to force a calibration method for further computations.
            // Each time the user calibrates, we switch to the method he just used.
            calibratorType = type;

            switch (type)
            {
                case CalibratorType.Line:
                    calibrator = calibrationLine;
                    break;
                case CalibratorType.Plane:
                    calibrator = calibrationPlane;
                    break;
            }
        }
        
        public CoordinateSystemGrid GetCoordinateSystemGrid()
        {
            return coordinateSystemGrid;
        }

        public void AfterDistortionUpdated()
        {
            AfterCalibrationChanged();
        }

        #region Methods specific to a calibration technique
        public void CalibrationByLine_Initialize(float ratio)
        {
            calibrationLine.Initialize(ratio);
            AfterCalibrationChanged();
        }
        public void CalibrationByPlane_Initialize(SizeF size, QuadrilateralF quadImage)
        {
            QuadrilateralF undistorted = new QuadrilateralF(
                distortionHelper.Undistort(quadImage.A),
                distortionHelper.Undistort(quadImage.B),
                distortionHelper.Undistort(quadImage.C),
                distortionHelper.Undistort(quadImage.D));

            calibrationPlane.Initialize(size, undistorted);
            AfterCalibrationChanged();
        }
        public void CalibrationByPlane_Update(QuadrilateralF quadImage)
        {
            QuadrilateralF undistorted = new QuadrilateralF(
                distortionHelper.Undistort(quadImage.A),
                distortionHelper.Undistort(quadImage.B),
                distortionHelper.Undistort(quadImage.C),
                distortionHelper.Undistort(quadImage.D));

            calibrationPlane.Update(undistorted);
            AfterCalibrationChanged();
        }
        public SizeF CalibrationByPlane_GetRectangleSize()
        {
            // Real size of the calibration rectangle. Used to populate the calibration dialog.
            return calibrationPlane.Size;
        }
        public bool CalibrationByPlane_IsValid()
        {
            return calibrationPlane.Valid;
        }
        public CalibrationPlane CalibrationByPlane_GetCalibrator()
        {
            return calibrationPlane;
        }
        public ProjectiveMapping CalibrationByPlane_GetProjectiveMapping()
        {
            return calibrationPlane.ProjectiveMapping;
        }
        public QuadrilateralF CalibrationByPlane_GetProjectedQuad()
        {
            // Projection of the reference rectangle onto image space.
            // This is the quadrilateral defined by the user.
            return calibrationPlane.QuadImage;
        }
        #endregion

        #region Value computers
        /// <summary>
        /// Takes a point in image space and returns it in world space.
        /// </summary>
        public PointF GetPoint(PointF p)
        {
            return calibrator.Transform(distortionHelper.Undistort(p));
        }

        /// <summary>
        /// Takes a point in image space and returns it in world space, 
        /// based on the value of the coordinate system origin at the specified time.
        /// </summary>
        public PointF GetPointAtTime(PointF p, long time)
        {
            if (calibratorType != CalibratorType.Line)
                return GetPoint(p);

            PointF origin = getCalibrationOrigin(time);
            return calibrationLine.Transform(distortionHelper.Undistort(p), origin);
        }

        /// <summary>
        /// Takes a point in rectified image space and returns it in world space.
        /// </summary>
        public PointF GetPointFromRectified(PointF p)
        {
            return calibrator.Transform(p);
        }

        /// <summary>
        /// Takes a scalar in image space and returns a scalar in world space.
        /// Not suitable for geometry.
        /// </summary>
        public float GetScalar(float v)
        {
            PointF a = GetPoint(PointF.Empty);
            PointF b = GetPoint(new PointF(v, 0));

            float d = GeometryHelper.GetDistance(a, b);
            return v < 0 ? -d : d;
        }

        /// <summary>
        /// Takes an interval in frames and returns it in seconds.
        /// </summary>
        public float GetTime(int frames)
        {
            // TODO: have the function takes a number of timestamps instead for better accuracy.
            return (float)(frames / captureFramesPerSecond);
        }
        
        /// <summary>
        /// Takes a speed in length units/second and returns it in the current speed unit.
        /// </summary>
        public float ConvertSpeed(float v)
        {
            return UnitHelper.ConvertVelocity(v, lengthUnit, speedUnit);
        }

        /// <summary>
        /// Takes an acceleration in length units/second squared and return it in the acceleration unit.
        /// </summary>
        public float ConvertAcceleration(float a)
        {
            return UnitHelper.ConvertAcceleration(a, lengthUnit, accelerationUnit);
        }

        public float ConvertAccelerationFromVelocity(float a)
        {
            // Passed acceleration is expressed in units configured for velocity.
            float magnitude = UnitHelper.ConvertForLengthUnit(a, speedUnit, lengthUnit);
            return UnitHelper.ConvertAcceleration(magnitude, lengthUnit, accelerationUnit);
        }

        public float ConvertAngle(float radians)
        {
            return angleUnit == AngleUnit.Radian ? radians : (float)(radians * MathHelper.RadiansToDegrees);
        }
        public float ConvertAngleFromDegrees(float degrees)
        {
            return angleUnit == AngleUnit.Degree ? degrees : (float)(degrees * MathHelper.DegreesToRadians);
        }

        public float ConvertAngularVelocity(float radiansPerSecond)
        {
            return (float)UnitHelper.ConvertAngularVelocity(radiansPerSecond, angularVelocityUnit);
        }
        
        public float ConvertAngularAcceleration(float radiansPerSecondSquared)
        {
            return UnitHelper.ConvertAngularAcceleration(radiansPerSecondSquared, angularAccelerationUnit); 
        }
        #endregion

        #region Value as text
        public string GetPointText(PointF p, bool precise, bool abbreviation, long time)
        {
            PointF a = GetPointAtTime(p, time);
            
            string valueTemplate = precise ? "{0:0.00} ; {1:0.00}" : "{0:0} ; {1:0}";
            string text = String.Format(valueTemplate, a.X, a.Y);
            
            if(abbreviation)
                text = text + " " + String.Format("{0}", UnitHelper.LengthAbbreviation(lengthUnit));
            
            return text;
        }
        
        /// <summary>
        /// Takes two points in image coordinates and return the length of the segment in real world units.
        /// </summary>
        public string GetLengthText(PointF p1, PointF p2, bool precise, bool abbreviation)
        {
            float length = GeometryHelper.GetDistance(GetPoint(p1), GetPoint(p2));
            string valueTemplate = precise ? "{0:0.00}" : "{0:0}";
            string text = String.Format(valueTemplate, length);
            
            if(abbreviation)
                text = text + " " + String.Format("{0}", UnitHelper.LengthAbbreviation(lengthUnit));
            
            return text;
        }

        /// <summary>
        /// Takes the image space coordinates of the center of the circle and the image space coordinates of the point on the circle 
        /// that is along the X-axis in world space.
        /// Returns the circumference in world units.
        /// </summary>
        public string GetCircumferenceText(PointF p1, PointF p2, bool precise, bool abbreviation)
        {
            float length = GeometryHelper.GetDistance(GetPoint(p1), GetPoint(p2));
            string valueTemplate = precise ? "{0:0.00}" : "{0:0}";
            string text = String.Format(valueTemplate, 2 * Math.PI * length);

            if (abbreviation)
                text = text + " " + String.Format("{0}", UnitHelper.LengthAbbreviation(lengthUnit));

            return text;
        }

        public string GetLengthAbbreviation()
        {
            return UnitHelper.LengthAbbreviation(lengthUnit);
        }
        public string GetSpeedAbbreviation()
        {
            SpeedUnit unit = IsCalibrated ? speedUnit : SpeedUnit.PixelsPerSecond;
            return UnitHelper.SpeedAbbreviation(unit);
        }
        public string GetAccelerationAbbreviation()
        {
            AccelerationUnit unit = IsCalibrated ? accelerationUnit : AccelerationUnit.PixelsPerSecondSquared;
            return UnitHelper.AccelerationAbbreviation(unit);
        }
        public string GetAngleAbbreviation()
        {
            return UnitHelper.AngleAbbreviation(angleUnit);
        }
        public string GetAngularVelocityAbbreviation()
        {
            return UnitHelper.AngularVelocityAbbreviation(angularVelocityUnit);
        }
        public string GetAngularAccelerationAbbreviation()
        {
            return UnitHelper.AngularAccelerationAbbreviation(angularAccelerationUnit);
        }
        #endregion
         
        #region Inverse transformations (from calibrated space to image space).
        public float GetImageLength(PointF p1, PointF p2)
        {
            PointF a = distortionHelper.Distort(calibrator.Untransform(p1));
            PointF b = distortionHelper.Distort(calibrator.Untransform(p2));

            return GeometryHelper.GetDistance(a, b);
        }
        
        /// <summary>
        /// Takes a scalar value in world space and return it in image space.
        /// Not suitable for geometry.
        /// </summary>
        public float GetImageScalar(float v)
        {
            PointF a = distortionHelper.Distort(calibrator.Untransform(PointF.Empty));
            PointF b = distortionHelper.Distort(calibrator.Untransform(new PointF(v, 0)));
            
            float d = GeometryHelper.GetDistance(a, b);
            return v < 0 ? -d : d;
        }

        /// <summary>
        /// Takes a point in real world coordinates and returns it in image coordinates.
        /// </summary>
        public PointF GetImagePoint(PointF p)
        {
            return distortionHelper.Distort(calibrator.Untransform(p));
        }

        /// <summary>
        /// Takes a circle in real world coordinates and returns a cooresponding ellipse in image coordinates.
        /// </summary>
        public Ellipse GetEllipseFromCircle(Circle circle)
        {
            PointF center = circle.Center;
            float radius = circle.Radius;
            
            if(calibratorType == CalibratorType.Line)
                return new Ellipse(GetImagePoint(center), GetImageScalar(radius), GetImageScalar(radius), 0);
            
            // Get the square enclosing the circle for mapping.
            PointF a = GetImagePoint(center.Translate(-radius, -radius));
            PointF b = GetImagePoint(center.Translate(radius, -radius));
            PointF c = GetImagePoint(center.Translate(radius, radius));
            PointF d = GetImagePoint(center.Translate(-radius, radius));
            QuadrilateralF quadImage = new QuadrilateralF(a, b, c, d);

            ProjectiveMapping mapping = new ProjectiveMapping();
            mapping.Update(QuadrilateralF.CenteredUnitSquare, quadImage);
            return mapping.Ellipse();
        }

        /// <summary>
        /// Takes a circle in image space and returns a cooresponding ellipse in image space.
        /// This is an ill posed problem. The center is respected but the radius could be taken 
        /// anywhere around the circle in image space, and that yields different radii in world space.
        /// We use the point along the X axis as the radius.
        /// </summary>
        public Ellipse GetEllipseFromCircle(PointF center, float radius, out PointF radiusLeftInImage, out PointF radiusRightInImage)
        {
            radiusLeftInImage = new PointF(center.X - radius, center.Y);
            radiusRightInImage = new PointF(center.X + radius, center.Y);

            if (calibratorType == CalibratorType.Line)
                return new Ellipse(center, radius, radius, 0);

            // Rebuild the world-space circle based on center and radius alone.
            PointF centerInWorld = GetPoint(center);

            // Estimate the radius in world space.
            // Get scalar will assumes reference direction to be X-axis in image space.
            float radiusInWorld = GetScalar(radius);

            // Get the intersection points of a horizontal diameter.
            // This is used to draw a line from the center to the outline of the ellipse in perspective.
            radiusLeftInImage = GetImagePoint(centerInWorld.Translate(-radiusInWorld, 0));
            radiusRightInImage = GetImagePoint(centerInWorld.Translate(radiusInWorld, 0));

            // Get the square enclosing the circle for mapping.
            PointF a = GetImagePoint(centerInWorld.Translate(-radiusInWorld, -radiusInWorld));
            PointF b = GetImagePoint(centerInWorld.Translate(radiusInWorld, -radiusInWorld));
            PointF c = GetImagePoint(centerInWorld.Translate(radiusInWorld, radiusInWorld));
            PointF d = GetImagePoint(centerInWorld.Translate(-radiusInWorld, radiusInWorld));
            QuadrilateralF quadImage = new QuadrilateralF(a, b, c, d);
            
            ProjectiveMapping mapping = new ProjectiveMapping();
            mapping.Update(QuadrilateralF.CenteredUnitSquare, quadImage);
            return mapping.Ellipse();
        }
        #endregion

        #region Serialization
        public void WriteXml(XmlWriter w)
        {
            if(calibratorType == CalibratorType.Line)
            {
                w.WriteStartElement("CalibrationLine");
                calibrationLine.WriteXml(w);
                w.WriteEndElement();
            }
            else if(calibratorType == CalibratorType.Plane)
            {   
                w.WriteStartElement("CalibrationPlane");                
                calibrationPlane.WriteXml(w);
                w.WriteEndElement();
            }

            w.WriteStartElement("Unit");
            w.WriteAttributeString("Abbreviation", GetLengthAbbreviation());
            w.WriteString(lengthUnit.ToString());
            w.WriteEndElement();

            DistortionSerializer.Serialize(w, distortionHelper.Parameters, false, imageSize);
        }
        public void ReadXml(XmlReader r, PointF scale, Size imageSize)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "CalibrationPlane":
                        calibratorType = CalibratorType.Plane;
                        calibrator = calibrationPlane;
                        calibrationPlane.ReadXml(r, scale);
                        ComputeCoordinateSystemGrid();
                        break;
                    case "CalibrationLine":
                        calibratorType = CalibratorType.Line;
                        calibrator = calibrationLine;
                        calibrationLine.ReadXml(r, scale);
                        break;
                    case "Unit":
                        lengthUnit = (LengthUnit) Enum.Parse(typeof(LengthUnit), r.ReadElementContentAsString());
                        break;
                    case "CameraCalibration":
                        DistortionParameters parameters = DistortionSerializer.Deserialize(r, imageSize);
                        distortionHelper.Initialize(parameters, imageSize);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();

            AfterCalibrationChanged();
        }
        #endregion

        #region Private helpers
        private void AfterCalibrationChanged()
        {
            ComputeCoordinateSystemGrid();

            if (CalibrationChanged != null)
                CalibrationChanged(this, EventArgs.Empty);
        }
        private void ComputeCoordinateSystemGrid()
        {
            if (!initialized)
                return;

            coordinateSystemGrid = CoordinateSystemGridFinder.Find(this);
        }
        #endregion
    }
}
