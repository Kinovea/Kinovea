using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    public class TrackingProfile
    {
        public string Name { get; private set; }
        public double SimilarityThreshold { get; private set; }
        public double TemplateUpdateThreshold { get; private set; }
        public Size SearchWindow { get; private set; }
        public Size BlockWindow { get; private set; }
        public TrackerParameterUnit SearchWindowUnit { get; private set; }
        public TrackerParameterUnit BlockWindowUnit { get; private set; }
        public int RefinementNeighborhood { get; private set; }
        public bool ResetOnMove { get; private set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public TrackingProfile() :
            this("default", 0.5, 0.8, new Size(20, 20), new Size(5, 5), TrackerParameterUnit.Percentage, TrackerParameterUnit.Percentage, 1, false)
        {
        }

        public TrackingProfile(string name, double similarityThreshold, double templateUpdateThreshold, Size searchWindow, Size blockWindow, TrackerParameterUnit searchWindowUnit, TrackerParameterUnit blockWindowUnit, int refinementNeighborhood, bool resetOnMove)
        {
            this.Name = name;
            this.SimilarityThreshold = similarityThreshold;
            this.TemplateUpdateThreshold = templateUpdateThreshold;
            this.SearchWindow = searchWindow;
            this.BlockWindow = blockWindow;
            this.SearchWindowUnit = searchWindowUnit;
            this.BlockWindowUnit = blockWindowUnit;
            this.RefinementNeighborhood = refinementNeighborhood;
            this.ResetOnMove = resetOnMove;
        }

        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "SimilarityThreshold":
                        SimilarityThreshold = double.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "TemplateUpdateThreshold":
                        TemplateUpdateThreshold = double.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "SearchWindow":
                        if (r.MoveToAttribute("unit"))
                            SearchWindowUnit = (TrackerParameterUnit)Enum.Parse(typeof(TrackerParameterUnit), r.ReadContentAsString());

                        r.ReadStartElement();
                        SearchWindow = XmlHelper.ParseSize(r.ReadContentAsString());
                        r.ReadEndElement();
                        break;
                    case "BlockWindow":
                        if (r.MoveToAttribute("unit"))
                            BlockWindowUnit = (TrackerParameterUnit)Enum.Parse(typeof(TrackerParameterUnit), r.ReadContentAsString());

                        r.ReadStartElement();
                        BlockWindow = XmlHelper.ParseSize(r.ReadContentAsString());
                        r.ReadEndElement();
                        break;
                    case "RefinementNeighborhood":
                        RefinementNeighborhood = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "ResetOnMove":
                        ResetOnMove = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
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
            w.WriteElementString("SimilarityThreshold", SimilarityThreshold.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("TemplateUpdateThreshold", TemplateUpdateThreshold.ToString(CultureInfo.InvariantCulture));

            w.WriteStartElement("SearchWindow");
            w.WriteAttributeString("unit", SearchWindowUnit.ToString());
            w.WriteString(XmlHelper.WriteSize(SearchWindow));
            w.WriteEndElement();

            w.WriteStartElement("BlockWindow");
            w.WriteAttributeString("unit", BlockWindowUnit.ToString());
            w.WriteString(XmlHelper.WriteSize(BlockWindow));
            w.WriteEndElement();

            w.WriteElementString("RefinementNeighborhood", RefinementNeighborhood.ToString(CultureInfo.InvariantCulture));

            w.WriteElementString("ResetOnMove", XmlHelper.WriteBoolean(ResetOnMove));
        }
    }
}
