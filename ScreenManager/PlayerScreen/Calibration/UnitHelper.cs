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
        public static string LengthAbbreviation(LengthUnits unit)
		{
			string result = "";
			switch(unit)
			{
				case LengthUnits.Centimeters:
					result = "cm";
					break;
				case LengthUnits.Meters:
					result = "m";
					break;
				case LengthUnits.Inches:
					result = "in";
					break;
				case LengthUnits.Feet:
					result = "ft";
					break;
				case LengthUnits.Yards:
					result = "yd";
					break;
                case LengthUnits.Percentage:
					result = "%";
					break;
				case LengthUnits.Pixels:
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
        
		public static double ConvertLengthForSpeedUnit(double length, LengthUnits lengthUnit, SpeedUnit speedUnits)
		{
			// Convert from one length unit to another.
			// For example: user calibrated the screen using centimeters and wants a speed in km/h.
			// We get a distance in centimeters, we convert it to kilometers.
			// The scenario where the space is calibrated but the user wants the speed in pixels is not supported.
			
			// http://en.wikipedia.org/wiki/Conversion_of_units
			// 1 inch 			= 0.0254 m.
			// 1 foot			= 0.3048 m.
			// 1 yard 			= 0.9144 m.
			// 1 mile 			= 1 609.344 m.
			// 1 nautical mile  = 1 852 m.
			
			double result = 0;
			
			switch(lengthUnit)
			{
				case LengthUnits.Centimeters:
					switch(speedUnits)
					{
						case SpeedUnit.FeetPerSecond:
							//  Centimeters to feet.
							result = length / 30.48;
							break;
						case SpeedUnit.MetersPerSecond:
							//  Centimeters to meters.
							result = length / 100;
							break;
						case SpeedUnit.KilometersPerHour:
							// Centimeters to kilometers.
							result = length / 100000;
							break;
						case SpeedUnit.MilesPerHour:
							// Centimeters to miles 
							result = length / 160934.4;
							break;
						case SpeedUnit.Knots:
							// Centimeters to nautical miles
							result = length / 185200;
							break;
						case SpeedUnit.PixelsPerFrame:
						default:
							result = 0;
							break;
					}
					break;
				case LengthUnits.Meters:
					switch(speedUnits)
					{
						case SpeedUnit.FeetPerSecond:
							// Meters to feet.
							result = length / 0.3048;
							break;
						case SpeedUnit.MetersPerSecond:
							// Meters to meters.
							result = length;
							break;
						case SpeedUnit.KilometersPerHour:
							// Meters to kilometers.
							result = length / 1000;
							break;
						case SpeedUnit.MilesPerHour:
							// Meters to miles.
							result = length / 1609.344;
							break;
						case SpeedUnit.Knots:
							// Meters to nautical miles.
							result = length / 1852;
							break;
						case SpeedUnit.PixelsPerFrame:
						default:
							result = 0;
							break;
					}
					break;
				case LengthUnits.Inches:
					switch(speedUnits)
					{
						case SpeedUnit.FeetPerSecond:
							// Inches to feet.
							result = length / 12;
							break;
						case SpeedUnit.MetersPerSecond:
							// Inches to meters.
							result = length / 39.3700787;
							break;
						case SpeedUnit.KilometersPerHour:
							// Inches to kilometers.
							result = length / 39370.0787;
							break;
						case SpeedUnit.MilesPerHour:
							// Inches to miles.
							result = length / 63360;
							break;
						case SpeedUnit.Knots:
							// Inches to nautical miles.
							result = length / 72913.3858;
							break;
						case SpeedUnit.PixelsPerFrame:
						default:
							result = 0;
							break;
					}
					break;
				case LengthUnits.Feet:
					switch(speedUnits)
					{
						case SpeedUnit.FeetPerSecond:
							// Feet to feet.
							result = length;
							break;
						case SpeedUnit.MetersPerSecond:
							// Feet to meters.
							result = length / 3.2808399;
							break;
						case SpeedUnit.KilometersPerHour:
							// Feet to kilometers.
							result = length / 3280.8399;
							break;
						case SpeedUnit.MilesPerHour:
							// Feet to miles.
							result = length / 5280;
							break;
						case SpeedUnit.Knots:
							// Feet to nautical miles.
							result = length / 6076.11549;
							break;
						case SpeedUnit.PixelsPerFrame:
						default:
							result = 0;
							break;
					}
					break;
				case LengthUnits.Yards:
					switch(speedUnits)
					{
						case SpeedUnit.FeetPerSecond:
							// Yards to feet.
							result = length * 3;
							break;
						case SpeedUnit.MetersPerSecond:
							// Yards to meters.
							result = length / 1.0936133;
							break;
						case SpeedUnit.KilometersPerHour:
							// Yards to kilometers.
							result = length / 1093.6133;
							break;
						case SpeedUnit.MilesPerHour:
							// Yards to miles.
							result = length / 1760;
							break;
						case SpeedUnit.Knots:
							// Yards to nautical miles.
							result = length / 2025.37183;
							break;
						case SpeedUnit.PixelsPerFrame:
						default:
							result = 0;
							break;
					}
					break;
				case LengthUnits.Pixels:
                case LengthUnits.Percentage:
				default:
					// If input length is in pixel, this means the image is not calibrated.
					// Unless the target speed unit is pixel per frame, we can't compute the speed.
					result = speedUnits != SpeedUnit.PixelsPerFrame ? 0 : length;
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
