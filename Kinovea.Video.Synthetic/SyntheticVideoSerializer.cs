using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Kinovea.Services;
using System.Drawing;

namespace Kinovea.Video.Synthetic
{
    public static class SyntheticVideoSerializer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static SyntheticVideo Deserialize(string file)
        {
            if (!File.Exists(file))
                return null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            SyntheticVideo video = null;

            using (XmlReader r = XmlReader.Create(file, settings))
            {
                video = Parse(r);
            }

            return video;
        }

        private static SyntheticVideo Parse(XmlReader r)
        {
            SyntheticVideo video = new SyntheticVideo();

            r.MoveToContent();

            if (!(r.Name == "KinoveaSyntheticVideo"))
                return video;

            r.ReadStartElement();
            r.ReadElementContentAsString("FormatVersion", "");

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "ImageSize":
                        video.ImageSize = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        break;
                    case "FramesPerSecond":
                        video.FramePerSecond = r.ReadElementContentAsDouble();
                        break;
                    case "DurationFrames":
                        video.DurationFrames = r.ReadElementContentAsInt();
                        break;
                    case "BackgroundColor":
                        video.BackgroundColor = XmlHelper.ParseColor(r.ReadElementContentAsString(), Color.White);
                        break;
                    case "FrameNumber":
                        video.FrameNumber = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "Objects":
                        video.Objects = ParseObjects(r);
                        break;
                    default:
                        // Skip unparsed nodes.
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KSV XML: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();

            return video;
        }

        private static List<SyntheticObject> ParseObjects(XmlReader r)
        {
            List<SyntheticObject> objects = new List<SyntheticObject>();

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Object")
                {
                    SyntheticObject o = ParseObject(r);
                    if (o != null)
                        objects.Add(o);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();

            return objects;
        }

        private static SyntheticObject ParseObject(XmlReader r)
        {
            int radius = 0;
            PointF position = PointF.Empty;
            double vx = 0;
            double vy = 0;
            double ax = 0;
            double ay = 0;

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "Radius":
                        radius = r.ReadElementContentAsInt();
                        break;
                    case "Position":
                        position = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "Velocity":
                        PointF v = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        vx = v.X;
                        vy = v.Y;
                        break;
                    case "Acceleration":
                        PointF a = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        ax = a.X;
                        ay = a.Y;
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KSV synthetic object: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();
            
            return radius != 0 ? new SyntheticObject(radius, position, vx, vy, ax, ay) : null;
        }
    }
}
