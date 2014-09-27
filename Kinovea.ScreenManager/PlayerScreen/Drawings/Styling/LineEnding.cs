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
    /// A simple wrapper around two LineCap values.
    /// Used to describe arrow endings and possibly other endings.
    /// </summary>
    [TypeConverter(typeof(LineEndingConverter))]
    public struct LineEnding
    {
        public readonly LineCap StartCap;
        public readonly LineCap EndCap;

        #region Static properties
        private static LineEnding none = new LineEnding(LineCap.Round, LineCap.Round);
        public static LineEnding None
        {
            get { return none; }
        }
        private static LineEnding startArrow = new LineEnding(LineCap.ArrowAnchor, LineCap.Round);
        public static LineEnding StartArrow
        {
            get { return startArrow; }
        }
        private static LineEnding endArrow = new LineEnding(LineCap.Round, LineCap.ArrowAnchor);
        public static LineEnding EndArrow
        {
            get { return endArrow; }
        }
        private static LineEnding doubleArrow = new LineEnding(LineCap.ArrowAnchor, LineCap.ArrowAnchor);
        public static LineEnding DoubleArrow
        {
            get { return doubleArrow; }
        }
        #endregion

        public LineEnding(LineCap start, LineCap end)
        {
            StartCap = start;
            EndCap = end;
        }
    }

    /// <summary>
    /// Converter class for LineEnding. 
    /// Support: string.
    /// </summary>
    public class LineEndingConverter : TypeConverter
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
                return LineEnding.None;

            string[] split = stringValue.Split(new Char[] { ';' });

            if (split.Length != 2)
                return LineEnding.None;

            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(LineCap));
            LineCap start = (LineCap)enumConverter.ConvertFromString(context, culture, split[0]);
            LineCap end = (LineCap)enumConverter.ConvertFromString(context, culture, split[1]);

            return new LineEnding(start, end);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            LineEnding lineEnding = (LineEnding)value;
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(LineCap));
            string result = String.Format("{0};{1}",
                enumConverter.ConvertToString(context, culture, (LineCap)lineEnding.StartCap),
                enumConverter.ConvertToString(context, culture, (LineCap)lineEnding.EndCap));
            return result;
        }
    }
}
