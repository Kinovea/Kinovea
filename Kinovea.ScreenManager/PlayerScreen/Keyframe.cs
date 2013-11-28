/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public long Position
        {
            get { return m_Position; }
            set { m_Position = value;}
        }
        public Bitmap Thumbnail
        {
            get { return m_Thumbnail; }
            //set { m_Thumbnail = value; }
        }
        public Bitmap DisabledThumbnail
        {
            get { return m_DisabledThumbnail; }
            set { m_DisabledThumbnail = value; }
        }
        public List<AbstractDrawing> Drawings
        {
            get { return m_Drawings; }
            set { m_Drawings = value;}
        }
        public Bitmap FullFrame
        {
            get { return m_FullFrame; }
            set { m_FullFrame = value; }
        }
        public string CommentRtf
        {
            get { return m_CommentRtf; }
            set { m_CommentRtf = value; }
        }
        /// <summary>
        /// The title of a keyframe is dynamic.
        /// It is the timecode until the user actually manually changes it.
        /// </summary>
        public String Title
        {
            get 
            { 
                if(m_Title != null)
                {
                    if(m_Title.Length > 0)
                    {
                        return m_Title;
                    }
                    else 
                    {
                        return m_Timecode;
                    }
                }
                else
                {
                    return m_Timecode;
                }
            }
            set 
            { 
                m_Title = value;
                m_ParentMetadata.UpdateTrajectoriesForKeyframes();
            }
        }
        public String TimeCode
        {
            get { return m_Timecode; }
            set { m_Timecode = value; }
        }
        public bool Disabled
        {
            get { return m_bDisabled; }
            set { m_bDisabled = value; }
        }
        public Metadata ParentMetadata
        {
            get { return m_ParentMetadata; }    // unused.
            set { m_ParentMetadata = value; }
        }
        public int ContentHash
        {
            get { return GetContentHash();}
        }
        #endregion

        #region Members
        private long m_Position = -1;            // Position is absolute in all timestamps.
        private string  m_Title = "";
        private string m_Timecode = "";
        private string m_CommentRtf;
        private Bitmap m_Thumbnail;
        private Bitmap m_DisabledThumbnail;
        private List<AbstractDrawing> m_Drawings = new List<AbstractDrawing>();
        private Bitmap m_FullFrame;
        private bool m_bDisabled;
        private Metadata m_ParentMetadata;
        #endregion

        #region Constructor
        public Keyframe(Metadata _ParentMetadata)
        {
            // Used only during parsing to hold dummy Keyframe while it is loaded.
            // Must be followed by a call to PostImportMetadata()
            m_ParentMetadata = _ParentMetadata;
        }
        public Keyframe(long _position, string _timecode, Bitmap _image, Metadata _ParentMetadata)
        {
            // Title is a variable default.
            // as long as it's null, it takes the value of timecode.
            // which is updated when selection change.
            // as soon as the user put value in title, we use it instead.
            m_Position = _position;
            m_Timecode = _timecode;
            m_Thumbnail = new Bitmap(_image, 100, 75);
            m_FullFrame = ImageHelper.ConvertToJPG(_image, 90);
            m_ParentMetadata = _ParentMetadata;
        }
        #endregion

        #region Public Interface
        public void ImportImage(Bitmap _image)
        {
            m_Thumbnail = new Bitmap(_image, 100, 75);
            m_FullFrame = ImageHelper.ConvertToJPG(_image, 90);
        }
        public void GenerateDisabledThumbnail()
        {
            m_DisabledThumbnail = Grayscale.CommonAlgorithms.BT709.Apply(m_Thumbnail);
        }
        public void AddDrawing(AbstractDrawing obj)
        {
            // insert to the top of z-order except for grids.
            if(obj is DrawingPlane)
                m_Drawings.Add(obj);
            else
                m_Drawings.Insert(0, obj);
        }
        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("Position");
            string userTime = m_ParentMetadata.TimeStampsToTimecode(m_Position, false, TimecodeFormat.Unknown, false);
            w.WriteAttributeString("UserTime", userTime);
            w.WriteString(m_Position.ToString());
            w.WriteEndElement();
            
            if(!string.IsNullOrEmpty(Title))
                w.WriteElementString("Title", Title);
            
            if(!string.IsNullOrEmpty(m_CommentRtf))
                w.WriteElementString("Comment", m_CommentRtf);
            
            if (m_Drawings.Count > 0)
            {
                w.WriteStartElement("Drawings");
                foreach (AbstractDrawing drawing in m_Drawings)
                {
                    IKvaSerializable serializableDrawing = drawing as IKvaSerializable;
                    if(serializableDrawing != null)
                    {
                        // The XML name for this drawing should be stored in its [XMLType] C# attribute.
                        Type t = serializableDrawing.GetType();
                        object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                    
                        if(attributes.Length > 0)
                        {
                            string xmlName = ((XmlTypeAttribute)attributes[0]).TypeName;
                            
                            w.WriteStartElement(xmlName);
                            serializableDrawing.WriteXml(w);
                            w.WriteEndElement();
                        }
                    }
                }
                w.WriteEndElement();
            }
        }
        #endregion

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if(obj is Keyframe)
            {
                return this.m_Position.CompareTo(((Keyframe)obj).m_Position);
            }
            else
            {
                throw new ArgumentException("Impossible comparison");
            }
        }
        #endregion

        #region LowerLevel Helpers
        private int GetContentHash()
        {
            int iHashCode = 0;
            foreach (AbstractDrawing drawing in m_Drawings)
                iHashCode ^= drawing.ContentHash;

            if(m_CommentRtf != null)
                iHashCode ^= m_CommentRtf.GetHashCode();
            
            if(m_Title != null)
                iHashCode ^= m_Title.GetHashCode();
            
            iHashCode ^= m_Timecode.GetHashCode();

            return iHashCode;
        }
        #endregion
    }
}
