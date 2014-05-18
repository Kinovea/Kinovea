#region License
/*
Copyright © Joan Charmant 2008
joan.charmant@gmail.com 
 
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

using AForge.Imaging.Filters;
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
        public bool Initialized
        {
            get { return initialized; }
        }
        public long Position
        {
            get { return position; }
            set { position = value;}
        }
        public Bitmap Thumbnail
        {
            get { return thumbnail; }
        }
        public Bitmap DisabledThumbnail
        {
            get { return disabledThumbnail; }
        }
        public List<AbstractDrawing> Drawings
        {
            get { return drawings; }
        }
        public Bitmap FullFrame
        {
            get { return fullFrame; }
        }
        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        public string Title
        {
            get 
            {
                return string.IsNullOrEmpty(title) ? timecode : title;
            }
            set 
            { 
                title = value;
                metadata.UpdateTrajectoriesForKeyframes();
            }
        }
        public string TimeCode
        {
            get { return timecode; }
            set { timecode = value; }
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
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private bool initialized;
        private long position = -1;            // Position is absolute in all timestamps.
        private string title;
        private string timecode;
        private string comments;
        private Bitmap fullFrame;
        private Bitmap thumbnail;
        private Bitmap disabledThumbnail;
        private List<AbstractDrawing> drawings = new List<AbstractDrawing>();
        private bool disabled;
        private Metadata metadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Keyframe(long position, string timecode, Metadata metadata)
        {
            this.position = position;
            this.timecode = timecode;
            this.metadata = metadata;
        }
        public Keyframe(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(0, "", metadata)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region Public Interface
        public void Initialize(long position, Bitmap image)
        {
            this.position = position;
            
            if (image != null)
            {
                this.thumbnail = new Bitmap(image, 100, 75);
                this.fullFrame = ImageHelper.ConvertToJPG(image, 90);
                this.disabledThumbnail = Grayscale.CommonAlgorithms.BT709.Apply(thumbnail);
            }
            
            initialized = true;
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
        #endregion

        #region KVA Serialization
        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("Position");
            string userTime = metadata.TimeCodeBuilder(position - metadata.SelectionStart, TimeType.Time, TimecodeFormat.Unknown, false);
            w.WriteAttributeString("UserTime", userTime);
            w.WriteString(position.ToString());
            w.WriteEndElement();

            if (!string.IsNullOrEmpty(Title))
                w.WriteElementString("Title", Title);

            if (!string.IsNullOrEmpty(comments))
                w.WriteElementString("Comment", comments);

            if (drawings.Count == 0)
                return;

            w.WriteStartElement("Drawings");
            foreach (AbstractDrawing drawing in drawings)
            {
                IKvaSerializable serializableDrawing = drawing as IKvaSerializable;
                if (serializableDrawing == null)
                    continue;

                DrawingSerializer.Serialize(w, serializableDrawing);
            }
            w.WriteEndElement();
        }
        private void ReadXml(XmlReader r, PointF scale, TimestampMapper timestampMapper)
        {
            if (r.MoveToAttribute("id"))
                id = new Guid(r.ReadContentAsString());

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Position":
                        int inputPosition = r.ReadElementContentAsInt();
                        position = timestampMapper(inputPosition, false);
                        break;
                    case "Title":
                        title = r.ReadElementContentAsString();
                        break;
                    case "Comment":
                        comments = r.ReadElementContentAsString();
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
            // TODO: catch empty tag <Drawings/>.

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = DrawingSerializer.Deserialize(r, scale, metadata, TimeHelper.IdentityTimestampMapper);
                metadata.AddDrawing(this, drawing);
            }

            r.ReadEndElement();
        }
        
        #endregion

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if(obj is Keyframe)
                return this.position.CompareTo(((Keyframe)obj).position);
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
            
            if(title != null)
                hash ^= title.GetHashCode();
            
            hash ^= timecode.GetHashCode();

            return hash;
        }
        #endregion
    }
}
