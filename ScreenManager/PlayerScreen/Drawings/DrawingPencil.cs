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
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingPencil : AbstractDrawing, IXMLSerializable, IDecorable, IInitializable
    {
        #region Properties
        public DrawingType DrawingType
        {
        	get { return DrawingType.Pencil; }
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public override Capabilities Caps
		{
			get { return Capabilities.ConfigureColorSize | Capabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        #endregion

        #region Members
        
        // Core & decoration
        private List<Point> m_PointList;
        private LineStyle m_PenStyle;
        private LineStyle m_MemoPenStyle;
        private double m_fStretchFactor;
        private InfosFading m_InfosFading;
        private Point m_DirectZoomTopLeft;
        // Computed
        private List<Point> m_RescaledPointList;
        #endregion

        #region Constructors
        public DrawingPencil() : this(0, 0, 0, 0, 0, 0)
        {
        }
        public DrawingPencil(int x1, int y1, int x2, int y2, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            m_PointList = new List<Point>();
            m_PointList.Add(new Point(x1, y1));
            m_PointList.Add(new Point(x2, y2));

            m_InfosFading = new InfosFading(_iTimestamp, _AverageTimeStampsPerFrame);
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
            m_PenStyle = new LineStyle(1, LineShape.Simple, Color.Black);
            
            // Computed
            m_RescaledPointList = new List<Point>();
            m_RescaledPointList.Add(RescalePoint(new Point(x1, y1), m_fStretchFactor));
            m_RescaledPointList.Add(RescalePoint(new Point(x2, y2), m_fStretchFactor));

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

                float fPenWidth = (float)((double)m_PenStyle.Size * m_fStretchFactor);
                if (fPenWidth < 1) fPenWidth = 1;

                Pen penLine = m_PenStyle.GetInternalPen(iPenAlpha, fPenWidth);
                
                Point[] points = new Point[m_RescaledPointList.Count];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = new Point(m_RescaledPointList[i].X, m_RescaledPointList[i].Y);
                }
                _canvas.DrawCurve(penLine, points, 0.5f);
                
                penLine.Dispose();
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            for(int i=0;i<m_PointList.Count;i++)
            {
                m_PointList[i] = new Point(m_PointList[i].X + _deltaX, m_PointList[i].Y + _deltaY);
            }

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object, 1+: on handle.
            
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
        #endregion

		#region IXMLSerializable implementation
        public void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingPencil");

            // Points
            _xmlWriter.WriteStartElement("PointList");
            _xmlWriter.WriteAttributeString("Count", m_PointList.Count.ToString());
            foreach (Point p in m_PointList)
            {
                _xmlWriter.WriteStartElement("Point");
                _xmlWriter.WriteString(p.X.ToString() + ";" + p.Y.ToString());
                _xmlWriter.WriteEndElement();
            }
            _xmlWriter.WriteEndElement();

            m_PenStyle.ToXml(_xmlWriter);
            m_InfosFading.ToXml(_xmlWriter, false);

            // </Drawing>
            _xmlWriter.WriteEndElement();
        }
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            return rm.GetString("ToolTip_DrawingToolPencil", Thread.CurrentThread.CurrentUICulture);
        }
        public override int GetHashCode()
        {
            // combine all relevant fields with XOR to get the Hash.

            int iHashCode = 0;
            foreach (Point p in m_PointList)
            {
                iHashCode ^= p.GetHashCode();
            }

            iHashCode ^= m_PenStyle.GetHashCode();

            return iHashCode;
        }
       
        #region IDecorable implementation
        public void UpdateDecoration(Color _color)
        {
        	m_PenStyle.Update(_color);
        }
        public void UpdateDecoration(LineStyle _style)
        {
        	m_PenStyle.Update(_style, false, true, true);
        }
        public void UpdateDecoration(int _iFontSize)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));
        }
        public void MemorizeDecoration()
        {
        	m_MemoPenStyle = m_PenStyle.Clone();
        }
        public void RecallDecoration()
        {
        	m_PenStyle = m_MemoPenStyle.Clone();
        }
        #endregion

        #region IInitializable implementation
        public void ContinueSetup(Point point)
		{
			AddPoint(point);
		}
        #endregion
        
        public void AddPoint(Point _coordinates)
        {
            m_PointList.Add(_coordinates);
            m_RescaledPointList.Add(RescalePoint(_coordinates, m_fStretchFactor));
        }
        public static AbstractDrawing FromXml(XmlTextReader _xmlReader, PointF _scale)
        {
            DrawingPencil dp = new DrawingPencil();

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "PointList")
                    {
                        ParsePointList(dp, _xmlReader, _scale);
                    }
                    else if (_xmlReader.Name == "LineStyle")
                    {
                        dp.m_PenStyle = LineStyle.FromXml(_xmlReader);   
                    }
                    else if (_xmlReader.Name == "InfosFading")
                    {
                        dp.m_InfosFading.FromXml(_xmlReader);
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

            dp.RescaleCoordinates(dp.m_fStretchFactor, dp.m_DirectZoomTopLeft);
            return dp;
        }
        private static void ParsePointList(DrawingPencil _dp, XmlTextReader _xmlReader, PointF _scale)
        {
            _dp.m_PointList.Clear();
            _dp.m_RescaledPointList.Clear();

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Point")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');

                        Point adapted = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));

                        _dp.m_PointList.Add(adapted);
                        _dp.m_RescaledPointList.Add(adapted);
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "PointList")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }

        #region Lower level helpers
        private Point RescalePoint(Point _point, double _fStretchFactor)
        {
            return new Point((int)((double)_point.X * _fStretchFactor), (int)((double)_point.Y * _fStretchFactor));
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            for(int i=0;i<m_PointList.Count;i++)
            {
                m_RescaledPointList[i] = new Point((int)((double)(m_PointList[i].X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_PointList[i].Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            }
        }
        private bool IsPointInObject(Point _point)
        {
            // _point is descaled.

            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            
            Point[] points = new Point[m_PointList.Count];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point(m_PointList[i].X, m_PointList[i].Y);
            }
            areaPath.AddCurve(points, 0.5f);

            Pen areaPen = new Pen(Color.Black, m_PenStyle.Size + 7);
            areaPen.StartCap = LineCap.Round;
            areaPen.EndCap = LineCap.Round;
            areaPen.LineJoin = LineJoin.Round;
            
            areaPath.Widen(areaPen);
			areaPen.Dispose();
			
            // Create region from the path
            Region areaRegion = new Region(areaPath);

            return areaRegion.IsVisible(_point);
        }
        #endregion
    }
}