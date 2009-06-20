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
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;
using Kinovea.Services;
using System.Resources;
using System.Threading;
using System.Reflection;

namespace Kinovea.ScreenManager
{
    public class DrawingCross2D : AbstractDrawing
    {
        #region Properties
        public override DrawingToolType ToolType
        {
        	get { return DrawingToolType.Cross2D; }
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        
        // Next 2 props are accessed from Track creation.
        public Point CenterPoint 
		{
			get { return m_CenterPoint; }
		}
        public Color PenColor
        {
        	get { return m_PenStyle.Color; }
        }
        #endregion

        #region Members
		// Position
        private Point m_CenterPoint;           
		private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        
        // Decoration
        private LineStyle m_PenStyle;
        private LineStyle m_MemoPenStyle;
        private InfosFading m_InfosFading;
        private static readonly int m_iDefaultBackgroundAlpha = 64;
        private static readonly int m_iDefaultRadius = 3;
               
        // Computed
        private Point RescaledCenterPoint;
        #endregion

        #region Constructors
        public DrawingCross2D(int x, int y, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
            // Position
            m_CenterPoint = new Point(x, y);
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
            // Decoration
            m_PenStyle = new LineStyle(1, LineShape.Simple, Color.CornflowerBlue);
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            int iPenAlpha = (int)((double)255 * fOpacityFactor);

            if (iPenAlpha > 0)
            {
                // Rescale the points.
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                // Cross
                Pen PenEdges = m_PenStyle.GetInternalPen(iPenAlpha); //new Pen(Color.FromArgb(iPenAlpha, PenColor), 1);
                _canvas.DrawLine(PenEdges, RescaledCenterPoint.X - m_iDefaultRadius, RescaledCenterPoint.Y, RescaledCenterPoint.X + m_iDefaultRadius, RescaledCenterPoint.Y);
                _canvas.DrawLine(PenEdges, RescaledCenterPoint.X, RescaledCenterPoint.Y - m_iDefaultRadius, RescaledCenterPoint.X, RescaledCenterPoint.Y + m_iDefaultRadius);

                // Background
                _canvas.FillEllipse(new SolidBrush(Color.FromArgb((int)((double)m_iDefaultBackgroundAlpha * fOpacityFactor), m_PenStyle.Color)), RescaledCenterPoint.X - m_iDefaultRadius - 1, RescaledCenterPoint.Y - m_iDefaultRadius - 1, (m_iDefaultRadius * 2) + 2, (m_iDefaultRadius * 2) + 2);
            }
        }
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            // Not implemented (No handlers)
        }
        public override void MoveDrawing(int _deltaX, int _deltaY)
        {
            // _delatX and _delatY are mouse delta already descaled.
            m_CenterPoint.X += _deltaX;
            m_CenterPoint.Y += _deltaY;

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object.
            
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                if (IsPointInObject(_point))
                {
                    iHitResult = 0;
                }
            }
            
            return iHitResult;
        }
        public override void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingCross2D");

            // CenterPoint
            _xmlWriter.WriteStartElement("CenterPoint");
            _xmlWriter.WriteString(m_CenterPoint.X.ToString() + ";" + m_CenterPoint.Y.ToString());
            _xmlWriter.WriteEndElement();

            m_PenStyle.ToXml(_xmlWriter);
            m_InfosFading.ToXml(_xmlWriter, false);

            // </Drawing>
            _xmlWriter.WriteEndElement();
        }
        public static AbstractDrawing FromXml(XmlTextReader _xmlReader, PointF _scale)
        {
            DrawingCross2D dc = new DrawingCross2D(0,0,0,0);

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "CenterPoint")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        dc.m_CenterPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    }
                    else if (_xmlReader.Name == "LineStyle")
                    {
                        dc.m_PenStyle = LineStyle.FromXml(_xmlReader);   
                    }
                    else if (_xmlReader.Name == "InfosFading")
                    {
                        dc.m_InfosFading.FromXml(_xmlReader);
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "Drawing")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

            dc.RescaleCoordinates(dc.m_fStretchFactor, dc.m_DirectZoomTopLeft);
            return dc;
        }
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            return rm.GetString("ToolTip_DrawingToolCross2D", Thread.CurrentThread.CurrentUICulture);
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_CenterPoint.GetHashCode();
            iHash ^= m_PenStyle.GetHashCode();
            return iHash;
        }
        
        public override void UpdateDecoration(Color _color)
        {
        	m_PenStyle.Update(_color);
        }
        public override void UpdateDecoration(LineStyle _style)
        {
        	// Actually not used for now.
        	m_PenStyle.Update(_style, false, true, true);	
        }
        public override void UpdateDecoration(int _iFontSize)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));
        }
        public override void MemorizeDecoration()
        {
        	m_MemoPenStyle = m_PenStyle.Clone();
        }
        public override void RecallDecoration()
        {
        	m_PenStyle = m_MemoPenStyle.Clone();
        }
        
        #endregion

        #region Lower level helpers
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            RescaledCenterPoint = new Point((int)((double)(m_CenterPoint.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_CenterPoint.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private bool IsPointInObject(Point _point)
        {
            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            Pen areaPen = new Pen(Color.Black, 7);

            areaPath.AddLine(m_CenterPoint.X - m_iDefaultRadius, m_CenterPoint.Y, m_CenterPoint.X + m_iDefaultRadius, m_CenterPoint.Y);
            areaPath.Widen(areaPen);

            // Create region from the path
            Region areaRegion = new Region(areaPath);

            return areaRegion.IsVisible(_point);
        }
        #endregion

    }
}
