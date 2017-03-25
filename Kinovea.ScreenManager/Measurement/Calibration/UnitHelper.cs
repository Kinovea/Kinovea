﻿#region License
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

        #region Abbreviations
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
                    
                    if (string.IsNullOrEmpty(PreferencesManager.PlayerPreferences.CustomLengthAbbreviation))
                        result = "%";
                    else
                        result = PreferencesManager.PlayerPreferences.CustomLengthAbbreviation;
                    
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
                case SpeedUnit.PixelsPerSecond:
                default:
                    abbreviation = "px/s";
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
                case AccelerationUnit.PixelsPerSecondSquared:
                default:
                    abbreviation = "px/s²";
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
        #endregion

        /// <summary>
        /// Takes a velocity in length unit per second and returns it in speed unit.
        /// </summary>
        public static float ConvertVelocity(float v, LengthUnit lengthUnit, SpeedUnit unit)
        {
            float result = 0;
            float perSecond = ConvertLengthForSpeedUnit(v, lengthUnit, unit);
            
            switch (unit)
            {
                case SpeedUnit.FeetPerSecond:
                case SpeedUnit.MetersPerSecond:
                case SpeedUnit.PixelsPerSecond:
                    result = perSecond;
                    break;
                case SpeedUnit.KilometersPerHour:
                case SpeedUnit.MilesPerHour:
                case SpeedUnit.Knots:
                    result = perSecond * 3600;
                    break;
                default:
                    result = 0;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Takes an acceleration in length unit per second squared and returns it in acceleration unit.
        /// </summary>
        public static float ConvertAcceleration(float a, LengthUnit lengthUnit, AccelerationUnit unit)
        {
            float perSecondSquared = ConvertLengthForAccelerationUnit(a, lengthUnit, unit);
            return perSecondSquared;
        }

        /// <summary>
        /// Takes an angular velocity in radians per second and returns it in angular velocity unit.
        /// </summary>
        public static double ConvertAngularVelocity(double radiansPerSecond, AngularVelocityUnit unit)
        {
            double result = 0;

            switch (unit)
            {
                case AngularVelocityUnit.DegreesPerSecond:
                    result = radiansPerSecond * MathHelper.RadiansToDegrees;
                    break;
                case AngularVelocityUnit.RadiansPerSecond:
                    result = radiansPerSecond;
                    break;
                case AngularVelocityUnit.RevolutionsPerMinute:
                    double revolutionsPerSecond = radiansPerSecond / (2 * Math.PI);
                    result = revolutionsPerSecond * 60;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Takes an angular acceleration in radians per second squared and returns it in angular acceleration unit.
        /// </summary>
        public static float ConvertAngularAcceleration(float radiansPerSecondSquared, AngularAccelerationUnit unit)
        {
            float result = 0;

            switch (unit)
            {
                case AngularAccelerationUnit.DegreesPerSecondSquared:
                    result = (float)(radiansPerSecondSquared * MathHelper.RadiansToDegrees);
                    break;
                case AngularAccelerationUnit.RadiansPerSecondSquared:
                    result = radiansPerSecondSquared;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Takes a value in speed unit and returns it in length unit.
        /// </summary>
        public static float ConvertForLengthUnit(float v, SpeedUnit speedUnits, LengthUnit lengthUnit)
        {
            // Convert from one length unit to another, source unit is extracted from velocity unit.
            // first convert into meters, then from meters to output.
            if (speedUnits == SpeedUnit.PixelsPerSecond)
                return v;

            float metersPerSecond = GetMeters(v, speedUnits);

            double result = 0;
            switch (lengthUnit)
            {
                case LengthUnit.Centimeters:
                    result = metersPerSecond / centimeterToMeters;
                    break;
                case LengthUnit.Feet:
                    result = metersPerSecond / footToMeters;
                    break;
                case LengthUnit.Inches:
                    result = metersPerSecond / inchToMeters;
                    break;
                case LengthUnit.Meters:
                    result = metersPerSecond;
                    break;
                case LengthUnit.Millimeters:
                    result = metersPerSecond / millimeterToMeters;
                    break;
                case LengthUnit.Yards:
                    result = metersPerSecond / yardToMeters;
                    break;
                case LengthUnit.Percentage:
                case LengthUnit.Pixels:
                default:
                    result = v;
                    break;
            }

            return (float)result;
        }

        private static float ConvertLengthForSpeedUnit(float length, LengthUnit lengthUnit, SpeedUnit speedUnits)
        {
            // Convert from one length unit to another, target unit is extracted from velocity unit.
            // We first convert from whatever unit into meters, then from meters to the output.

            if (lengthUnit == LengthUnit.Pixels || lengthUnit == LengthUnit.Percentage)
                return length;

            float meters = GetMeters(length, lengthUnit);

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
                case SpeedUnit.PixelsPerSecond:
                default:
                    result = length;
                    break;
            }

            return (float)result;
        }

        private static float ConvertLengthForAccelerationUnit(float length, LengthUnit lengthUnit, AccelerationUnit accUnit)
        {
            // Convert from one length unit to another, target unit is extracted from acceleration unit.
            // We first convert from whatever unit into meters, then from meters to the output.

            // The scenario where the space is calibrated but the user wants the acceleration in pixels is not supported.
            if (lengthUnit == LengthUnit.Pixels || lengthUnit == LengthUnit.Percentage)
                return length;

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
                case AccelerationUnit.PixelsPerSecondSquared:
                default:
                    result = 0;
                    break;
            }

            return (float)result;
        }

        private static float GetMeters(float length, LengthUnit unit)
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

            return (float)meters;
        }

        private static float GetMeters(float speed, SpeedUnit unit)
        {
            double meters = 0;

            switch (unit)
            {
                case SpeedUnit.MetersPerSecond:
                    meters = speed;
                    break;
                case SpeedUnit.KilometersPerHour:
                    meters = speed * kilometerToMeters;
                    break;
                case SpeedUnit.FeetPerSecond:
                    meters = speed * footToMeters;
                    break;
                case SpeedUnit.MilesPerHour:
                    meters = speed * mileToMeters;
                    break;
                case SpeedUnit.Knots:
                    meters = speed * nauticalMileToMeters;
                    break;
                case SpeedUnit.PixelsPerSecond:
                    meters = speed;
                    break;
            }

            return (float)meters;
        }  
    }
}
