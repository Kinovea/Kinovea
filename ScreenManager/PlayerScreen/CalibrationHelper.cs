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
using Kinovea.Services;
using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// CalibrationHelper encapsulates informations used for pixels to real world calculations.
	/// The user can specify the real distance of a Line drawing and a coordinate system.
	/// We also keep the length units and the preferred unit for speeds.
	/// 
	/// 
	/// </summary>
	public class CalibrationHelper
	{
		#region Properties		
		public LengthUnits CurrentLengthUnit 
		{
			get { return m_CurrentLengthUnit; }
			set { m_CurrentLengthUnit = value; }
		}		
		public double PixelToUnit 
		{
			get { return m_fPixelToUnit; }
			set { m_fPixelToUnit = value; }
		}
		public SpeedUnits CurrentSpeedUnit 
		{
			get { return m_CurrentSpeedUnit; }
			set { m_CurrentSpeedUnit = value; }
		}
		public bool IsOriginSet
		{
			get { return (m_CoordinatesOrigin.X >= 0 && m_CoordinatesOrigin.Y >= 0); }
		}
		public Point CoordinatesOrigin
		{
			get { return m_CoordinatesOrigin; }
			set { m_CoordinatesOrigin = value; }
		}
		public double FramesPerSeconds
		{
			// This is the frames per second as in real action reference. 
			// It takes high-speed camera into account, may be different than the video framerate.
			get { return m_fFramesPerSeconds; }
			set { m_fFramesPerSeconds = value; }
		}
		#endregion
		
		#region Members
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
		private LengthUnits m_CurrentLengthUnit = LengthUnits.Pixels;
		private SpeedUnits m_CurrentSpeedUnit = SpeedUnits.PixelsPerFrame;
		private double m_fPixelToUnit = 1.0;
		private Point m_CoordinatesOrigin = new Point(-1,-1);
		private double m_fFramesPerSeconds = 25;
		#endregion
		
		#region Constructor
		public CalibrationHelper()
		{
			PreferencesManager prefManager = PreferencesManager.Instance();
			m_CurrentSpeedUnit = prefManager.SpeedUnit;
		}
		#endregion
		
		#region Public Methods
		public static string GetLengthAbbreviationFromUnit(LengthUnits _unit)
		{
			string abbreviation = "";
			switch(_unit)
			{
				case LengthUnits.Centimeters:
					abbreviation = "cm";
					break;
				case LengthUnits.Meters:
					abbreviation = "m";
					break;
				case LengthUnits.Inches:
					abbreviation = "in";
					break;
				case LengthUnits.Feet:
					abbreviation = "ft";
					break;
				case LengthUnits.Yards:
					abbreviation = "yd";
					break;
                case LengthUnits.Percentage:
					abbreviation = "%";
					break;
				case LengthUnits.Pixels:
				default:
					abbreviation = "px";
					break;
			}
			
			return abbreviation;
		}
		public string GetLengthAbbreviation()
		{
			return GetLengthAbbreviationFromUnit(m_CurrentLengthUnit);
		}
		public string GetLengthText(double _pixelLength)
		{
			// Return the length in the user unit, with the abbreviation.
			string lengthText = String.Format("{0:0.00} {1}", GetLengthInUserUnit(_pixelLength), GetLengthAbbreviationFromUnit(m_CurrentLengthUnit));
			return lengthText;
		}
		public string GetLengthText(Point p1, Point p2)
		{
			// Return the length in the user unit, with the abbreviation.
			string lengthText = "";
			
			if(p1.X == p2.X && p1.Y == p2.Y)
				lengthText = "0" + " " + GetLengthAbbreviationFromUnit(m_CurrentLengthUnit);
			else
				lengthText = String.Format("{0:0.00} {1}", GetLengthInUserUnit(p1, p2), GetLengthAbbreviationFromUnit(m_CurrentLengthUnit));
			
			return lengthText;
		}
		public string GetLengthText(PointF p1, PointF p2)
		{
			// Return the length in the user unit, with the abbreviation.
			string lengthText = "";
			
			if(p1.X == p2.X && p1.Y == p2.Y)
				lengthText = "0" + " " + GetLengthAbbreviationFromUnit(m_CurrentLengthUnit);
			else
				lengthText = String.Format("{0:0.00} {1}", GetLengthInUserUnit(p1, p2), GetLengthAbbreviationFromUnit(m_CurrentLengthUnit));
			
			return lengthText;
		}
		
		public string GetLengthText(double _fPixelLength, bool _bAbbreviation, bool _bPrecise)
		{
			// Return length as a string. 
			string lengthText = "";
			if(_bAbbreviation)
			{
				if(!_bPrecise)
					lengthText = String.Format("{0:0} {1}", GetLengthInUserUnit(_fPixelLength), GetLengthAbbreviationFromUnit(m_CurrentLengthUnit));
				else
					lengthText = String.Format("{0:0.00} {1}", GetLengthInUserUnit(_fPixelLength), GetLengthAbbreviationFromUnit(m_CurrentLengthUnit));
			}
			else
			{
				if(!_bPrecise)
					lengthText = String.Format("{0:0}", GetLengthInUserUnit(_fPixelLength));
				else
					lengthText = String.Format("{0:0.00}", GetLengthInUserUnit(_fPixelLength));
			}
			
			return lengthText;
		}
		public double GetLengthInUserUnit(Point p1, Point p2)
		{
			if(p1.X == p2.X && p1.Y == p2.Y)
			    return 0;
			else
			    return GetLengthInUserUnit(GeometryHelper.GetDistance(p1, p2));
		}
		public double GetLengthInUserUnit(PointF p1, PointF p2)
		{
			if(p1.X == p2.X && p1.Y == p2.Y)
			    return 0;
			else
			    return GetLengthInUserUnit(GeometryHelper.GetDistance(p1, p2));
		}
		
		public double GetLengthInUserUnit(double _fPixelLength )
		{
			return _fPixelLength  * m_fPixelToUnit;
		}
		
		public PointF GetPointInUserUnit(Point p)
		{
			double fX = GetLengthInUserUnit(p.X - m_CoordinatesOrigin.X);
			double fY = GetLengthInUserUnit(m_CoordinatesOrigin.Y - p.Y);
			return new PointF((float)fX, (float)fY);
		}
		public string GetPointText(Point p, bool _bAbbreviation)
		{
			double fX = GetLengthInUserUnit(p.X - m_CoordinatesOrigin.X);
			double fY = GetLengthInUserUnit(m_CoordinatesOrigin.Y - p.Y);
			
			string pointText;
			if(m_CurrentLengthUnit == LengthUnits.Pixels)
				pointText = String.Format("{{{0:0};{1:0}}}", fX, fY);
			else
				pointText = String.Format("{{{0:0.00};{1:0.00}}}", fX, fY);
			
			if(_bAbbreviation)
				pointText = pointText + " " + GetLengthAbbreviation();
			
			return pointText;
		}
		
		public static string GetSpeedAbbreviationFromUnit(SpeedUnits _unit)
		{
			string abbreviation = "";
			switch(_unit)
			{
				case SpeedUnits.FeetPerSecond:
					abbreviation = "ft/s";
					break;
				case SpeedUnits.MetersPerSecond:
					abbreviation = "m/s";
					break;
				case SpeedUnits.KilometersPerHour:
					abbreviation = "km/h";
					break;
				case SpeedUnits.MilesPerHour:
					abbreviation = "mph";
					break;
				case SpeedUnits.Knots:
					abbreviation = "kn";
					break;
				case SpeedUnits.PixelsPerFrame:
				default:
					abbreviation = "px/f";
					break;
			}
			
			return abbreviation;
		}
		public string GetSpeedText(Point p1, Point p2, int frames)
		{
			// Return the speed in user units, with the abbreviation.
			
			string speedText = "";
			
			if((p1.X == p2.X && p1.Y == p2.Y) || frames == 0)
			{
				speedText = "0" + " " + GetSpeedAbbreviationFromUnit(m_CurrentSpeedUnit);
			}
			else
			{
				SpeedUnits unitToUse = m_CurrentSpeedUnit;
				
				if(m_CurrentLengthUnit == LengthUnits.Pixels && m_CurrentSpeedUnit != SpeedUnits.PixelsPerFrame)
				{
					// The user may have configured a preferred speed unit that we can't use because no 
					// calibration has been done on the video. In this case we use the px/f speed unit,
					// but we don't change the user's preference.
					unitToUse = SpeedUnits.PixelsPerFrame;
				}
				
				speedText = String.Format("{0:0.00} {1}", GetSpeedInUserUnit(p1, p2, frames, unitToUse), GetSpeedAbbreviationFromUnit(unitToUse));
			}

			return speedText;
		}
		#endregion
		
		#region Private methods
		private double GetSpeedInUserUnit(Point p1, Point p2, int frames, SpeedUnits _SpeedUnit)
		{
			// Return the speed in the current user unit.
			double fUnitSpeed = 0;
			
			if(p1.X != p2.X || p1.Y != p2.Y)
			{
				// Compute the length in pixels and send to converter.
				double fPixelLength = Math.Sqrt(((p1.X - p2.X) * (p1.X - p2.X)) + ((p1.Y - p2.Y) * (p1.Y - p2.Y)));	
				fUnitSpeed = GetSpeedInUserUnit(fPixelLength, frames, _SpeedUnit);
			}
			
			return fUnitSpeed;
		}
		private double GetSpeedInUserUnit(double _fPixelLength, int frames, SpeedUnits _SpeedUnit)
		{
			// Return the speed in the current user unit.
			
			// 1. Convert length from pixels to known distance.
			// (depends on user calibration from a line drawing)
			double fUnitLength = GetLengthInUserUnit(_fPixelLength);

			// 2. Convert between length user units (length to speed)
			// (depends only on standards conversion ratio)
			double fUnitLength2 = ConvertLengthForSpeedUnit(fUnitLength, m_CurrentLengthUnit, _SpeedUnit);
			
			// 3. Convert to speed unit.
			// (depends on video frame rate)
			double fUnitSpeed = ConvertToSpeedUnit(fUnitLength2, frames, _SpeedUnit);
			
			log.Debug(String.Format("Pixel conversion for speed. Input:{0:0.00} px for {1} frames. length1: {2:0.00} {3}, length2:{4:0.00} {5}, speed:{6:0.00} {7}",
			                        _fPixelLength, frames,
			                        fUnitLength, GetLengthAbbreviation(), 
			                        fUnitLength2, GetSpeedAbbreviationFromUnit(_SpeedUnit), 
			                        fUnitSpeed, GetSpeedAbbreviationFromUnit(_SpeedUnit)));
			
			return fUnitSpeed;
		}
		#endregion
		
		#region Converters
		private double ConvertLengthForSpeedUnit(double _fLength, LengthUnits _lengthUnit, SpeedUnits _speedUnits)
		{
			// Convert from one length unit to another.
			// For example: user calibrated the screen using centimeters and wants a speed in km/h.
			// We get a distance in centimeters, we convert it to kilometers.
			
			// http://en.wikipedia.org/wiki/Conversion_of_units
			// 1 inch 			= 0.0254 m.
			// 1 foot			= 0.3048 m.
			// 1 yard 			= 0.9144 m.
			// 1 mile 			= 1 609.344 m.
			// 1 nautical mile  = 1 852 m.
			
			double fLength2 = 0;
			
			switch(_lengthUnit)
			{
				case LengthUnits.Centimeters:
					switch(_speedUnits)
					{
						case SpeedUnits.FeetPerSecond:
							//  Centimeters to feet.
							fLength2 = _fLength / 30.48;
							break;
						case SpeedUnits.MetersPerSecond:
							//  Centimeters to meters.
							fLength2 = _fLength / 100;
							break;
						case SpeedUnits.KilometersPerHour:
							// Centimeters to kilometers.
							fLength2 = _fLength / 100000;
							break;
						case SpeedUnits.MilesPerHour:
							// Centimeters to miles 
							fLength2 = _fLength / 160934.4;
							break;
						case SpeedUnits.Knots:
							// Centimeters to nautical miles
							fLength2 = _fLength / 185200;
							break;
						case SpeedUnits.PixelsPerFrame:
						default:
							// Centimeters to Pixels. (?)
							// User has calibrated the image but now wants the speed in px/f.
							fLength2 = _fLength / m_fPixelToUnit;
							break;
					}
					break;
				case LengthUnits.Meters:
					switch(_speedUnits)
					{
						case SpeedUnits.FeetPerSecond:
							// Meters to feet.
							fLength2 = _fLength / 0.3048;
							break;
						case SpeedUnits.MetersPerSecond:
							// Meters to meters.
							fLength2 = _fLength;
							break;
						case SpeedUnits.KilometersPerHour:
							// Meters to kilometers.
							fLength2 = _fLength / 1000;
							break;
						case SpeedUnits.MilesPerHour:
							// Meters to miles.
							fLength2 = _fLength / 1609.344;
							break;
						case SpeedUnits.Knots:
							// Meters to nautical miles.
							fLength2 = _fLength / 1852;
							break;
						case SpeedUnits.PixelsPerFrame:
						default:
							// Meters to Pixels. (revert)
							fLength2 = _fLength / m_fPixelToUnit;
							break;
					}
					break;
				case LengthUnits.Inches:
					switch(_speedUnits)
					{
						case SpeedUnits.FeetPerSecond:
							// Inches to feet.
							fLength2 = _fLength / 12;
							break;
						case SpeedUnits.MetersPerSecond:
							// Inches to meters.
							fLength2 = _fLength / 39.3700787;
							break;
						case SpeedUnits.KilometersPerHour:
							// Inches to kilometers.
							fLength2 = _fLength / 39370.0787;
							break;
						case SpeedUnits.MilesPerHour:
							// Inches to miles.
							fLength2 = _fLength / 63360;
							break;
						case SpeedUnits.Knots:
							// Inches to nautical miles.
							fLength2 = _fLength / 72913.3858;
							break;
						case SpeedUnits.PixelsPerFrame:
						default:
							// Inches to Pixels. (revert)
							fLength2 = _fLength / m_fPixelToUnit;
							break;
					}
					break;
				case LengthUnits.Feet:
					switch(_speedUnits)
					{
						case SpeedUnits.FeetPerSecond:
							// Feet to feet.
							fLength2 = _fLength;
							break;
						case SpeedUnits.MetersPerSecond:
							// Feet to meters.
							fLength2 = _fLength / 3.2808399;
							break;
						case SpeedUnits.KilometersPerHour:
							// Feet to kilometers.
							fLength2 = _fLength / 3280.8399;
							break;
						case SpeedUnits.MilesPerHour:
							// Feet to miles.
							fLength2 = _fLength / 5280;
							break;
						case SpeedUnits.Knots:
							// Feet to nautical miles.
							fLength2 = _fLength / 6076.11549;
							break;
						case SpeedUnits.PixelsPerFrame:
						default:
							// Feet to Pixels. (revert)
							fLength2 = _fLength / m_fPixelToUnit;
							break;
					}
					break;
				case LengthUnits.Yards:
					switch(_speedUnits)
					{
						case SpeedUnits.FeetPerSecond:
							// Yards to feet.
							fLength2 = _fLength * 3;
							break;
						case SpeedUnits.MetersPerSecond:
							// Yards to meters.
							fLength2 = _fLength / 1.0936133;
							break;
						case SpeedUnits.KilometersPerHour:
							// Yards to kilometers.
							fLength2 = _fLength / 1093.6133;
							break;
						case SpeedUnits.MilesPerHour:
							// Yards to miles.
							fLength2 = _fLength / 1760;
							break;
						case SpeedUnits.Knots:
							// Yards to nautical miles.
							fLength2 = _fLength / 2025.37183;
							break;
						case SpeedUnits.PixelsPerFrame:
						default:
							// Yards to Pixels. (revert)
							fLength2 = _fLength / m_fPixelToUnit;
							break;
					}
					break;
				case LengthUnits.Pixels:
                case LengthUnits.Percentage:
				default:
					// If input length is in pixel, this means the image is not calibrated.
					// Unless the target speed unit is pixel per frame, we can't compute the speed.
					if(_speedUnits != SpeedUnits.PixelsPerFrame)
					{
						fLength2 = 0;
						log.Error("Can't compute speed : image is not calibrated and speed is required in real world units.");
					}
					else
					{
						fLength2 = _fLength;
					}
					break;
			}
			
			return fLength2;
		}
		private double ConvertToSpeedUnit(double _fRawSpeed, int frames, SpeedUnits _speedUnit)
		{
			// We now have the right length unit, but for the total time between the frames. (e.g: km/x frames).
			// Convert this to real world speed.
			// (depends on video frame rate).
			
			double fPerUserUnit = 0;
			
			// 1. per seconds
			double fPerSecond = (_fRawSpeed / frames ) * m_fFramesPerSeconds;
			
			// 2. To required speed
			switch(_speedUnit)
			{
				case SpeedUnits.FeetPerSecond:
				case SpeedUnits.MetersPerSecond:
					// To seconds.
					fPerUserUnit = fPerSecond;
					break;
				case SpeedUnits.KilometersPerHour:
				case SpeedUnits.MilesPerHour:
				case SpeedUnits.Knots:
					// To hours.
					fPerUserUnit = fPerSecond * 3600;
					break;
				case SpeedUnits.PixelsPerFrame:
				default:
					// To frames.
					fPerUserUnit = _fRawSpeed / frames;
					break;
			}
			
			return fPerUserUnit;
		}
		#endregion
	}
}
