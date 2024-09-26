#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;
using System.Xml;
using System.Globalization;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Parameters passed to tracking module.
    /// </summary>
    public class TrackerParameters
    {
        /// <summary>
        /// The similarity value above which we consider a match.
        /// Default: 0.5.
        /// </summary>
        public double SimilarityThreshold
        {
            get { return similarityThreshold; }
        }

        /// <summary>
        /// The similarity value under which we will update the reference template with the current template.
        /// Above this value the template is not considered very different so we keep the reference one for next matching.
        /// Default: 0.8.
        /// </summary>
        public double TemplateUpdateThreshold
        {
            get { return templateUpdateThreshold; }
        }

        /// <summary>
        /// Number of surrounding pixels taken into account in each direction to refine the location of best match.
        /// </summary>
        public int RefinementNeighborhood
        {
            get { return refinementNeighborhood; }
        }

        /// <summary>
        /// Size of the search window.
        /// </summary>
        public Size SearchWindow
        {
            get { return searchWindow; }
        }

        /// <summary>
        /// Size of the pattern block we are matching.
        /// </summary>
        public Size BlockWindow
        {
            get { return blockWindow; }
        }

        public bool ResetOnMove
        {
            get { return resetOnMove; }
        }

        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= similarityThreshold.GetHashCode();
                hash ^= templateUpdateThreshold.GetHashCode();
                hash ^= searchWindow.GetHashCode();
                hash ^= blockWindow.GetHashCode();
                return hash;
            }
        }

        private double similarityThreshold = 0.5;
        private double templateUpdateThreshold = 0.8; // using CCORR : 0.90 or 0.95, when using CCOEFF : 0.80.
        private int refinementNeighborhood = 1;
        private Size searchWindow;
        private Size blockWindow;
        private bool resetOnMove = true;

        public TrackerParameters(double similarityThreshold, double templateUpdateThreshold, int refinementNeighborhood, Size searchWindow, Size blockWindow, bool resetOnMove)
        {
            this.similarityThreshold = similarityThreshold;
            this.templateUpdateThreshold = templateUpdateThreshold;
            this.refinementNeighborhood = refinementNeighborhood;
            this.searchWindow = searchWindow;
            this.blockWindow = blockWindow;
            this.resetOnMove = resetOnMove;
        }

        public TrackerParameters(TrackingProfile profile, Size imageSize)
        {
            this.similarityThreshold = profile.SimilarityThreshold;
            this.templateUpdateThreshold = profile.TemplateUpdateThreshold;

            this.searchWindow = profile.SearchWindow;
            if (profile.SearchWindowUnit == TrackerParameterUnit.Percentage)
                this.searchWindow = new Size((int)(imageSize.Width * (profile.SearchWindow.Width / 100.0)), (int)(imageSize.Height * (profile.SearchWindow.Height / 100.0)));

            this.blockWindow = profile.BlockWindow;
            if (profile.BlockWindowUnit == TrackerParameterUnit.Percentage)
                this.blockWindow = new Size((int)(imageSize.Width * (profile.BlockWindow.Width / 100.0)), (int)(imageSize.Height * (profile.BlockWindow.Height / 100.0)));

            this.resetOnMove = profile.ResetOnMove;
        }
        
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("SimilarityThreshold", String.Format(CultureInfo.InvariantCulture, "{0}", similarityThreshold));
            w.WriteElementString("TemplateUpdateThreshold", String.Format(CultureInfo.InvariantCulture, "{0}", templateUpdateThreshold));
            w.WriteElementString("RefinementNeighborhood", String.Format(CultureInfo.InvariantCulture, "{0}", refinementNeighborhood));
            w.WriteElementString("SearchWindow", String.Format(CultureInfo.InvariantCulture, "{0};{1}", searchWindow.Width, searchWindow.Height));
            w.WriteElementString("BlockWindow", String.Format(CultureInfo.InvariantCulture, "{0};{1}", blockWindow.Width, blockWindow.Height));
        }

        public static TrackerParameters ReadXml(XmlReader r, PointF scale)
        {
            TrackingProfile classic = new TrackingProfile();
            double similarityThreshold = classic.SimilarityThreshold;
            double updateThreshold = classic.TemplateUpdateThreshold;
            int refinementNeighborhood = classic.RefinementNeighborhood;
            Size searchWindow = classic.SearchWindow;
            Size blockWindow = classic.BlockWindow;
            
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "SimilarityThreshold":
                        similarityThreshold = r.ReadElementContentAsDouble();
                        break;
                    case "TemplateUpdateThreshold":
                        updateThreshold = r.ReadElementContentAsDouble();
                        break;
                    case "RefinementNeighborhood":
                        refinementNeighborhood = r.ReadElementContentAsInt();
                        break;
                    case "SearchWindow":
                        searchWindow = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        searchWindow = new SizeF(searchWindow.Width * scale.X, searchWindow.Height * scale.Y).ToSize();
                        break;
                    case "BlockWindow":
                        blockWindow = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        blockWindow = new SizeF(blockWindow.Width * scale.X, blockWindow.Height * scale.Y).ToSize();
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        break;
                }
            }
            
            r.ReadEndElement();

            TrackerParameters parameters = new TrackerParameters(similarityThreshold, updateThreshold, refinementNeighborhood, searchWindow, blockWindow, false);
            return parameters;
        }
    }
}
