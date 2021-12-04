using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Xml;

namespace Kinovea.ScreenManager
{
    public static class MeasurementSerializationHelper
    {
        public static MeasuredDataPosition CollectPosition(string name, PointF p, CalibrationHelper calibrationHelper)
        {
            MeasuredDataPosition md = new MeasuredDataPosition();
            md.Name = name;
            PointF coords = calibrationHelper.GetPoint(p);
            md.X = coords.X;
            md.Y = coords.Y;
            md.XLocal = String.Format("{0:0.00}", coords.X);
            md.YLocal = String.Format("{0:0.00}", coords.Y);
            return md;
        }

        public static void SerializeDistance(XmlWriter w, PointF p1, PointF p2, CalibrationHelper calibrationHelper)
        {
            PointF a = calibrationHelper.GetPoint(p1);
            PointF b = calibrationHelper.GetPoint(p2);
            float len = GeometryHelper.GetDistance(a, b);
            string value = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", len);
            string valueLocal = String.Format("{0:0.00}", len);
            w.WriteAttributeString("value", value);
            w.WriteAttributeString("valueLocal", valueLocal);
        }

        public static void SerializeAngle(XmlWriter w, AngleHelper angleHelper, CalibrationHelper calibrationHelper)
        {
            float angle = calibrationHelper.ConvertAngle(angleHelper.CalibratedAngle);
            string value = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", angle);
            string valueLocal = String.Format("{0:0.00}", angle);
            w.WriteAttributeString("value", value);
            w.WriteAttributeString("valueLocal", valueLocal);
        }
    }
}
