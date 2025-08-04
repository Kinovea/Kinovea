#region License
/*
Copyright � Joan Charmant 2008
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class Keyframe : AbstractDrawingManager, IComparable
    {
        #region Properties
        public override Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        public long Timestamp
        {
            get { return timestamp; }
            set 
            { 
                timestamp = value;
                foreach (AbstractDrawing d in Drawings)
                    d.UpdateReferenceTime(timestamp);
            }
        }
        public Bitmap Thumbnail
        {
            get { return thumbnail; }
        }
        public Bitmap DisabledThumbnail
        {
            get { return disabledThumbnail; }
        }
        public bool HasThumbnails
        {
            get { return thumbnail != null; }
        }
        public List<AbstractDrawing> Drawings
        {
            get { return drawings; }
        }
        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        public string Name
        {
            get 
            {
                return string.IsNullOrEmpty(name) ? TimeCode : name;
            }
            set 
            { 
                name = value;
                parentMetadata.UpdateTrajectoriesForKeyframes();
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
        public string TimeCode
        {
            get { return parentMetadata.TimeCodeBuilder(this.timestamp, TimeType.UserOrigin, TimecodeFormat.Unknown, true); }
        }
        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }
        public int ContentHash
        {
            get { return GetContentHash();}
        }
        public static Color DefaultColor
        {
            get { return defaultColor; }
        }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private long timestamp = -1;            // Absolute timestamp.
        private string name;
        private string comments;
        private Bitmap thumbnail;
        private Bitmap disabledThumbnail;
        public static readonly Color defaultColor = Color.SteelBlue;
        private Color color = defaultColor;
        private List<AbstractDrawing> drawings = new List<AbstractDrawing>();
        private bool disabled;
        private Metadata parentMetadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        /// <summary>
        /// General constructor used to build an empty keyframe or load it from KVA XML.
        /// </summary>
        public Keyframe(long timestamp, string name, Color color, Metadata metadata)
        {
            this.timestamp = timestamp;
            this.name = name;
            this.color = color;
            this.parentMetadata = metadata;
        }

        /// <summary>
        /// Constructor used by external serializers like OpenPose and Subtitles which build the keyframe data in advance.
        /// </summary>
        public Keyframe(Guid id, long timestamp, string name, Color color, string comments, List<AbstractDrawing> drawings, Metadata metadata)
        {
            this.id = id;
            this.timestamp = timestamp;
            this.name = name;
            this.color = color;
            this.comments = comments;
            this.drawings = drawings;
            this.parentMetadata = metadata;
        }
        public Keyframe(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(0, "", defaultColor, metadata)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region Public Interface
        public void InitializePosition(long timestamp)
        {
            this.timestamp = timestamp;
        }
        public void InitializeImage(Bitmap image)
        {
            if (image == null)
                return;
            
            Rectangle rect = UIHelper.RatioStretch(image.Size, new Size(100, 75));
            this.thumbnail = new Bitmap(image, rect.Width, rect.Height);
            this.disabledThumbnail = BitmapHelper.Grayscale(thumbnail);
        }
        #endregion

        #region AbstractDrawingManager implementation
        public override AbstractDrawing GetDrawing(Guid id)
        {
            return drawings.FirstOrDefault(d => d.Id == id);
        }
        public override void AddDrawing(AbstractDrawing drawing)
        {
            // insert to the top of z-order except for grids.
            if (drawing is DrawingPlane)
                drawings.Add(drawing);
            else
                drawings.Insert(0, drawing);
        }
        public override void RemoveDrawing(Guid id)
        {
            drawings.RemoveAll(d => d.Id == id);
        }
        public override void Clear()
        {
            drawings.Clear();
        }
        #endregion

        #region KVA Serialization
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteStartElement("Timestamp");
                w.WriteString(timestamp.ToString());
                w.WriteEndElement();

                if (!string.IsNullOrEmpty(Name))
                    w.WriteElementString("Name", Name);

                w.WriteElementString("Color", XmlHelper.WriteColor(color, false));

                if (!string.IsNullOrEmpty(comments))
                    w.WriteElementString("Comment", comments);
            }

            if (ShouldSerializeKVA(filter))
            {
                if (drawings.Count == 0)
                    return;

                // Drawings are written in reverse order to match order of addition.
                w.WriteStartElement("Drawings");
                for (int i = drawings.Count - 1; i >= 0; i--)
                {
                    IKvaSerializable serializableDrawing = drawings[i] as IKvaSerializable;
                    if (serializableDrawing == null)
                        continue;

                    DrawingSerializer.Serialize(w, serializableDrawing, SerializationFilter.KVA);
                }

                w.WriteEndElement();
            }
        }

        public bool ShouldSerializeCore(SerializationFilter filter)
        {
            return (filter & SerializationFilter.Core) == SerializationFilter.Core;
        }

        public bool ShouldSerializeKVA(SerializationFilter filter)
        {
            return (filter & SerializationFilter.KVA) == SerializationFilter.KVA;
        }

        public MeasuredDataKeyframe CollectMeasuredData()
        {
            MeasuredDataKeyframe md = new MeasuredDataKeyframe();
            md.Name = Name;
            md.Time = parentMetadata.GetNumericalTime(timestamp, TimeType.UserOrigin);
            md.Comment = comments;
            return md;
        }

        public void ReadXml(XmlReader r, PointF scale, TimestampMapper timestampMapper)
        {
            if (r.MoveToAttribute("id"))
                id = new Guid(r.ReadContentAsString());

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Position":
                    case "Timestamp":
                        int inputTimestamp = r.ReadElementContentAsInt();
                        timestamp = timestampMapper(inputTimestamp);
                        break;
                    case "Title":
                    case "Name":
                        name = r.ReadElementContentAsString();
                        break;
                    case "Color":
                        color = XmlHelper.ParseColor(r.ReadElementContentAsString(), Color.SteelBlue);
                        break;
                    case "Comment":
                        comments = r.ReadElementContentAsString();

                        // Note: XML spec specifies that any CRLF must be converted to single LF.
                        // This breaks the comparison between saved data and read data.
                        // Force CRLF back.
                        comments = comments.Replace("\n", "\r\n");
                        break;
                    case "Drawings":
                        ParseDrawings(r, scale);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();
        }
        private void ParseDrawings(XmlReader r, PointF scale)
        {
            // Note:
            // We do not do a metadata.AddDrawing at this point, only add the drawing internally to the keyframe without side-effects.
            // We wait for the complete keyframe to be parsed and ready, and then merge-insert it into the existing collection.
            // The drawing post-initialization will only be done at that point. 
            // This prevents doing post-initializations too early when the keyframe is not yet added to the metadata collection.
            // Note this also runs in the context of Undo of "Delete keyframe" action.

            bool isEmpty = r.IsEmptyElement;
            
            r.ReadStartElement();

            if (isEmpty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = DrawingSerializer.Deserialize(r, scale, TimeHelper.IdentityTimestampMapper, parentMetadata);
                if (drawing == null || !drawing.IsValid || drawings.Any(d => d.Id == drawing.Id))
                    continue;

                
                AddDrawing(drawing);
                drawing.ParentMetadata = this.parentMetadata;
                drawing.ReferenceTimestamp = this.Timestamp;
                drawing.InfosFading.ReferenceTimestamp = this.Timestamp;
                drawing.InfosFading.AverageTimeStampsPerFrame = this.parentMetadata.AverageTimeStampsPerFrame;
            }

            r.ReadEndElement();
        }
        
        #endregion

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if(obj is Keyframe)
                return this.timestamp.CompareTo(((Keyframe)obj).timestamp);
            else
                throw new ArgumentException("Impossible comparison");
        }
        #endregion

        #region LowerLevel Helpers
        private int GetContentHash()
        {
            int hash = 0;
            foreach (AbstractDrawing drawing in drawings)
                hash ^= drawing.ContentHash;

            if(comments != null)
                hash ^= comments.GetHashCode();

            if (!string.IsNullOrEmpty(name))
                hash ^= name.GetHashCode();

            hash ^= color.GetHashCode();

            hash ^= timestamp.GetHashCode();

            return hash;

        }
        #endregion
    }
}
