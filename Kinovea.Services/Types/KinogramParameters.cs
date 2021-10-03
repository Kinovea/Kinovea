using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// Parameters controlling the rendering of Kinograms.
    /// The controls in the configuration UI may contain more options
    /// but they are ultimately drilled down to these parameters.
    /// </summary>
    public class KinogramParameters
    {
        #region Properties
        /// <summary>
        /// Total number of tiles in the composite.
        /// </summary>
        public int TileCount { get; set; } = 18;
        
        /// <summary>
        /// Number of rows in the composite.
        /// The number of columns is always calculated from the total and rows.
        /// </summary>
        public int Rows { get; set; } = 3;

        /// <summary>
        /// Common crop size for all tiles.
        /// The size of the area of the source images we copy in the destination.
        /// </summary>
        public Size CropSize { get; set; } = new Size(400, 600);

        /// <summary>
        /// List of crop positions. 
        /// This is the top left of the crop window for each tile.
        /// </summary>
        public List<PointF> CropPositions { get; set; } = new List<PointF>();

        /// <summary>
        /// Wether time progresses from left to right or right to left.
        /// </summary>
        public bool LeftToRight { get; set; } = true;
        
        /// <summary>
        /// Color of the border between tiles.
        /// This is also the color visible when a tile is panned away from its source image.
        /// </summary>
        public Color BorderColor { get; set; } = Color.FromArgb(44, 44, 44);
        
        /// <summary>
        /// Whether to draw a border around tiles.
        /// </summary>
        public bool BorderVisible { get; set; } = true;

        // TODO:
        // legend visibility.
        // legend placement.
        // legend type (time, tile number).
        // Direction bullets (small arrows between tiles).
        // Oversampling factor: to improve quality when viewport zooming.

        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Returns a deep clone of this object.
        /// </summary>
        public KinogramParameters Clone()
        {
            KinogramParameters clone = new KinogramParameters();
            clone.TileCount = this.TileCount;
            clone.Rows = this.Rows;
            clone.CropSize = this.CropSize;
            clone.CropPositions = new List<PointF>();
            foreach (PointF p in this.CropPositions)
                clone.CropPositions.Add(p);

            clone.LeftToRight = this.LeftToRight;
            clone.BorderColor = this.BorderColor;
            clone.BorderVisible = this.BorderVisible;

            return clone;
        }

        public int GetContentHash()
        {
            int hash = 0;
            hash ^= TileCount.GetHashCode();
            hash ^= Rows.GetHashCode();
            hash ^= CropSize.GetHashCode();
            foreach (PointF cropPosition in CropPositions)
                hash ^= cropPosition.GetHashCode();

            hash ^= LeftToRight.GetHashCode();
            hash ^= BorderColor.GetHashCode();
            hash ^= BorderVisible.GetHashCode();
            return hash;
        }

        #region Serialization
        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "TileCount":
                        TileCount = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "Rows":
                        Rows = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "CropSize":
                        CropSize = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        break;
                    case "CropPositions":
                        ParseCropPositions(r);
                        break;
                    case "LeftToRight":
                        LeftToRight = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "BorderColor":
                        BorderColor = XmlHelper.ParseColor(r.ReadElementContentAsString(), Color.FromArgb(44, 44, 44));
                        break;
                    case "BorderVisible":
                        BorderVisible = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();
        }

        private void ParseCropPositions(XmlReader r)
        {
            CropPositions.Clear();
            bool empty = r.IsEmptyElement;

            r.ReadStartElement();
            if (empty)
                return;

            while(r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "CropPosition")
                    CropPositions.Add(XmlHelper.ParsePointF(r.ReadElementContentAsString()));
                else
                    r.ReadOuterXml();
            }

            r.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("TileCount", TileCount.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("Rows", Rows.ToString(CultureInfo.InvariantCulture));

            w.WriteElementString("CropSize", XmlHelper.WriteSize(CropSize));

            w.WriteStartElement("CropPositions");
            for (int i = 0; i < CropPositions.Count; i++)
                w.WriteElementString("CropPosition", XmlHelper.WritePointF(CropPositions[i]));
            w.WriteEndElement();

            w.WriteElementString("LeftToRight", XmlHelper.WriteBoolean(LeftToRight));
            w.WriteElementString("BorderColor", XmlHelper.WriteColor(BorderColor, false));
            w.WriteElementString("BorderVisible", XmlHelper.WriteBoolean(BorderVisible));
        }
        #endregion
    }
}
