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
        private double framesPerSecond = 25;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public CalibrationHelper()
        {
            speedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
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
        }
        public void CalibrationByLine_SetOrigin(PointF p)
        {
            calibrationLine.SetOrigin(p);
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
        }
        public SizeF CalibrationByPlane_GetRectangleSize()
        {
            return calibrationPlane.Size;
        }
        
        #endregion
        
        #region Value extractors
        public string GetLengthText(PointF p1, PointF p2, bool precise, bool abbreviation)
        {
            float length = GetLength(p1, p2);
            string valueTemplate = precise ? "{0:0.00}" : "{0:0}";
            string text = String.Format(valueTemplate, length);
            
            if(abbreviation)
                text = text + " " + String.Format("{0}", UnitHelper.LengthAbbreviation(lengthUnit));
            
            return text;
        }
        
        public float GetLength(PointF p1, PointF p2)
        {
            PointF a = calibrator.Transform(p1);
            PointF b = calibrator.Transform(p2);
            return GeometryHelper.GetDistance(a, b);
        }
        
        public string GetPointText(PointF p, bool precise, bool abbreviation)
        {
            PointF a = GetPoint(p);
            
            string valueTemplate = precise ? "{{{0:0.00};{1:0.00}}}" : "{{{0:0};{1:0}}}";
            string text = String.Format(valueTemplate, a.X, a.Y);
            
            if(abbreviation)
                text = text + " " + String.Format("{0}", UnitHelper.LengthAbbreviation(lengthUnit));
            
            return text;
        }
        
        public PointF GetPoint(PointF p)
        {
            return calibrator.Transform(p);
        }
        
        public string GetSpeedText(PointF p1, PointF p2, int frames)
        {
            if((p1.X == p2.X && p1.Y == p2.Y) || frames == 0)
                return "0" + " " + UnitHelper.SpeedAbbreviation(speedUnit);
            
            float length = GetLength(p1, p2);
            
            // The user may have configured a preferred speed unit but not done any space calibration. Force use of px/f.
            SpeedUnit unit = (lengthUnit == LengthUnit.Pixels && speedUnit != SpeedUnit.PixelsPerFrame) ? SpeedUnit.PixelsPerFrame : speedUnit;
            
            // Convert distance from length units to speed units, in case the user calibrated space in cm but want speed in m/s for example.
            double length2 = UnitHelper.ConvertLengthForSpeedUnit(length, lengthUnit, unit);
            
            double speed = UnitHelper.GetSpeed(length2, frames, framesPerSecond, unit);
            
            string text = String.Format("{0:0.00} {1}", speed, UnitHelper.SpeedAbbreviation(unit));
            return text;
        }
        #endregion
        
        #region Inverse transformations (from calibrated space to image space).
        public float GetImageLength(PointF p1, PointF p2)
        {
            PointF a = calibrator.Untransform(p1);
            PointF b = calibrator.Untransform(p2);
            return GeometryHelper.GetDistance(a, b);
        }
        
        public PointF GetImagePoint(PointF p)
        {
            return calibrator.Untransform(p);
        }
        #endregion
        
        public string GetLengthAbbreviation()
        {
            return UnitHelper.LengthAbbreviation(lengthUnit);
        }
       
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
