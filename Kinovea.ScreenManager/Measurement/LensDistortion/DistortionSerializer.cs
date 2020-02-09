using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DistortionSerializer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Serialize(XmlWriter w, DistortionParameters p, bool saveSize, Size imageSize)
        {
            if (p == null)
                return;

            w.WriteStartElement("CameraCalibration");

            Action<string, double> write = (element, value) => w.WriteElementString(element, string.Format(CultureInfo.InvariantCulture, "{0}", value));

            if (saveSize)
                w.WriteElementString("ImageSize", string.Format("{0};{1}", imageSize.Width, imageSize.Height));

            write("Fx", p.Fx);
            write("Fy", p.Fy);
            write("Cx", p.Cx);
            write("Cy", p.Cy);
            
            write("K1", p.K1);
            write("K2", p.K2);
            write("K3", p.K3);
            write("P1", p.P1);
            write("P2", p.P2);

            write("PixelsPerMM", p.PixelsPerMillimeter);

            w.WriteEndElement();
        }

        public static DistortionParameters Deserialize(XmlReader r, Size inputSize)
        {
            r.ReadStartElement();

            Size size = inputSize;
            double pixelsPerMillimeter = 0;
            double fx = 1;
            double fy = 1;
            double cx = 0;
            double cy = 0;
            double k1 = 0;
            double k2 = 0;
            double k3 = 0;
            double p1 = 0;
            double p2 = 0;

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "ImageSize":
                        size = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        break;
                    case "PixelsPerMM":
                        pixelsPerMillimeter = r.ReadElementContentAsDouble();
                        break;
                    case "Fx":
                        fx = r.ReadElementContentAsDouble();
                        break;
                    case "Fy":
                        fy = r.ReadElementContentAsDouble();
                        break;
                    case "Cx":
                        cx = r.ReadElementContentAsDouble();
                        break;
                    case "Cy":
                        cy = r.ReadElementContentAsDouble();
                        break;
                    case "K1":
                        k1 = r.ReadElementContentAsDouble();
                        break;
                    case "K2":
                        k2 = r.ReadElementContentAsDouble();
                        break;
                    case "K3":
                        k3 = r.ReadElementContentAsDouble();
                        break;
                    case "P1":
                        p1 = r.ReadElementContentAsDouble();
                        break;
                    case "P2":
                        p2 = r.ReadElementContentAsDouble();
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in Camera calibration: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();

            if (pixelsPerMillimeter == 0)
                pixelsPerMillimeter = size.Width / DistortionParameters.defaultSensorWidth;

            if (inputSize.Width != size.Width || inputSize.Height != size.Height)
            {
                double xFactor = (double)inputSize.Width / size.Width;
                double yFactor = (double)inputSize.Height / size.Height;

                fx *= xFactor;
                fy *= yFactor;
                cx *= xFactor;
                cy *= yFactor;
            }

            DistortionParameters parameters = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy, pixelsPerMillimeter);
            return parameters;
        }

    }
}
