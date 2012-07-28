#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Spotlights.
	/// This is the proxy object dispatching all spotlights requests. (draw, hit testing, etc.)
	/// </summary>
	public class SpotlightManager : AbstractMultiDrawing, IInitializable, IKvaSerializable
	{
		#region Properties
		public override object SelectedItem {
		    get 
		    {
                if(m_iSelected >= 0 && m_iSelected < m_Spotlights.Count)
                    return m_Spotlights[m_iSelected];
                else
                    return null;
		    }
		}
        public override int Count {
		    get { return m_Spotlights.Count; }
        }
		
		// Fading is not currently modifiable from outside.
        public override InfosFading  infosFading
        {
            get { throw new NotImplementedException("Spotlight, The method or operation is not implemented."); }
            set { throw new NotImplementedException("Spotlight, The method or operation is not implemented."); }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.None; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
		#endregion
		
		#region Members
		private List<Spotlight> m_Spotlights = new List<Spotlight>();
		private int m_iSelected = -1;
		private static readonly int m_iDefaultBackgroundAlpha = 150; // <-- opacity of the dim layer. Higher value => darker.
		#endregion
		
		#region AbstractDrawing Implementation
		public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
		{
		    // We draw a single translucent black rectangle to cover the whole image.
			// (Opacity varies between 0% and 50%, depending on the opacity factor of the closest spotlight in time)
			if(m_Spotlights.Count < 1)
			    return;
			
			// Create a mask rectangle and obliterate spotlights from it.
			// FIXME: spots subtract from each other which is not desirable.
			// TODO: might be better to first get the opacity, then only ask for the path. In case opacity is 0.
			GraphicsPath globalPath = new GraphicsPath();
			globalPath.AddRectangle(_canvas.ClipBounds);
			
			// Combine all spots into a single GraphicsPath.
			// Get their opacity in the process to compute the global opacity of the covering rectangle.
			double maxOpacity = 0.0;
			GraphicsPath spotsPath = new GraphicsPath();
			foreach(Spotlight spot in m_Spotlights)
			{
				double opacity = spot.AddSpot(_iCurrentTimestamp, spotsPath, _transformer);
				maxOpacity = Math.Max(maxOpacity, opacity);
			}
			
			if(maxOpacity <= 0)
                return;
			
			// Obliterate the spots from the mask.
			globalPath.AddPath(spotsPath, false);
			
			// Draw the mask with the spot holes on top of the frame.
			int backgroundAlpha = (int)((double)m_iDefaultBackgroundAlpha * maxOpacity);
			using(SolidBrush brushBackground = new SolidBrush(Color.FromArgb(backgroundAlpha, Color.Black)))
			//using(SolidBrush brushBackground = new SolidBrush(Color.FromArgb(backgroundAlpha, Color.FromArgb(255, 0, 0, 16))))
			{
                _canvas.FillPath(brushBackground, globalPath);
            }
			
			// Draw each spot border or any visuals.
            foreach(Spotlight spot in m_Spotlights)
                spot.Draw(_canvas, _transformer, _iCurrentTimestamp);
            
            globalPath.Dispose();
            spotsPath.Dispose();
		}
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
		    if(m_iSelected >= 0 && m_iSelected < m_Spotlights.Count)
				m_Spotlights[m_iSelected].MouseMove(_deltaX, _deltaY);
		}
		public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
		{
		    if(m_iSelected >= 0 && m_iSelected < m_Spotlights.Count)
				m_Spotlights[m_iSelected].MoveHandleTo(point);
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
		    int currentSpot = 0;
		    int handle = -1;
		    foreach(Spotlight spot in m_Spotlights)
		    {
		        handle = spot.HitTest(_point, _iCurrentTimestamp);
		        if(handle >= 0)
		        {
		            m_iSelected = currentSpot;
		            break;
		        }
		        currentSpot++;
		    }
		    
		    return handle;
		}
		#endregion
		
		#region AbstractMultiDrawing Implementation
		public override void Add(object _item)
        {
            Spotlight spotlight = _item as Spotlight;
            if(spotlight == null)
                return;
            
		    m_Spotlights.Add(spotlight);
		    m_iSelected = m_Spotlights.Count - 1;
		}
        public override void Remove(object _item)
		{
            Spotlight spotlight = _item as Spotlight;
            if(spotlight == null)
                return;
            
		    m_Spotlights.Remove(spotlight);
		    m_iSelected = -1;
		}
        public override void Clear()
        {
            m_Spotlights.Clear();
            m_iSelected = -1;
        }
		#endregion
		
		#region IInitializable implementation
        public void ContinueSetup(Point point, Keys modifiers)
		{
			MoveHandle(point, -1, modifiers);
		}
        #endregion
        
        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            /*_xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Origin":
				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        m_Center = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
				        break;
					case "Radius":
				        int radius = _xmlReader.ReadElementContentAsInt();
                        m_iRadius = (int)((double)radius * _scale.X);
                        break;
					case "DrawingStyle":
						m_Style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
				    case "InfosFading":
						m_InfosFading.ReadXml(_xmlReader);
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();*/
        }
        public void WriteXml(XmlWriter _xmlWriter)
		{
            /*_xmlWriter.WriteElementString("Origin", String.Format("{0};{1}", m_Center.X, m_Center.Y));
            _xmlWriter.WriteElementString("Radius", m_iRadius.ToString());
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();*/
        }
        #endregion
        
		#region Public methods
		public override string ToString()
        {
            return "Spotlight"; //ScreenManagerLang.ToolTip_DrawingToolSpotlight;
        }
		public void Add(Point _point, long _iPosition, long _iAverageTimeStampsPerFrame)
		{
		    // Equivalent to GetNewDrawing() for regular drawing tools.
			m_Spotlights.Add(new Spotlight(_iPosition, _iAverageTimeStampsPerFrame, _point));
			m_iSelected = m_Spotlights.Count - 1;
		}
		#endregion
	}
}

