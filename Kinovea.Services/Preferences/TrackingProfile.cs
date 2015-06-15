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
        public string Name { get; set; }
        public double SimilarityThreshold { get; set; }
        public double TemplateUpdateThreshold { get; set; }
        public Size SearchWindow { get; set; }
        public Size BlockWindow { get; set; }
        public TrackerParameterUnit SearchWindowUnit { get; set; }
        public TrackerParameterUnit BlockWindowUnit { get; set; }
        public int RefinementNeighborhood { get; set; }
        public bool ResetOnMove { get; set; }

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

            SimilarityThreshold = Math.Min(1.0, SimilarityThreshold);
            TemplateUpdateThreshold = Math.Min(1.0, TemplateUpdateThreshold);

            if (BlockWindow.Width >= SearchWindow.Width || BlockWindow.Height >= SearchWindow.Height)
                BlockWindow = new Size(SearchWindow.Width / 2, SearchWindow.Height / 2);
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
