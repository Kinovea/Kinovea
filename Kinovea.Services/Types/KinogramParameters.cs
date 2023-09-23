using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using System.ComponentModel;

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
        /// Set of indices of crop positions that were set manually.
        /// This is used to support interpolation.
        /// </summary>
        public HashSet<int> ManualPositions { get; set; } = new HashSet<int>();


        /// <summary>
        /// List of frame offsets.
        /// Frame offset for each tile relative to normal interpolation.
        /// Defaults to zero for every tile.
        /// </summary>
        public List<int> FrameOffsets { get; set; } = new List<int>();


        /// <summary>
        /// Whether to automatically interpolate non-manually placed positions.
        /// When this is true moving a single tile will also move all the other tiles
        /// to interpolate between the manually anchored ones.
        /// If this is false the other tiles won't be interpolated until we use the 
        /// interpolate menu manually.
        /// When using the Reset tile menu, if this is true the reset tile will 
        /// be interpolated, if this is false it will be reset to empty.
        /// </summary>
        public bool AutoInterpolate { get; set; } = true;

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

        /// <summary>
        /// Type of measurement used in the labels.
        /// Supported: None, Time, Frame.
        /// </summary>
        public MeasureLabelType MeasureLabelType { get; set; } = MeasureLabelType.None;

        /// <summary>
        /// Background color of the mini labels.
        /// </summary>
        public Color LabelColor { get; set; } = Color.FromArgb(0, 0, 0);

        /// <summary>
        /// Size of the font for the mini labels.
        /// </summary>
        public int LabelSize { get; set; } = 8;

        // TODO:
        // Direction bullets (small arrows between tiles).

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

            clone.ManualPositions = new HashSet<int>();
            foreach (int index in this.ManualPositions)
                clone.ManualPositions.Add(index);

            clone.FrameOffsets = new List<int>();
            foreach (int offset in this.FrameOffsets)
                clone.FrameOffsets.Add(offset);

            clone.AutoInterpolate = this.AutoInterpolate;
            clone.LeftToRight = this.LeftToRight;
            clone.BorderColor = this.BorderColor;
            clone.BorderVisible = this.BorderVisible;
            clone.LabelColor = this.LabelColor;
            clone.LabelSize = this.LabelSize;
            clone.MeasureLabelType = this.MeasureLabelType;

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
            
            foreach (int index in ManualPositions)
                hash ^= index.GetHashCode();

            foreach (int offset in FrameOffsets)
                hash ^= offset.GetHashCode();

            hash ^= AutoInterpolate.GetHashCode();
            hash ^= LeftToRight.GetHashCode();
            hash ^= BorderColor.GetHashCode();
            hash ^= BorderVisible.GetHashCode();
            hash ^= LabelColor.GetHashCode();
            hash ^= LabelSize.GetHashCode();
            hash ^= MeasureLabelType.GetHashCode();
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
                    case "AutoInterpolate":
                        AutoInterpolate = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
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
                    case "LabelColor":
                        LabelColor = XmlHelper.ParseColor(r.ReadElementContentAsString(), Color.FromArgb(0, 0, 0));
                        break;
                    case "LabelSize":
                        LabelSize = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "MeasureLabelType":
                        MeasureLabelType = XmlHelper.ParseEnum<MeasureLabelType>(r.ReadElementContentAsString(), MeasureLabelType.None);
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
            FrameOffsets.Clear();
            bool empty = r.IsEmptyElement;

            r.ReadStartElement();
            if (empty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "CropPosition":
                        bool isAnchor = false;
                        if (r.MoveToAttribute("anchor"))
                            isAnchor = XmlHelper.ParseBoolean(r.ReadContentAsString());

                        if (r.MoveToAttribute("offset"))
                        {
                            int offset = int.Parse(r.ReadContentAsString());
                            FrameOffsets.Add(offset);
                        }

                        r.ReadStartElement();
                        CropPositions.Add(XmlHelper.ParsePointF(r.ReadContentAsString()));
                        
                        if (isAnchor)
                            ManualPositions.Add(CropPositions.Count - 1);

                        r.ReadEndElement();
                        break;

                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        r.ReadOuterXml();
                        break;
                }
            }

            if (FrameOffsets.Count < CropPositions.Count)
            {
                for (int i = 0; i < CropPositions.Count - FrameOffsets.Count; i++)
                {
                    FrameOffsets.Add(0);
                }
            }
            else if (FrameOffsets.Count > CropPositions.Count)
            {
                FrameOffsets.RemoveRange(CropPositions.Count, FrameOffsets.Count - CropPositions.Count);
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
            {
                w.WriteStartElement("CropPosition");

                if (ManualPositions.Contains(i))
                    w.WriteAttributeString("anchor", "true");

                w.WriteAttributeString("offset", FrameOffsets[i].ToString(CultureInfo.InvariantCulture));

                w.WriteString(XmlHelper.WritePointF(CropPositions[i]));
                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteElementString("AutoInterpolate", XmlHelper.WriteBoolean(AutoInterpolate));
            w.WriteElementString("LeftToRight", XmlHelper.WriteBoolean(LeftToRight));
            w.WriteElementString("BorderColor", XmlHelper.WriteColor(BorderColor, false));
            w.WriteElementString("BorderVisible", XmlHelper.WriteBoolean(BorderVisible));
            w.WriteElementString("LabelColor", XmlHelper.WriteColor(LabelColor, false));
            w.WriteElementString("LabelSize", LabelSize.ToString(CultureInfo.InvariantCulture));

            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(MeasureLabelType));
            string xmlMeasureLabelType = enumConverter.ConvertToString(MeasureLabelType);
            w.WriteElementString("MeasureLabelType", xmlMeasureLabelType);
        }
        #endregion
    }
}
