#region License
/*
Copyright © Joan Charmant 2012.
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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class UnitHelper
    {
        public static string LengthAbbreviation(LengthUnit unit)
		{
			string result = "";
			switch(unit)
			{
			    case LengthUnit.Millimeters:
			        result = "mm";
			        break;
				case LengthUnit.Centimeters:
					result = "cm";
					break;
				case LengthUnit.Meters:
					result = "m";
					break;
				case LengthUnit.Inches:
					result = "in";
					break;
				case LengthUnit.Feet:
					result = "ft";
					break;
				case LengthUnit.Yards:
					result = "yd";
					break;
                case LengthUnit.Percentage:
					result = "%";
					break;
				case LengthUnit.Pixels:
				default:
					result = "px";
					break;
			}
			
			return result;
		}
        
        public static string SpeedAbbreviation(SpeedUnit unit)
		{
			string abbreviation = "";
			switch(unit)
			{
				case SpeedUnit.FeetPerSecond:
					abbreviation = "ft/s";
					break;
				case SpeedUnit.MetersPerSecond:
					abbreviation = "m/s";
					break;
				case SpeedUnit.KilometersPerHour:
					abbreviation = "km/h";
					break;
				case SpeedUnit.MilesPerHour:
					abbreviation = "mph";
					break;
				case SpeedUnit.Knots:
					abbreviation = "kn";
					break;
				case SpeedUnit.PixelsPerFrame:
				default:
					abbreviation = "px/f";
					break;
			}
			
			return abbreviation;
		}
        
		public static double ConvertLengthForSpeedUnit(double length, LengthUnit lengthUnit, SpeedUnit speedUnits)
		{
			// Convert from one length unit to another.
			// We first convert from whatever unit into meters, then from meters to the output.
			// The scenario where the space is calibrated but the user wants the speed in pixels is not supported.
			
			if(lengthUnit == LengthUnit.Pixels || lengthUnit == LengthUnit.Percentage)
			    return speedUnits == SpeedUnit.PixelsPerFrame ? length : 0;
			
			// http://en.wikipedia.org/wiki/Conversion_of_units
			// 1 inch 			= 0.0254 m.
			// 1 foot			= 0.3048 m.
			// 1 yard 			= 0.9144 m.
			// 1 mile 			= 1 609.344 m.
			// 1 nautical mile  = 1 852 m.
			
			const double millimeterToMeters = 0.001;
			const double centimeterToMeters = 0.01;
			const double inchToMeters = 0.0254;
			const double footToMeters = 0.3048;
			const double yardToMeters = 0.9144;
			const double kilometerToMeters = 1000;
			const double mileToMeters = 1609.344;
			const double nauticalMileToMeters = 1852;
			
			double meters = 0;
			
			switch(lengthUnit)
			{
			    case LengthUnit.Millimeters:
			        meters = length * millimeterToMeters;
			        break;
			    case LengthUnit.Centimeters:
			        meters = length * centimeterToMeters;
			        break;
			    case LengthUnit.Meters:
			        meters = length;
			        break;
			    case LengthUnit.Inches:
			        meters = length * inchToMeters;
			        break;
			    case LengthUnit.Feet:
			        meters = length * footToMeters;
			        break;
			    case LengthUnit.Yards:
			        meters = length * yardToMeters;
			        break;
			}
			
            double result = 0;
			
            switch(speedUnits)
			{
                case SpeedUnit.FeetPerSecond:
                    result = meters / footToMeters;
                    break;
                case SpeedUnit.MetersPerSecond:
                    result = meters;
                    break;
                case SpeedUnit.KilometersPerHour:
                    result = meters / kilometerToMeters;
                    break;
                case SpeedUnit.MilesPerHour:
                    result = meters / mileToMeters;
                    break;
                case SpeedUnit.Knots:
                    result = meters / nauticalMileToMeters;
                    break;
                case SpeedUnit.PixelsPerFrame:
                default:
                    result = 0;
                    break;
			}
			
			return result;
		}
		
		public static double GetSpeed(double length, int frames, double framesPerSecond, SpeedUnit speedUnit)
		{
		    // Length is given in the right unit.
			double result = 0;
			double perSecond = (length / frames ) * framesPerSecond;
			
			// Convert to expected time unit.
			switch(speedUnit)
			{
				case SpeedUnit.FeetPerSecond:
				case SpeedUnit.MetersPerSecond:
					// To seconds.
					result = perSecond;
					break;
				case SpeedUnit.KilometersPerHour:
				case SpeedUnit.MilesPerHour:
				case SpeedUnit.Knots:
					// To hours.
					result = perSecond * 3600;
					break;
				case SpeedUnit.PixelsPerFrame:
				default:
					// To frames.
					result = length / frames;
					break;
			}
			
			return result;
		}        
    }
}
