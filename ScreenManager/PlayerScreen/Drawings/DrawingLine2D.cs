#region License
/*
Copyright © Joan Charmant 2008-2011.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingLine2D : AbstractDrawing, IXMLSerializable, IDecorable, IInitializable
    {
        #region Properties
        public DrawingType DrawingType
        {
        	get { return DrawingType.Line; }
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public Metadata ParentMetadata
        {
            // get => unused.
            set { m_ParentMetadata = value; }
        }
		public bool ShowMeasure 
		{
			get { return m_bShowMeasure; }
			set { m_bShowMeasure = value; }
		}
        #endregion

        #region Members
        // Core
        public Point m_StartPoint;            	// Public because also used for the Active Screen Bordering...
        public Point m_EndPoint;				// Idem.
        
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        
        // Computed
        private Point m_RescaledStartPoint;
        private Point m_RescaledEndPoint;

        // Fading
        private InfosFading m_InfosFading;
        
        // Decoration
        private LineStyle m_PenStyle;
        private LineStyle m_MemoPenStyle;
        private KeyframeLabel m_LabelMeasure;
        private bool m_bShowMeasure;
        private Metadata m_ParentMetadata;
        #endregion

        #region Constructors
        public DrawingLine2D(int x1, int y1, int x2, int y2, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
            // Core
            m_StartPoint = new Point(x1, y1);
            m_EndPoint = new Point(x2, y2);
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            m_PenStyle = new LineStyle(1, LineShape.Simple, Color.DarkSlateGray);

            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            
            m_LabelMeasure = new KeyframeLabel(GetMiddlePoint(), Color.Black);
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            int iPenAlpha = (int)((double)255 * fOpacityFactor);

            if (iPenAlpha > 0)
            {
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                Pen penEdges = m_PenStyle.GetInternalPen(iPenAlpha);
                
                _canvas.DrawLine(penEdges, m_RescaledStartPoint.X, m_RescaledStartPoint.Y, m_RescaledEndPoint.X, m_RescaledEndPoint.Y);

                // Handlers
                penEdges.Width = 1;

                if (_bSelected) 
                    penEdges.Width++;

                if(m_PenStyle.Shape == LineShape.Simple)
                {
                	_canvas.DrawEllipse(penEdges, GetRescaledHandleRectangle(1));
                	_canvas.DrawEllipse(penEdges, GetRescaledHandleRectangle(2));
                }
                else if(m_PenStyle.Shape == LineShape.EndArrow)
                {
                	_canvas.DrawEllipse(penEdges, GetRescaledHandleRectangle(1));
                }
                
                penEdges.Dispose();
                
                if(m_bShowMeasure)
                {
                	// Text of the measure. (The helpers knows the unit)
	                string text = m_ParentMetadata.CalibrationHelper.GetLengthText(m_StartPoint, m_EndPoint);
	                m_LabelMeasure.Text = text;
	                
                    // Draw.
	                m_LabelMeasure.Draw(_canvas, _fStretchFactor, _DirectZoomTopLeft, fOpacityFactor);
                }
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // Move the specified handle to the specified coordinates.
            // In Line2D, handles are directly mapped to the endpoints of the line.

            // _point is mouse coordinates already descaled.
            switch(handleNumber)
            {
            	case 1:
            		m_StartPoint = point;
                    m_LabelMeasure.MoveTo(GetMiddlePoint());
            		break;
            	case 2:
            		m_EndPoint = point;
                    m_LabelMeasure.MoveTo(GetMiddlePoint());
            		break;
            	case 3:
            		// Move the center of the mini label to the mouse coord.
            		m_LabelMeasure.MoveLabel(point);
            		break;
            }

            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            m_StartPoint.X += _deltaX;
            m_StartPoint.Y += _deltaY;

            m_EndPoint.X += _deltaX;
            m_EndPoint.Y += _deltaY;

            m_LabelMeasure.MoveTo(GetMiddlePoint());
            
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
            	if(m_bShowMeasure && m_LabelMeasure.HitTest(_point))
            	{
            		iHitResult = 3;
            	}
            	else if (GetHandleRectangle(1).Contains(_point))
                {
                    iHitResult = 1;
                }
                else if (GetHandleRectangle(2).Contains(_point))
                {
                    iHitResult = 2;
                }
                else
                {
                    if (IsPointInObject(_point))
                    {
                        iHitResult = 0;
                    }
                }
            }
            return iHitResult;
        }
        #endregion

		#region IXMLSerializable implementation
        public void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingLine2D");
            
            // m_StartPoint
            _xmlWriter.WriteStartElement("m_StartPoint");
            _xmlWriter.WriteString(m_StartPoint.X.ToString() + ";" + m_StartPoint.Y.ToString());
            _xmlWriter.WriteEndElement();

            // m_EndPoint
            _xmlWriter.WriteStartElement("m_EndPoint");
            _xmlWriter.WriteString(m_EndPoint.X.ToString() + ";" + m_EndPoint.Y.ToString());
            _xmlWriter.WriteEndElement();

            // Color, Style, Fading.
            m_PenStyle.ToXml(_xmlWriter);
            m_InfosFading.ToXml(_xmlWriter, false);
            
            // Show measure.
            _xmlWriter.WriteStartElement("MeasureIsVisible");
            _xmlWriter.WriteString(m_bShowMeasure.ToString());
            _xmlWriter.WriteEndElement();

            if(m_bShowMeasure)
            {
            	// This is only for spreadsheet export support. These values are not read at import.
            	_xmlWriter.WriteStartElement("Measure");
            	
            	double len = m_ParentMetadata.CalibrationHelper.GetLengthInUserUnit(m_StartPoint, m_EndPoint);
	            string value = String.Format("{0:0.00}", len);
	            string valueInvariant = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", len);

            	_xmlWriter.WriteAttributeString("UserLength", value);
            	_xmlWriter.WriteAttributeString("UserLengthInvariant", valueInvariant);
            	_xmlWriter.WriteAttributeString("UserUnitLength", m_ParentMetadata.CalibrationHelper.GetLengthAbbreviation());
            	
            	_xmlWriter.WriteEndElement();
            }
            
            _xmlWriter.WriteEndElement();// </Drawing>
        }
        #endregion

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
			MoveHandle(point, 2);
		}
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            return rm.GetString("ToolTip_DrawingToolLine2D", Thread.CurrentThread.CurrentUICulture);
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_StartPoint.GetHashCode();
            iHash ^= m_EndPoint.GetHashCode();
            iHash ^= m_PenStyle.GetHashCode();

            return iHash;
        }
        public static AbstractDrawing FromXml(XmlTextReader _xmlReader, PointF _scale)
        {
            DrawingLine2D dl = new DrawingLine2D(0,0,0,0,0,0);

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "m_StartPoint")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        dl.m_StartPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    }
                    else if (_xmlReader.Name == "m_EndPoint")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        dl.m_EndPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    }
                    else if (_xmlReader.Name == "LineStyle")
                    {
                        dl.m_PenStyle = LineStyle.FromXml(_xmlReader);   
                    }
                    else if (_xmlReader.Name == "InfosFading")
                    {
                        dl.m_InfosFading.FromXml(_xmlReader);
                    }
                    else if(_xmlReader.Name == "MeasureIsVisible")
                    {
                    	dl.m_bShowMeasure = bool.Parse(_xmlReader.ReadString());
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

            dl.m_LabelMeasure.MoveTo(dl.GetMiddlePoint());
            dl.RescaleCoordinates(dl.m_fStretchFactor, dl.m_DirectZoomTopLeft);
            return dl;
        }
        

        #region Lower level helpers
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            m_RescaledStartPoint = new Point((int)((double)(m_StartPoint.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_StartPoint.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            m_RescaledEndPoint = new Point((int)((double)(m_EndPoint.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_EndPoint.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private Rectangle GetHandleRectangle(int _handle)
        {
            //----------------------------------------------------------------------------
            // This function is only used for Hit Testing.
            // The Rectangle here is bigger than the bounding box of the handlers circles.
            //----------------------------------------------------------------------------
            int widen = 6;
            if (_handle == 1)
            {
                return new Rectangle(m_StartPoint.X - widen, m_StartPoint.Y - widen, widen * 2, widen * 2);
            }
            else
            {
                return new Rectangle(m_EndPoint.X - widen, m_EndPoint.Y - widen, widen * 2, widen * 2);
            }
        }
        private Rectangle GetRescaledHandleRectangle(int _handle)
        {
            if (_handle == 1)
            {
                return new Rectangle(m_RescaledStartPoint.X - 3, m_RescaledStartPoint.Y - 3, 6, 6);
            }
            else
            {
                return new Rectangle(m_RescaledEndPoint.X - 3, m_RescaledEndPoint.Y - 3, 6, 6);
            }
        }
        private Rectangle GetShiftedRescaledHandleRectangle(int _handle, int _iLeftShift, int _iTopShift)
        {
            Rectangle handle = GetRescaledHandleRectangle(_handle);

            // Hack : we reduce the zone by 1 px each direction because the PDFSharp library will draw too big circles.
            return new Rectangle(handle.Left + _iLeftShift, handle.Top + _iTopShift + 1, handle.Width - 1, handle.Height - 1);
        }
        private bool IsPointInObject(Point _point)
        {
            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            Pen areaPen = new Pen(Color.Black, 7);

            if(m_StartPoint.X == m_EndPoint.X && m_StartPoint.Y == m_EndPoint.Y)
            {
            	// Special case
            	areaPath.AddLine(m_StartPoint.X, m_StartPoint.Y, m_EndPoint.X + 2, m_EndPoint.Y + 2);
            }
            else
            {
            	areaPath.AddLine(m_StartPoint.X, m_StartPoint.Y, m_EndPoint.X, m_EndPoint.Y);
            	
            }
            
            areaPath.Widen(areaPen);
            areaPen.Dispose();
            
            // Create region from the path
            Region areaRegion = new Region(areaPath);

            return areaRegion.IsVisible(_point);
        }
        private Point GetMiddlePoint()
        {
        	int ix = m_StartPoint.X + ((m_EndPoint.X - m_StartPoint.X)/2);
            int iy = m_StartPoint.Y + ((m_EndPoint.Y - m_StartPoint.Y)/2);
            return new Point(ix, iy);
        }
        #endregion
    }
}
