using System;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
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
