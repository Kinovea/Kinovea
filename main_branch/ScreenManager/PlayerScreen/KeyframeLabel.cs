/*
Copyright © Joan Charmant 2008-2009.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// A class to encapsulate a little label.
	/// Mainly used for Keyframe labels but could be used for small textual labels too.
	/// The label is comprised of a reference position, 
	/// a background possibly shifted away, and a connector to link to two.
	/// The background position can be absolute or relative to the ref point.
	/// The ref point is stored in TrackPos.
	/// Background shift is directly stored in Background.Location. 
	/// </summary>
    public class KeyframeLabel
    {
        #region Properties
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }
        public InfosTextDecoration TextDecoration
        {
        	get { return m_TextDecoration;}
        }
        public TrackPosition TrackPos
        {
        	get { return m_TrackPos;}
        	set 
        	{
        		m_TrackPos = value;
        		if(!m_bBackgroundIsRelative)
        		{
        			// We must reset the background position.
        			Background.Location = new Point(m_TrackPos.X + 25, m_TrackPos.Y);
        		}
        	}
        	
        }
        #endregion

        public long iTimestamp;                 // Absolute.
        public TrackPosition RescaledTrackPos = new TrackPosition(0, 0, 0);
        public Rectangle Background;            // Absolute positionning (as original image size)
        public Rectangle RescaledBackground;    // Relative positionning (as current image size)
        public bool m_bBackgroundIsRelative;

        #region Members
        private string m_Text;
        private InfosTextDecoration m_TextDecoration;
        private double m_fRescaledFontSize;
        private TrackPosition m_TrackPos = new TrackPosition(0, 0, 0);
        #endregion

        #region Construction
        public KeyframeLabel()
            : this(false, Color.Black)
        {
        }
        public KeyframeLabel(bool _bIsBackgroundRelative, Color _color)
        {
        	m_bBackgroundIsRelative = _bIsBackgroundRelative;
            Background = new Rectangle(-20, -50, 1, 1);
            m_Text = "Label";
            m_TextDecoration = new InfosTextDecoration("Arial", 8, FontStyle.Bold, Color.White, Color.FromArgb(160, _color));
			m_fRescaledFontSize = (double)m_TextDecoration.FontSize;
        }
        #endregion

        public bool HitTest(Point _point)
        {
            // _point is mouse coordinates already descaled.
            int iOffsetX = 0;
            int iOffsetY = 0;
            if (m_bBackgroundIsRelative)
            {
                iOffsetX = m_TrackPos.X;
                iOffsetY = m_TrackPos.Y;
            }

            Rectangle hitRect = new Rectangle(Background.X + iOffsetX, Background.Y + iOffsetY, Background.Width, Background.Height);

            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddRectangle(hitRect);
            Region areaRegion = new Region(areaPath);
            return areaRegion.IsVisible(_point);
        }
        public void ResetBackground(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            // Scale and shift background before drawing.

            // 1. Unscaled values
            Button but = new Button();
            Graphics g = but.CreateGraphics();
            SizeF bgSize = g.MeasureString( " " + m_Text + " ", m_TextDecoration.GetInternalFont() );
            g.Dispose();
            Background.Width = (int)bgSize.Width + 8;
            Background.Height = (int)bgSize.Height + 4;

            // 2. Scaled values.
            Rescale(_fStretchFactor, _DirectZoomTopLeft);
        }
        public void Rescale(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
			m_fRescaledFontSize = (double)m_TextDecoration.FontSize * _fStretchFactor;
			            
            RescaledBackground = new Rectangle();
            if (!m_bBackgroundIsRelative)
            {
                RescaledBackground.Location = new Point((int)((double)(Background.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(Background.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            }
            else
            {
                RescaledBackground.Location = new Point((int)((double)Background.X * _fStretchFactor), (int)((double)Background.Y * _fStretchFactor));
            }

            RescaledBackground.Size = new Size((int)((double)Background.Width * _fStretchFactor), (int)((double)Background.Height * _fStretchFactor));
            
            if(m_TrackPos != null)
            {
            	RescaledTrackPos = new TrackPosition((int)((double)(m_TrackPos.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_TrackPos.Y - _DirectZoomTopLeft.Y) * _fStretchFactor), m_TrackPos.T);
            }
        }
        public override int GetHashCode()
        {
            int iHash = 0;
            
            if (m_bBackgroundIsRelative)
            {
                iHash ^= m_Text.GetHashCode(); 
            }
            iHash ^= m_TextDecoration.GetHashCode();
            iHash ^= Background.Location.GetHashCode();
            
            return iHash;
        }
        
        #region XML conversion
        public void ToXml(XmlTextWriter _xmlWriter, long _iBeginTimeStamp)
        {
            _xmlWriter.WriteStartElement("KeyframeLabel");

            _xmlWriter.WriteStartElement("SpacePosition");
            _xmlWriter.WriteString(Background.Left.ToString() + ";" + Background.Top.ToString());
            _xmlWriter.WriteEndElement();

            _xmlWriter.WriteStartElement("TimePosition");
            long ts = 0;
            if (!m_bBackgroundIsRelative)
            {
                ts = m_TrackPos.T + _iBeginTimeStamp;
            }
            _xmlWriter.WriteString(ts.ToString());
            _xmlWriter.WriteEndElement();

            m_TextDecoration.ToXml(_xmlWriter);

            // </KeyframeLabel>
            _xmlWriter.WriteEndElement();
        }
        public static KeyframeLabel FromXml(XmlTextReader _xmlReader, bool _relative, PointF _scale)
        {
            // Read all tags between <KeyframeLabel> and </KeyframeLabel> and fills up an object.

            KeyframeLabel kfl = new KeyframeLabel(_relative, Color.Black);

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "SpacePosition")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        Point adapted = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                        
                        kfl.Background = new Rectangle(adapted, new Size(10, 10));
                        kfl.RescaledBackground = new Rectangle(adapted, new Size(10, 10));
                    }
                    else if (_xmlReader.Name == "TimePosition")
                    {
                        // Time was stored absolute.
                        kfl.iTimestamp = long.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "TextDecoration")
                    {
                    	kfl.m_TextDecoration = InfosTextDecoration.FromXml(_xmlReader);
                    }
                    /*else if (_xmlReader.Name == "BackgroundBrush")
                    {
                        ParseBackgroundBrush(_xmlReader, kfl);
                    }
                    else if (_xmlReader.Name == "Font")
                    {
                        ParseFont(_xmlReader, kfl);
                    }*/
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "KeyframeLabel")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }  

            return kfl;
        }
		#endregion
		
        #region Drawing
        public void Draw(Graphics _canvas, double _fFadingFactor)
        {
            DrawBackground(_canvas, _fFadingFactor);
            DrawText(_canvas, _fFadingFactor);
        }
        private void DrawBackground(Graphics _canvas, double _fFadingFactor)
        {
            // The roundness of the rectangle is computed from the font size.
            int radius = (int)(m_fRescaledFontSize / 2);
            int diameter = radius * 2;

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            int iOffsetX = 0;
            int iOffsetY = 0;
            if (m_bBackgroundIsRelative)
            {
                // If we are on relative coords (i.e: drawing the main label that follows the target)
                // then the background "top left" is actually relative to the current position.
                // (i.e: can be negative.)
                // Hence we need add to it the current track position.

                iOffsetX = RescaledTrackPos.X;
                iOffsetY = RescaledTrackPos.Y;
            }

            gp.AddArc(RescaledBackground.X + iOffsetX, RescaledBackground.Y + iOffsetY, diameter, diameter, 180, 90);
            gp.AddLine(RescaledBackground.X + iOffsetX + radius, RescaledBackground.Y + iOffsetY, RescaledBackground.X + iOffsetX + RescaledBackground.Width - diameter, RescaledBackground.Y + iOffsetY);

            gp.AddArc(RescaledBackground.X + iOffsetX + RescaledBackground.Width - diameter, RescaledBackground.Y + iOffsetY, diameter, diameter, 270, 90);
            gp.AddLine(RescaledBackground.X + iOffsetX + RescaledBackground.Width, RescaledBackground.Y + iOffsetY + radius, RescaledBackground.X + iOffsetX + RescaledBackground.Width, RescaledBackground.Y + iOffsetY + RescaledBackground.Height - diameter);

            gp.AddArc(RescaledBackground.X + iOffsetX + RescaledBackground.Width - diameter, RescaledBackground.Y + iOffsetY + RescaledBackground.Height - diameter, diameter, diameter, 0, 90);
            gp.AddLine(RescaledBackground.X + iOffsetX + RescaledBackground.Width - radius, RescaledBackground.Y + iOffsetY + RescaledBackground.Height, RescaledBackground.X + iOffsetX + radius, RescaledBackground.Y + iOffsetY + RescaledBackground.Height);

            gp.AddArc(RescaledBackground.X + iOffsetX, RescaledBackground.Y + iOffsetY + RescaledBackground.Height - diameter, diameter, diameter, 90, 90);
            gp.AddLine(RescaledBackground.X + iOffsetX, RescaledBackground.Y + iOffsetY + RescaledBackground.Height - radius, RescaledBackground.X + iOffsetX, RescaledBackground.Y + iOffsetY + radius);

            gp.CloseFigure();

            Color fadingColor = m_TextDecoration.GetFadingBackColor(_fFadingFactor);
            Color moreFadingColor = m_TextDecoration.GetFadingBackColor(_fFadingFactor/4);
            
            // Small dot
            _canvas.FillEllipse(new SolidBrush(fadingColor), RescaledTrackPos.X - 2, RescaledTrackPos.Y - 2, 4, 4);
            
            // Connector
            _canvas.DrawLine(new Pen(moreFadingColor), RescaledTrackPos.X, RescaledTrackPos.Y, RescaledBackground.X + iOffsetX + RescaledBackground.Width / 2, RescaledBackground.Y + iOffsetY + RescaledBackground.Height / 2);

            // Rounded rectangle
            _canvas.FillPath(new SolidBrush(fadingColor), gp);
        }
        private void DrawText(Graphics _canvas, double _fFadingFactor)
        {
            int iOffsetX = 0;
            int iOffsetY = 0;
            if (m_bBackgroundIsRelative)
            {
                // see comment in DrawBackground
                iOffsetX = RescaledTrackPos.X;
                iOffsetY = RescaledTrackPos.Y;
            }

            // TODO: we should be able to get a font at the right size given a multiplicator somehow.
            // and use GetInternalFont(double)
            Font f = m_TextDecoration.GetInternalFont();
            Font fontText = new Font(f.FontFamily, (float)m_fRescaledFontSize, f.Style);
            
            _canvas.DrawString(" " + m_Text, fontText, new SolidBrush(m_TextDecoration.GetFadingForeColor(_fFadingFactor)), new Point(RescaledBackground.X + iOffsetX + 5, RescaledBackground.Y + iOffsetY + 3));
        }
        #endregion
    }
}
