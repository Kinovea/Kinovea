using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// A common data structure for all composite configuration options.
    /// It is simpler to group all options in a single class for serialization, etc.
    /// Not all options may be used by all composite, and some options may mean different things depending on the composite.
    /// </summary>
    public class DelayCompositeConfiguration
    {
        public DelayCompositeType CompositeType { get; set; }
        public int ImageCount { get; set; }
        public float RefreshRate { get; set; }
        public int Start { get; set; }
        public int Interval { get; set; }

        private static DelayCompositeConfiguration defaultConfiguration;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DelayCompositeConfiguration()
        {
            CompositeType = DelayCompositeType.Basic;
            ImageCount = 1;
            RefreshRate = 1;
            Start = 0;
            Interval = 1;
        }

        static DelayCompositeConfiguration()
        {
            defaultConfiguration = new DelayCompositeConfiguration();
        }

        public static DelayCompositeConfiguration Default 
        {
            get { return defaultConfiguration; } 
        }

        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "CompositeType":
                        CompositeType = (DelayCompositeType)Enum.Parse(typeof(DelayCompositeType), r.ReadElementContentAsString());
                        break;
                    case "ImageCount":
                        ImageCount = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "RefreshRate":
                        RefreshRate = float.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "Start":
                        Start = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "Interval":
                        Interval = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("CompositeType", CompositeType.ToString());
            w.WriteElementString("ImageCount", ImageCount.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("RefreshRate", RefreshRate.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("Start", Start.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("Interval", Interval.ToString(CultureInfo.InvariantCulture));
        }
    }
}
