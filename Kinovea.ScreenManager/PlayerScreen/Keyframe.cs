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
    public class Keyframe : IComparable
    {
        #region Properties
        public Guid Id
        {
            get { return id; }
            set { id = value; }
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

        /// <summary>
        /// The title of a keyframe is set to the timecode until the user manually set it.
        /// </summary>
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
        private long position = -1;            // Position is absolute in all timestamps.
        private string  title = "";
        private string timecode = "";
        private string comments;
        private Bitmap thumbnail;
        private Bitmap disabledThumbnail;
        private List<AbstractDrawing> drawings = new List<AbstractDrawing>();
        private Bitmap fullFrame;
        private bool disabled;
        private Metadata metadata;
        #endregion

        #region Constructor
        public Keyframe(Metadata metadata)
        {
            // Used only during parsing to hold dummy Keyframe while it is loaded.
            // Must be followed by a call to PostImportMetadata()
            this.metadata = metadata;
        }
        public Keyframe(long position, string timecode, Bitmap image, Metadata metadata)
        {
            // Title is a variable default.
            // as long as it's null, it takes the value of timecode (which is updated when selection change).
            // as soon as the user put value in title, we use it instead.
            this.position = position;
            this.timecode = timecode;
            this.thumbnail = new Bitmap(image, 100, 75);
            this.fullFrame = ImageHelper.ConvertToJPG(image, 90);
            this.metadata = metadata;
        }
        #endregion

        #region Public Interface
        public void ImportImage(Bitmap image)
        {
            this.thumbnail = new Bitmap(image, 100, 75);
            this.fullFrame = ImageHelper.ConvertToJPG(image, 90);
        }
        public void GenerateDisabledThumbnail()
        {
            disabledThumbnail = Grayscale.CommonAlgorithms.BT709.Apply(thumbnail);
        }
        public AbstractDrawing GetDrawing(Guid id)
        {
            return drawings.FirstOrDefault(d => d.Id == id);
        }
        public void AddDrawing(AbstractDrawing drawing)
        {
            // insert to the top of z-order except for grids.
            if(drawing is DrawingPlane)
                drawings.Add(drawing);
            else
                drawings.Insert(0, drawing);
        }
        public void RemoveDrawing(Guid id)
        {
            drawings.RemoveAll(d => d.Id == id);
        }
        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("Position");
            string userTime = metadata.TimeCodeBuilder(position - metadata.SelectionStart, TimeType.Time, TimecodeFormat.Unknown, false);
            w.WriteAttributeString("UserTime", userTime);
            w.WriteString(position.ToString());
            w.WriteEndElement();
            
            if(!string.IsNullOrEmpty(Title))
                w.WriteElementString("Title", Title);
            
            if(!string.IsNullOrEmpty(comments))
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
