using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class MeasurementSerializationHelper
    {
        public static MeasuredDataPosition CollectPosition(string name, PointF p, CalibrationHelper calibrationHelper)
        {
            MeasuredDataPosition md = new MeasuredDataPosition();
            md.Name = name;
            PointF coords = p;
            if (PreferencesManager.PlayerPreferences.ExportSpace == ExportSpace.WorldSpace)
                coords = calibrationHelper.GetPoint(p);

            md.X = coords.X;
            md.Y = coords.Y;
            md.XLocal = String.Format("{0:0.00}", coords.X);
            md.YLocal = String.Format("{0:0.00}", coords.Y);
            return md;
        }

        public static MeasuredDataDistance CollectDistance(string name, PointF p1, PointF p2, CalibrationHelper calibrationHelper)
        {
            MeasuredDataDistance md = new MeasuredDataDistance();
            md.Name = name;

            PointF a = p1;
            PointF b = p2;
            if (PreferencesManager.PlayerPreferences.ExportSpace == ExportSpace.WorldSpace)
            {
                a = calibrationHelper.GetPoint(p1);
                b = calibrationHelper.GetPoint(p2);
            }

            float len = GeometryHelper.GetDistance(a, b);
            md.Value = len;
            md.ValueLocal = String.Format("{0:0.00}", len);
            return md;
        }

        public static MeasuredDataAngle CollectAngle(string name, AngleHelper angleHelper, CalibrationHelper calibrationHelper)
        {
            MeasuredDataAngle md = new MeasuredDataAngle();
            md.Name = name;

            float angle = calibrationHelper.ConvertAngle(angleHelper.CalibratedAngle);
            md.Value = angle;
            md.ValueLocal = String.Format("{0:0.00}", angle);
            return md;
        }
    }
}
