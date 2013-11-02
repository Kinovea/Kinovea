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
        // http://en.wikipedia.org/wiki/Conversion_of_units
        // 1 inch 			= 0.0254 m.
        // 1 foot			= 0.3048 m.
        // 1 yard 			= 0.9144 m.
        // 1 mile 			= 1 609.344 m.
        // 1 nautical mile  = 1 852 m.
        private const double millimeterToMeters = 0.001;
        private const double centimeterToMeters = 0.01;
        private const double inchToMeters = 0.0254;
        private const double footToMeters = 0.3048;
        private const double yardToMeters = 0.9144;
        private const double kilometerToMeters = 1000;
        private const double mileToMeters = 1609.344;
        private const double nauticalMileToMeters = 1852;

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

        public static string AccelerationAbbreviation(AccelerationUnit unit)
        {
            string abbreviation = "";
            switch (unit)
            {
                case AccelerationUnit.FeetPerSecondSquared:
                    abbreviation = "ft/s²";
                    break;
                case AccelerationUnit.MetersPerSecondSquared:
                    abbreviation = "m/s²";
                    break;
                case AccelerationUnit.PixelsPerFrameSquared:
                default:
                    abbreviation = "px/f²";
                    break;
            }

            return abbreviation;
        }

        public static string AngleAbbreviation(AngleUnit unit)
        {
            string abbreviation = "";
            switch (unit)
            {
                case AngleUnit.Degree:
                    abbreviation = "°";
                    break;
                case AngleUnit.Radian:
                    abbreviation = "rad";
                    break;
            }

            return abbreviation;
        }

        public static string AngularVelocityAbbreviation(AngularVelocityUnit unit)
        {
            string abbreviation = "";
            switch (unit)
            {
                case AngularVelocityUnit.DegreesPerSecond:
                    abbreviation = "deg/s";
                    break;
                case AngularVelocityUnit.RadiansPerSecond:
                    abbreviation = "rad/s";
                    break;
                case AngularVelocityUnit.RevolutionsPerMinute:
                    abbreviation = "rpm";
                    break;
            }

            return abbreviation;
        }

        public static string AngularAccelerationAbbreviation(AngularAccelerationUnit unit)
        {
            string abbreviation = "";
            switch (unit)
            {
                case AngularAccelerationUnit.DegreesPerSecondSquared:
                    abbreviation = "deg/s²";
                    break;
                case AngularAccelerationUnit.RadiansPerSecondSquared:
                    abbreviation = "rad/s²";
                    break;
            }

            return abbreviation;
        }

        public static double ConvertVelocity(double v, double framesPerSecond, LengthUnit lengthUnit, SpeedUnit unit)
        {
            // v is given in <lengthUnit>/f.
            if (unit == SpeedUnit.PixelsPerFrame)
                return v;

            double v2 = ConvertLengthForSpeedUnit(v, lengthUnit, unit);
            
            double result = 0;
            double perSecond = v2 * framesPerSecond;

            switch (unit)
            {
                case SpeedUnit.FeetPerSecond:
                case SpeedUnit.MetersPerSecond:
                    result = perSecond;
                    break;
                case SpeedUnit.KilometersPerHour:
                case SpeedUnit.MilesPerHour:
                case SpeedUnit.Knots:
                    result = perSecond * 3600;
                    break;
                default:
                    result = v2;
                    break;
            }

            return result;
        }

        public static double ConvertAcceleration(double a, double framesPerSecond, LengthUnit lengthUnit, AccelerationUnit unit)
        {
            // a is given in <lengthUnit>/frame².
            if (unit == AccelerationUnit.PixelsPerFrameSquared)
                return a;

            double a2 = ConvertLengthForAccelerationUnit(a, lengthUnit, unit);
            
            double result = 0;
            double perSecondSquared = a2 * framesPerSecond;

            switch (unit)
            {
                case AccelerationUnit.FeetPerSecondSquared:
                case AccelerationUnit.MetersPerSecondSquared:
                    result = perSecondSquared;
                    break;
                default:
                    result = a2;
                    break;
            }

            return result;
        }

        public static double ConvertAngularVelocity(double radiansPerFrame, double framesPerSecond, AngularVelocityUnit unit)
        {
            if (unit == AngularVelocityUnit.RadiansPerFrame)
                return radiansPerFrame;

            double result = 0;

            switch (unit)
            {
                case AngularVelocityUnit.DegreesPerSecond:
                    result = radiansPerFrame * MathHelper.RadiansToDegrees * framesPerSecond;
                    break;
                case AngularVelocityUnit.RadiansPerSecond:
                    result = radiansPerFrame * framesPerSecond;
                    break;
                case AngularVelocityUnit.RevolutionsPerMinute:
                    double revolutionsPerFrame = radiansPerFrame / (2 * Math.PI);
                    result = revolutionsPerFrame * framesPerSecond * 60;
                    break;
            }

            return result;
        }

        public static double ConvertAngularAcceleration(double radiansPerFrameSquared, double framesPerSecond, AngularAccelerationUnit unit)
        {
            if (unit == AngularAccelerationUnit.RadiansPerFrameSquared)
                return radiansPerFrameSquared;

            double result = 0;

            switch (unit)
            {
                case AngularAccelerationUnit.DegreesPerSecondSquared:
                    result = radiansPerFrameSquared * MathHelper.RadiansToDegrees * framesPerSecond;
                    break;
                case AngularAccelerationUnit.RadiansPerSecondSquared:
                    result = radiansPerFrameSquared * framesPerSecond;
                    break;
            }

            return result;
        }

        private static double ConvertLengthForSpeedUnit(double length, LengthUnit lengthUnit, SpeedUnit speedUnits)
        {
            // Convert from one length unit to another, target unit is extracted from velocity unit.
            // We first convert from whatever unit into meters, then from meters to the output.

            // The scenario where the space is calibrated but the user wants the speed in pixels is not supported.
            if (lengthUnit == LengthUnit.Pixels || lengthUnit == LengthUnit.Percentage)
                return speedUnits == SpeedUnit.PixelsPerFrame ? length : 0;

            double meters = GetMeters(length, lengthUnit);

            double result = 0;
            switch (speedUnits)
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

        private static double ConvertLengthForAccelerationUnit(double length, LengthUnit lengthUnit, AccelerationUnit accUnit)
        {
            // Convert from one length unit to another, target unit is extracted from acceleration unit.
            // We first convert from whatever unit into meters, then from meters to the output.

            // The scenario where the space is calibrated but the user wants the acceleration in pixels is not supported.
            if (lengthUnit == LengthUnit.Pixels || lengthUnit == LengthUnit.Percentage)
                return accUnit == AccelerationUnit.PixelsPerFrameSquared ? length : 0;

            double meters = GetMeters(length, lengthUnit);

            double result = 0;
            switch (accUnit)
            {
                case AccelerationUnit.FeetPerSecondSquared:
                    result = meters / footToMeters;
                    break;
                case AccelerationUnit.MetersPerSecondSquared:
                    result = meters;
                    break;
                case AccelerationUnit.PixelsPerFrameSquared:
                default:
                    result = 0;
                    break;
            }

            return result;
        }

        private static double GetMeters(double length, LengthUnit unit)
        {
            double meters = 0;

            switch (unit)
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

            return meters;
        }  
    
    }
}
