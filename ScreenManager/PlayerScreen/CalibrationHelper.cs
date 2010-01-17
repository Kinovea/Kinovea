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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// CalibrationHelper encapsulates informations used for pixels to real world calculations.
	/// The user can specify the real distance of a Line drawing and a coordinate system.
	/// We also keep the length units and the preferred unit for speeds.
	/// </summary>
	public class CalibrationHelper
	{
		/// <summary>
		/// Standards units for distance, restricted to sports range. (No microscopic or macroscopic).
		/// </summary>
		public enum LengthUnits
		{
			Centimeters,
			Meters,
			Inches,
			Feet,
			Yards,
			Pixels					// Native unit.
		}
		
		/// <summary>
		/// Standards speed units.
		/// </summary>
		public enum SpeedUnits
		{
			MetersPerSecond,
			KilometersPerHour,
			MilesPerHour,
			Knots,
			PixelsPerFrame,			// Native unit. (might be useful for animation too).
		}
		
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
		public Point CoordinatesOrigin
		{
			get { return m_CoordinatesOrigin; }
			set { m_CoordinatesOrigin = value; }
		}
		#endregion
		
		#region Members
		private LengthUnits m_CurrentLengthUnit = LengthUnits.Pixels;
		private SpeedUnits m_CurrentSpeedUnit = SpeedUnits.PixelsPerFrame;
		private double m_fPixelToUnit = 1.0;
		private Point m_CoordinatesOrigin = new Point(-1,-1);
		#endregion
		
		#region Public Methods
		public static string GetAbbreviationFromUnit(LengthUnits _unit)
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
				case LengthUnits.Pixels:
				default:
					abbreviation = "px";
					break;
			}
			
			return abbreviation;
		}
		public string GetAbbreviation()
		{
			return GetAbbreviationFromUnit(m_CurrentLengthUnit);
		}
		public string GetLengthText(Point p1, Point p2)
		{
			// Return the length in the user unit, with the abbreviation.
			string lengthText = "";
			
			if(p1.X == p2.X && p1.Y == p2.Y)
			{
				lengthText = "0" + " " + GetAbbreviationFromUnit(m_CurrentLengthUnit);
			}
			else
			{
				if(m_CurrentLengthUnit == LengthUnits.Pixels)
				{
					lengthText = String.Format("{0:0} {1}", GetLengthDouble(p1, p2), GetAbbreviationFromUnit(m_CurrentLengthUnit));
				}
				else
				{
					lengthText = String.Format("{0:0.00} {1}", GetLengthDouble(p1, p2), GetAbbreviationFromUnit(m_CurrentLengthUnit));
				}
			}
			
			return lengthText;
		}
		public string GetLengthText(double _fPixelLength, bool _bAbbreviation, bool _bPrecise)
		{
			string lengthText = "";
			if(_bAbbreviation)
			{
				if(m_CurrentLengthUnit == LengthUnits.Pixels || !_bPrecise)
				{
					lengthText = String.Format("{0:0} {1}", GetLengthDouble(_fPixelLength), GetAbbreviationFromUnit(m_CurrentLengthUnit));
				}
				else
				{
					lengthText = String.Format("{0:0.00} {1}", GetLengthDouble(_fPixelLength), GetAbbreviationFromUnit(m_CurrentLengthUnit));
				}
			}
			else 
			{
				if(m_CurrentLengthUnit == LengthUnits.Pixels || !_bPrecise)
				{
					lengthText = String.Format("{0:0}", GetLengthDouble(_fPixelLength));
				}
				else
				{
					lengthText = String.Format("{0:0.00}", GetLengthDouble(_fPixelLength));
				}
			}
			
			return lengthText;
		}
		public double GetLengthDouble(Point p1, Point p2)
		{
			// Return the length in the user unit.
			double fUnitLength = 0;
			
			if(p1.X != p2.X || p1.Y != p2.Y)
			{
				double fPixelLength = Math.Sqrt(((p1.X - p2.X) * (p1.X - p2.X)) + ((p1.Y - p2.Y) * (p1.Y - p2.Y)));	
				
				fUnitLength = GetLengthDouble(fPixelLength);
			}
			
			return fUnitLength;
		}
		public double GetLengthDouble(double _fPixelLength )
		{
			// Return the length in the user unit.
			return _fPixelLength  * m_fPixelToUnit;
		}
		
		#endregion
	}
}
