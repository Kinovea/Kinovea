using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class DistortionImporterAgisoft
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static DistortionParameters Import(string path, Size imageSize)
        {
            DistortionParameters parameters = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                int width = ReadInt(doc, "/calibration/width");
                int height = ReadInt(doc, "/calibration/height");

                double fx = ReadDouble(doc, "/calibration/fx");
                double fy = ReadDouble(doc, "/calibration/fy");
                double cx = ReadDouble(doc, "/calibration/cx");
                double cy = ReadDouble(doc, "/calibration/cy");
                double k1 = ReadDouble(doc, "/calibration/k1");
                double k2 = ReadDouble(doc, "/calibration/k2");
                double k3 = ReadDouble(doc, "/calibration/k3");
                double p1 = ReadDouble(doc, "/calibration/p1");
                double p2 = ReadDouble(doc, "/calibration/p2");

                if (imageSize.Width != width || imageSize.Height != height)
                {
                    double xFactor = (double)imageSize.Width / width;
                    double yFactor = (double)imageSize.Height / height;

                    fx *= xFactor;
                    fy *= yFactor;
                    cx *= xFactor;
                    cy *= yFactor;
                }

                double pixelsPerMillemeters = imageSize.Width / DistortionParameters.defaultSensorWidth;

                parameters = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy, pixelsPerMillemeters);
            }
            catch
            {
                log.ErrorFormat("Import of Agisoft Lens distortion parameters failed.");
            }

            return parameters;
        }

        private static int ReadInt(XmlDocument doc, string xpath)
        {
            int value = 0;

            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null)
                throw new XmlException();
            
            value = int.Parse(node.InnerText, CultureInfo.InvariantCulture);
            return value;
        }

        private static double ReadDouble(XmlDocument doc, string xpath)
        {
            double value = 0;

            XmlNode node = doc.SelectSingleNode(xpath);

            if (node == null)
                throw new XmlException();

            value = double.Parse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture);
            return value;
        }
    }
}
