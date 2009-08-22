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

using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using AForge.Imaging.Filters;

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
        public List<String> Comments
        {
            get { return m_Comments; }
            set { m_Comments = value; }  
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
        #endregion

        #region Members
        private long m_Position = -1;            // Position is absolute in all timestamps.
        private string  m_Title = "";
        private string m_Timecode = "";
        private List<string> m_Comments = new List<string>();
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
            m_FullFrame = ConvertToJPG(_image);
        	m_ParentMetadata = _ParentMetadata;
        }
        #endregion

        #region Public Interface
        public void ImportImage(Bitmap _image)
        {
            m_Thumbnail = new Bitmap(_image, 100, 75);
            m_FullFrame = ConvertToJPG(_image);
        }
        public void GenerateDisabledThumbnail()
        {
            m_DisabledThumbnail = ConvertToGrayscale(m_Thumbnail);
        }
        public void AddDrawing(AbstractDrawing obj)
        {
            // insert to the top of z-order
            m_Drawings.Insert(0, obj);
        }
        public void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Keyframe");

            // Position
            _xmlWriter.WriteStartElement("Position");
            _xmlWriter.WriteString(m_Position.ToString());
            _xmlWriter.WriteEndElement();

            // Title
            _xmlWriter.WriteStartElement("Title");
            _xmlWriter.WriteString(Title);
            _xmlWriter.WriteEndElement();

            // UserTime
            _xmlWriter.WriteStartElement("UserTime");
           	string userTime = m_ParentMetadata.m_TimeStampsToTimecodeCallback(m_Position, TimeCodeFormat.Unknown, false);
            _xmlWriter.WriteString(userTime );
            _xmlWriter.WriteEndElement();
            
            // Comments
            if (m_Comments.Count > 0)
            {
                _xmlWriter.WriteStartElement("CommentLines");
                foreach (string line in m_Comments)
                {
                    _xmlWriter.WriteStartElement("CommentLine");
                    _xmlWriter.WriteString(line);
                    _xmlWriter.WriteEndElement();
                }
                _xmlWriter.WriteEndElement();
            }

            // Drawings
            if (m_Drawings.Count > 0)
            {
                _xmlWriter.WriteStartElement("Drawings");
                foreach (AbstractDrawing drawing in m_Drawings)
                {
                    drawing.ToXmlString(_xmlWriter);
                }
                _xmlWriter.WriteEndElement();
            }


            // </Keyframe>
            _xmlWriter.WriteEndElement();
        }
        public override int GetHashCode()
        {
            // Combine (XOR) all hash code for drawings, then comments, then title.

            int iHashCode = 0;
            foreach (AbstractDrawing drawing in m_Drawings)
            {
                iHashCode ^= drawing.GetHashCode();
            }

            foreach (String comment in m_Comments)
            {
                iHashCode ^= comment.GetHashCode();
            }

            if(m_Title != null)
            {
            	iHashCode ^= m_Title.GetHashCode();
            }
            
            iHashCode ^= m_Timecode.GetHashCode();

            return iHashCode;
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
        private Bitmap ConvertToGrayscale(Bitmap _source)
        {
            Bitmap bm;
            
            // AForge filter = fast.
            GrayscaleBT709 filter = new GrayscaleBT709();
            bm = filter.Apply(_source);
            
            return bm;
        }
        public Bitmap ConvertToJPG(Bitmap _image)
        {
            // Intermediate MemoryStream for the conversion.
            MemoryStream memStr = new MemoryStream();

            //Get the list of available encoders
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            //find the encoder with the image/jpeg mime-type
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                {
                    ici = codec;
                }
            }

            if (ici != null)
            {
                //Create a collection of encoder parameters (we only need one in the collection)
                EncoderParameters ep = new EncoderParameters();

                //We'll store images at 90% quality as compared with the original
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)90);

                _image.Save(memStr, ici, ep);
            }
            else
            {
                // No JPG encoder found (is that common ?) Use default system.
                _image.Save(memStr, ImageFormat.Jpeg);
            }

            return new Bitmap(memStr);
        }
        #endregion
    }
}
