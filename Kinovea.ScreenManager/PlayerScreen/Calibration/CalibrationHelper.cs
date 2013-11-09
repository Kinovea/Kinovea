#region License
/*
Copyright © Joan Charmant 2009.
joan.charmant@gmail.com 
 
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

        public double FramesPerSecond
        {
            // Frames per second, as in real action reference. (takes high speed camera into account.)
            get { return framesPerSecond; }
            set { framesPerSecond = value; }
        }
        
        public CalibratorType CalibratorType
        {
            get { return calibratorType;}
        }
        #endregion
        
        #region Members
        private CalibratorType calibratorType = CalibratorType.Line;
        private ICalibrator calibrator;
        private CalibrationLine calibrationLine = new CalibrationLine();
        private CalibrationPlane calibrationPlane = new CalibrationPlane();
        private Size imageSize;
        private RectangleF boundingRectangle;
        private LengthUnit lengthUnit = LengthUnit.Pixels;
        private SpeedUnit speedUnit = SpeedUnit.PixelsPerSecond;
        private AccelerationUnit accelerationUnit = AccelerationUnit.PixelsPerSecondSquared;
        private AngleUnit angleUnit = AngleUnit.Degree;
        private AngularVelocityUnit angularVelocityUnit = AngularVelocityUnit.DegreesPerSecond;
        private AngularAccelerationUnit angularAccelerationUnit = AngularAccelerationUnit.DegreesPerSecondSquared;
        private double framesPerSecond = 25;
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
        
        public void Initialize(Size imageSize)
        {
            this.imageSize = imageSize;
            SetOrigin(imageSize.Center());
        }
        
        /// <summary>
        /// Returns the origin of the coordinate system in image coordinates.
        /// </summary>
        public PointF GetOrigin()
        {
            return calibrator.Untransform(PointF.Empty);
        }

        /// <summary>
        /// Takes a point in image coordinates to act as the origin of the current coordinate system.
        /// </summary>
        public void SetOrigin(PointF p)
        {
            calibrator.SetOrigin(p);
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
        
        /// <summary>
        /// Returns best candidates for real world coordinates of the corners of the image.
        /// </summary>
        public RectangleF GetBoundingRectangle()
        {
            return boundingRectangle;
        }

        #region Methods specific to a calibration technique
        public void CalibrationByLine_Initialize(float ratio)
        {
            calibrationLine.Initialize(ratio);
            AfterCalibrationChanged();
        }
        public void CalibrationByPlane_Initialize(SizeF size, QuadrilateralF quadImage)
        {
            calibrationPlane.Initialize(size, quadImage);
            AfterCalibrationChanged();
        }
        public SizeF CalibrationByPlane_GetRectangleSize()
        {
            // Real size of the calibration rectangle. Used to populate the calibration dialog.
            return calibrationPlane.Size;
        }
        #endregion

        #region Value computers
        /// <summary>
        /// Takes a point in image coordinates and returns it in real world coordinates.
        /// </summary>
        public PointF GetPoint(PointF p)
        {
            return calibrator.Transform(p);
        }

        /// <summary>
        /// Takes an interval in frames and returns it in seconds.
        /// </summary>
        public float GetTime(int frames)
        {
            // TODO: have the function takes a number of timestamps instead for better accuracy.
            return (float)(frames / framesPerSecond);
        }
        
        /// <summary>
        /// Takes a speed in calibration units/seconds and returns it in the current speed unit.
        /// </summary>
        public float ConvertSpeed(float v)
        {
            return UnitHelper.ConvertVelocity(v, lengthUnit, speedUnit);
        }

        public float ConvertAcceleration(float a)
        {
            return UnitHelper.ConvertAcceleration(a, lengthUnit, accelerationUnit);
        }
        
        public float ConvertAngle(float radians)
        {
            return angleUnit == AngleUnit.Radian ? radians : (float)(radians * MathHelper.RadiansToDegrees);
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
        public string GetPointText(PointF p, bool precise, bool abbreviation)
        {
            // TODO: remove this function in favore of getting the raw value and formatting in the caller ?
            PointF a = GetPoint(p);
            
            string valueTemplate = precise ? "{{{0:0.00};{1:0.00}}}" : "{{{0:0};{1:0}}}";
            string text = String.Format(valueTemplate, a.X, a.Y);
            
            if(abbreviation)
                text = text + " " + String.Format("{0}", UnitHelper.LengthAbbreviation(lengthUnit));
            
            return text;
        }
        
        public string GetLengthText(PointF p1, PointF p2, bool precise, bool abbreviation)
        {
            float length = GeometryHelper.GetDistance(GetPoint(p1), GetPoint(p2));
            string valueTemplate = precise ? "{0:0.00}" : "{0:0}";
            string text = String.Format(valueTemplate, length);
            
            if(abbreviation)
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
            PointF a = calibrator.Untransform(p1);
            PointF b = calibrator.Untransform(p2);
            return GeometryHelper.GetDistance(a, b);
        }
        
        public float GetImageScalar(float v)
        {
            PointF a = calibrator.Untransform(PointF.Empty);
            PointF b = calibrator.Untransform(new PointF(v, 0));
            float d = GeometryHelper.GetDistance(a, b);
            return v < 0 ? -d : d;
        }

        /// <summary>
        /// Takes a point in real world coordinates and returns it in image coordinates.
        /// </summary>
        public PointF GetImagePoint(PointF p)
        {
            return calibrator.Untransform(p);
        }

        /// <summary>
        /// Takes a circle in real world coordinates and returns a cooresponding ellipse in image coordinates.
        /// </summary>
        public Ellipse GetEllipseFromCircle(PointF center, float radius)
        {
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
        }
        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "CalibrationPlane":
                        calibratorType = CalibratorType.Plane;
                        calibrator = calibrationPlane;
                        calibrationPlane.ReadXml(r);
                        ComputeBoundingRectangle();
                        break;
                    case "CalibrationLine":
                        calibratorType = CalibratorType.Line;
                        calibrator = calibrationLine;
                        calibrationLine.ReadXml(r);
                        break;
                    case "Unit":
                        lengthUnit = (LengthUnit) Enum.Parse(typeof(LengthUnit), r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();
        }
        #endregion

        #region Private helpers
        private void AfterCalibrationChanged()
        {
            ComputeBoundingRectangle();

            if (CalibrationChanged != null)
                CalibrationChanged(this, EventArgs.Empty);
        }
        private void ComputeBoundingRectangle()
        {
            // Tries to find a rectangle in real world coordinates corresponding to the image corners.
            // This is used by coordinate systems to find a good filling of the image plane for drawing the grid.
            // The result is given back in real world coordinates.

            if (calibratorType == CalibratorType.Line)
            {
                PointF a = calibrator.Transform(PointF.Empty);
                PointF b = calibrator.Transform(new PointF(imageSize.Width, 0));
                PointF c = calibrator.Transform(new PointF(imageSize.Width, imageSize.Height));
                PointF d = calibrator.Transform(new PointF(0, imageSize.Height));
                boundingRectangle = new RectangleF(a.X, a.Y, b.X - a.X, a.Y - d.Y);
            }
            else
            {
                // Redo the user mapping but use the bounding box of the user quadrilateral instead of the quadrilateral itself.
                // This way we are sure the image corners have real world equivalent.
                RectangleF bbox = calibrationPlane.QuadImage.GetBoundingBox();
                QuadrilateralF quadImage = new QuadrilateralF(bbox);

                CalibrationPlane calibrationPlane2 = new CalibrationPlane();
                calibrationPlane2.Initialize(calibrationPlane.Size, quadImage);
                calibrationPlane2.SetOrigin(calibrationPlane.Untransform(PointF.Empty));

                PointF a = calibrationPlane2.Transform(PointF.Empty);
                PointF b = calibrationPlane2.Transform(new PointF(imageSize.Width, 0));
                PointF c = calibrationPlane2.Transform(new PointF(imageSize.Width, imageSize.Height));
                PointF d = calibrationPlane2.Transform(new PointF(0, imageSize.Height));
                boundingRectangle = new RectangleF(a.X, a.Y, b.X - a.X, a.Y - d.Y);
            }
        }
        #endregion
    }
}
