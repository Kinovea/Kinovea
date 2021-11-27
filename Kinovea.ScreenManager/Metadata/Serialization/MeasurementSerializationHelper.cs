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
        public static void SerializePosition(XmlWriter w, PointF p, CalibrationHelper calibrationHelper)
        {
            PointF coords = calibrationHelper.GetPoint(p);
            string x = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.X);
            string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.Y);
            string xLocal = String.Format("{0:0.00}", coords.X);
            string yLocal = String.Format("{0:0.00}", coords.Y);
            w.WriteAttributeString("x", x);
            w.WriteAttributeString("y", y);
            w.WriteAttributeString("xLocal", xLocal);
            w.WriteAttributeString("yLocal", yLocal);
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
