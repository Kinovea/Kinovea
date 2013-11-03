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
        
        private LengthUnit lengthUnit = LengthUnit.Pixels;
        private SpeedUnit speedUnit = SpeedUnit.PixelsPerFrame;
        private AccelerationUnit accelerationUnit = AccelerationUnit.PixelsPerFrameSquared;
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
        
        #region Methods specific to a calibration technique
        public void SetCalibratorFromType(CalibratorType type)
        {
            calibratorType = type;
            
            switch(type)
            {
                case CalibratorType.Line:
                    calibrator = calibrationLine;
                    break;
                case CalibratorType.Plane:
                    calibrator = calibrationPlane;
                    break;
            }
        }
        public void CalibrationByLine_SetPixelToUnit(float ratio)
        {
            calibrationLine.SetPixelToUnit(ratio);

            if (CalibrationChanged != null)
                CalibrationChanged(this, EventArgs.Empty);
        }
        public void CalibrationByLine_SetOrigin(PointF p)
        {
            calibrationLine.SetOrigin(p);
            if (CalibrationChanged != null)
                CalibrationChanged(this, EventArgs.Empty);
        }
        public PointF CalibrationByLine_GetOrigin()
        {
            return calibrationLine.Origin;
        }
        public bool CalibrationByLine_GetIsOriginSet()
        {
            return calibrationLine.IsOriginSet;
        }
        public void CalibrationByPlane_Initialize(SizeF size, QuadrilateralF quadImage)
        {
            calibrationPlane.Initialize(size, quadImage);
            if (CalibrationChanged != null)
                CalibrationChanged(this, EventArgs.Empty);
        }
        public SizeF CalibrationByPlane_GetRectangleSize()
        {
            return calibrationPlane.Size;
        }
        
        #endregion
        
        #region Value computers
        public float GetScalar(float v)
        {
            PointF a = calibrator.Transform(PointF.Empty);
            PointF b = calibrator.Transform(new PointF(v, 0));
            float d = GeometryHelper.GetDistance(a, b);
            return v < 0 ? -d : d;
        }

        public PointF GetPoint(PointF p)
        {
            return calibrator.Transform(p);
        }
        
        public float GetLength(PointF p1, PointF p2)
        {
            PointF a = calibrator.Transform(p1);
            PointF b = calibrator.Transform(p2);
            return GeometryHelper.GetDistance(a, b);
        }

        public float GetSpeed(PointF p0, PointF p1, int dt, Component component)
        {
            // px/f
            float v = GetSpeedPixelsPerFrame(p0, p1, dt, component);

            // calibrated length unit/f
            float v2 = GetScalar(v);

            // speed unit. (e.g: m/s). If the user hasn't calibrated, force usage of px/f.
            SpeedUnit unit = IsCalibrated ? speedUnit : SpeedUnit.PixelsPerFrame;
            double v3 = UnitHelper.ConvertVelocity(v2, framesPerSecond, lengthUnit, unit);

            return (float)v3;
        }

        private float GetSpeedPixelsPerFrame(PointF p0, PointF p1, int dt, Component component)
        {
            // In pixels per frame.
            float d = 0F;
            switch (component)
            {
                case Component.Magnitude:
                    d = GeometryHelper.GetDistance(p0, p1);
                    break;
                case Component.Horizontal:
                    d = p1.X - p0.X;
                    break;
                case Component.Vertical:
                    d = p1.Y - p0.Y;
                    d = -d;
                    break;
            }

            float v = d / dt;
            return v;
        }

        public float GetAcceleration(PointF p0, PointF p2, int interval1, PointF p1, PointF p3, int interval2, int interval3, Component component)
        {
            // px/f²
            float v1 = GetSpeedPixelsPerFrame(p0, p1, interval1, component);
            float v2 = GetSpeedPixelsPerFrame(p2, p3, interval2, component);
            float a = (v2 - v1) / interval3;

            // calibrated length unit/f²
            float a2 = GetScalar(a);

            // acceleration unit. (e.g: m/s²). If the user hasn't calibrated, force usage of px/f².
            AccelerationUnit unit = IsCalibrated ? accelerationUnit : AccelerationUnit.PixelsPerFrameSquared;
            double a3 = UnitHelper.ConvertAcceleration(a2, framesPerSecond, lengthUnit, unit);

            return (float)a3;
        }

        public float GetAngle(float radians)
        {
            return angleUnit == AngleUnit.Radian ? radians : (float)(radians * MathHelper.RadiansToDegrees);
        }

        public float GetAngularVelocity(float radiansPerFrame)
        {
            return (float)UnitHelper.ConvertAngularVelocity(radiansPerFrame, framesPerSecond, angularVelocityUnit);            
        }
        
        public float GetTangentialVelocity(float radiansPerFrame, float radius)
        {
            float pixelsPerFrame = radiansPerFrame * radius;

            float lengthPerFrame = GetScalar(pixelsPerFrame);

            // speed unit. (e.g: m/s). If the user hasn't calibrated, force usage of px/f.
            SpeedUnit unit = IsCalibrated ? speedUnit : SpeedUnit.PixelsPerFrame;
            double velocity = UnitHelper.ConvertVelocity(lengthPerFrame, framesPerSecond, lengthUnit, unit);

            return (float)velocity;
        }

        public float GetCentripetalAcceleration(float radiansPerFrame, float radius)
        {
            float value = radiansPerFrame * radiansPerFrame * radius;
            return (float)UnitHelper.ConvertAngularAcceleration(value, framesPerSecond, angularAccelerationUnit);
        }
        public float GetAngularAcceleration(float radiansPerFrameSquared)
        {
            return (float)UnitHelper.ConvertAngularAcceleration(radiansPerFrameSquared, framesPerSecond, angularAccelerationUnit); 
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
            float length = GetLength(p1, p2);
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
            SpeedUnit unit = IsCalibrated ? speedUnit : SpeedUnit.PixelsPerFrame;
            return UnitHelper.SpeedAbbreviation(unit);
        }
        public string GetAccelerationAbbreviation()
        {
            AccelerationUnit unit = IsCalibrated ? accelerationUnit : AccelerationUnit.PixelsPerFrameSquared;
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

        public PointF GetImagePoint(PointF p)
        {
            return calibrator.Untransform(p);
        }

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
    }
}
