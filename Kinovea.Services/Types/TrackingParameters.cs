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

namespace Kinovea.Services
{
    /// <summary>
    /// This class contains the parameters used by tracking.
    /// It is a union of the parameters used by the various tracking algorithms and 
    /// has a type field to identify the algorithm. 
    /// Each implementation then uses the parameters it needs.
    /// </summary>
    public class TrackingParameters
    {

        #region Properties

        /// <summary>
        /// The tracking algorithm to use.
        /// </summary>
        public TrackingAlgorithm TrackingAlgorithm
        {
            get { return trackingAlgorithm; }
        }

        /// <summary>
        /// Size of the search window.
        /// This is used by all algorithms.
        /// </summary>
        public Size SearchWindow
        {
            get { return searchWindow; }
            set 
            { 
                searchWindow = value;
                FitBlockWindow();
            }
        }

        /// <summary>
        /// Size of the template window.
        /// This is used by template matching algorithm.
        /// </summary>
        public Size BlockWindow
        {
            get { return blockWindow; }
            set 
            { 
                blockWindow = value;
                FitBlockWindow();
            }
        }

        /// <summary>
        /// The similarity value above which we consider a match.
        /// Used by template matching.
        /// </summary>
        public double SimilarityThreshold
        {
            get { return similarityThreshold; }
        }

        /// <summary>
        /// The similarity value under which we update the reference template with the current template.
        /// Used by template matching.
        /// Above this value the template is not considered very different 
        /// so we keep the reference one for next matching.
        /// </summary>
        public double TemplateUpdateThreshold
        {
            get { return templateUpdateThreshold; }
        }

        /// <summary>
        /// Number of surrounding pixels taken into account in each direction to refine the location of best match.
        /// Used by template matching.
        /// </summary>
        public int RefinementNeighborhood
        {
            get { return refinementNeighborhood; }
        }

        
        public bool ResetOnMove
        {
            get { return resetOnMove; }
            set { resetOnMove = value; }
        }

        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= trackingAlgorithm.GetHashCode();
                hash ^= searchWindow.GetHashCode();
                hash ^= blockWindow.GetHashCode();
                hash ^= similarityThreshold.GetHashCode();
                hash ^= templateUpdateThreshold.GetHashCode();
                return hash;
            }
        }
        #endregion 

        #region Members
        private TrackingAlgorithm trackingAlgorithm = TrackingAlgorithm.Correlation;
        private Size searchWindow = new Size(100, 100);
        private Size blockWindow = new Size(20, 20);
        private double similarityThreshold = 0.5;
        private double templateUpdateThreshold = 0.8; // using CCORR : 0.90 or 0.95, when using CCOEFF : 0.80.
        private int refinementNeighborhood = 1;
        private bool resetOnMove = true;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public TrackingParameters()
        {
        }

        public TrackingParameters Clone()
        {
            TrackingParameters clone = new TrackingParameters();
            clone.trackingAlgorithm = this.trackingAlgorithm;
            clone.searchWindow = this.searchWindow;
            clone.blockWindow = this.blockWindow;
            clone.similarityThreshold = this.similarityThreshold;
            clone.templateUpdateThreshold = this.templateUpdateThreshold;
            clone.refinementNeighborhood = this.refinementNeighborhood;
            clone.resetOnMove = this.resetOnMove;
            return clone;
        }
 
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Algorithm", trackingAlgorithm.ToString());
            w.WriteElementString("SearchWindow", XmlHelper.WriteSize(searchWindow));
            w.WriteElementString("BlockWindow", XmlHelper.WriteSize(blockWindow));
            w.WriteElementString("SimilarityThreshold", XmlHelper.WriteFloat((float)similarityThreshold));
            w.WriteElementString("TemplateUpdateThreshold", XmlHelper.WriteFloat((float)templateUpdateThreshold));
            w.WriteElementString("RefinementNeighborhood", string.Format(CultureInfo.InvariantCulture, "{0}", refinementNeighborhood));
        }

        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Algorithm":
                        trackingAlgorithm = XmlHelper.ParseEnum<TrackingAlgorithm>(r.ReadElementContentAsString(), TrackingAlgorithm.Correlation);
                        break;
                    case "SearchWindow":
                        searchWindow = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        break;
                    case "BlockWindow":
                        blockWindow = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        break;
                    case "SimilarityThreshold":
                        similarityThreshold = r.ReadElementContentAsDouble();
                        break;
                    case "TemplateUpdateThreshold":
                        templateUpdateThreshold = r.ReadElementContentAsDouble();
                        break;
                    case "RefinementNeighborhood":
                        refinementNeighborhood = r.ReadElementContentAsInt();
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }
            
            r.ReadEndElement();
        }
    
        /// <summary>
        /// Make sure the search window is at least as large as the template window.
        /// </summary>
        private void FitBlockWindow()
        {
            if (trackingAlgorithm != TrackingAlgorithm.Correlation)
                return;

            bool fit = blockWindow.Width <= searchWindow.Width && blockWindow.Height <= searchWindow.Height;
            if (!fit)
            {
                searchWindow = blockWindow;
            }
        }
    }
}
