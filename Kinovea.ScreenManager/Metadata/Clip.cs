using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A clip represents a named segment of the video.
    /// </summary>
    public class Clip
    {
        /// <summary>
        /// Name of the clip.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Start of the clip in timestamps.
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// End of the clip in timestamps.
        /// </summary>
        public long End { get; set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Name":
                        Name = r.ReadElementContentAsString();
                        break;
                    case "Start":
                        Start = long.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "End":
                        End = long.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
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
            w.WriteElementString("Name", Name);
            w.WriteElementString("Start", Start.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("End", End.ToString(CultureInfo.InvariantCulture));
        }
    }
}
