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

namespace Videa.ScreenManager
{
	/// <summary>
	/// LineLengthHelper encapsulate the conversion between pixels and 
	/// the current real world unit specified by the user.
	/// It also holds the current ratio for the unit and the choosen unit itself.
	/// </summary>
	public class LineLengthHelper
	{
		/// <summary>
		/// A set of units. We do not propose microscopic units here.
		/// </summary>
		public enum LengthUnits
		{
			Centimeters,
			Meters,
			Inches,
			Feet,
			Yards,
			Pixels
		}
		
		#region Properties		
		public LengthUnits CurrentUnit 
		{
			get { return m_CurrentUnit; }
			set { m_CurrentUnit = value; }
		}		
		public double PixelToUnit 
		{
			get { return m_fPixelToUnit; }
			set { m_fPixelToUnit = value; }
		}		
		#endregion
		
		#region Members
		private LengthUnits m_CurrentUnit = LengthUnits.Pixels;
		private double m_fPixelToUnit = 1.0;
		#endregion
		
		#region Public Methods
		public static string GetAbbreviation(LengthUnits _unit)
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
		public string GetLengthText(Point p1, Point p2)
		{
			// Return the length in the user unit, with the abbreviation.
			string lengthText = "";
			
			if(p1.X == p2.X && p1.Y == p2.Y)
			{
				lengthText = "0" + " " + GetAbbreviation(m_CurrentUnit);
			}
			else
			{
            	lengthText = String.Format("{0:0.00} {1}", GetLengthDouble(p1, p2), GetAbbreviation(m_CurrentUnit));
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
				
				fUnitLength = fPixelLength * m_fPixelToUnit;
			}
			
			return fUnitLength;
		}
		
		#endregion
	}
}
