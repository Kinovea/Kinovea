using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// Parameters for the photofinish capture mode.
    /// </summary>
    public class PhotofinishConfiguration
    {
        public int ThresholdHeight { get; set; }
        public int ConsolidationHeight { get; set; }
        public int OutputHeight { get; set; }
        private static PhotofinishConfiguration defaultConfiguration;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PhotofinishConfiguration()
        {
            ThresholdHeight = 16;
            ConsolidationHeight = 2;
            OutputHeight = 1000;
        }

        static PhotofinishConfiguration()
        {
            defaultConfiguration = new PhotofinishConfiguration();
        }

        public static PhotofinishConfiguration Default
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
                    case "ThresholdHeight":
                        ThresholdHeight = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "ConsolidationHeight":
                        ConsolidationHeight = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "OutputHeight":
                        OutputHeight = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
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
            w.WriteElementString("ThresholdHeight", ThresholdHeight.ToString());
            w.WriteElementString("ConsolidationHeight", ConsolidationHeight.ToString());
            w.WriteElementString("OutputHeight", OutputHeight.ToString());
        }
    }
}
