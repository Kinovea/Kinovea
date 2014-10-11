#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A simple wrapper around a dash style and the presence of time ticks.
    /// Used to describe line shape for tracks.
    /// </summary>
    [TypeConverter(typeof(TrackShapeConverter))]
    public struct TrackShape
    {
        public readonly DashStyle DashStyle;
        public readonly bool ShowSteps;

        #region Static Properties
        private static TrackShape solid = new TrackShape(DashStyle.Solid, false);
        public static TrackShape Solid
        {
            get { return solid; }
        }
        private static TrackShape dash = new TrackShape(DashStyle.Dash, false);
        public static TrackShape Dash
        {
            get { return dash; }
        }
        private static TrackShape solidSteps = new TrackShape(DashStyle.Solid, true);
        public static TrackShape SolidSteps
        {
            get { return solidSteps; }
        }
        private static TrackShape dashSteps = new TrackShape(DashStyle.Dash, true);
        public static TrackShape DashSteps
        {
            get { return dashSteps; }
        }
        #endregion

        public TrackShape(DashStyle style, bool steps)
        {
            DashStyle = style;
            ShowSteps = steps;
        }
    }

    /// <summary>
    /// Converter class for TrackShape.
    /// Support: string.
    /// </summary>
    public class TrackShapeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) ? true : base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
                return base.ConvertFrom(context, culture, value);

            string stringValue = value as string;

            if (string.IsNullOrEmpty(stringValue))
                return TrackShape.Solid;

            string[] split = stringValue.Split(new Char[] { ';' });

            if (split.Length != 2)
                return TrackShape.Solid;

            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(DashStyle));
            DashStyle dash = (DashStyle)enumConverter.ConvertFromString(context, culture, split[0]);

            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
            bool steps = (bool)boolConverter.ConvertFromString(context, culture, split[1]);

            return new TrackShape(dash, steps);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            TrackShape trackShape = (TrackShape)value;
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(DashStyle));
            string result = String.Format("{0};{1}",
                enumConverter.ConvertToString(context, culture, (DashStyle)trackShape.DashStyle),
                trackShape.ShowSteps ? "true" : "false");

            return result;
        }
    }
}