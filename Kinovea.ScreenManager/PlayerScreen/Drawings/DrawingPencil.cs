#region License
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
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Pencil")]
    public class DrawingPencil : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public override string DisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolPencil; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = 0;
                foreach (PointF p in pointList)
                    hash ^= p.GetHashCode();
            
                hash ^= m_StyleHelper.ContentHash;
                hash ^= m_InfosFading.ContentHash;

                return hash;
            }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return m_Style;}
        }
        public override InfosFading InfosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        #endregion

        #region Members
        private List<PointF> pointList = new List<PointF>();
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingPencil(Point _origin, Point _second, long _iTimestamp, long _AverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            pointList.Add(_origin);
            pointList.Add(_second);
            m_InfosFading = new InfosFading(_iTimestamp, _AverageTimeStampsPerFrame);
            
            m_StyleHelper.Color = Color.Black;
            m_StyleHelper.LineSize = 1;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
        }
        public DrawingPencil(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, Point.Empty, 0, 0, ToolManager.Pencil.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, IImageToViewportTransformer _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if(fOpacityFactor <= 0)
                return;
            
            using(Pen penLine = m_StyleHelper.GetPen(fOpacityFactor, _transformer.Scale))
            {
                Point[] points = _transformer.Transform(pointList).ToArray();
                _canvas.DrawCurve(penLine, points, 0.5f);
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            pointList = pointList.Select(p => p.Translate(dx, dy)).ToList();
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = m_InfosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0 && IsPointInObject(point, transformer))
                result = 0;
                
            return result;
        }
        #endregion

        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(_xmlReader.Name)
                {
                    case "PointList":
                        ParsePointList(_xmlReader, _scale);
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
            
            _xmlReader.ReadEndElement();
        }
        private void ParsePointList(XmlReader _xmlReader, PointF _scale)
        {
            pointList.Clear();
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                if(_xmlReader.Name == "Point")
                {
                    PointF p = XmlHelper.ParsePointF(_xmlReader.ReadElementContentAsString());
                    PointF adapted = p.Scale(_scale.X, _scale.Y);
                    pointList.Add(adapted);
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("PointList");
            _xmlWriter.WriteAttributeString("Count", pointList.Count.ToString());
            foreach (PointF p in pointList)
                _xmlWriter.WriteElementString("Point", string.Format(CultureInfo.InvariantCulture, "{0};{1}", p.X, p.Y));

            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement(); 
        }
        #endregion
        
        #region IInitializable implementation
        public void ContinueSetup(PointF point, Keys modifiers)
        {
            AddPoint(point, modifiers);
        }
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "pen size");
        }
        private void AddPoint(PointF point, Keys modifiers)
        {
            PointF newPoint = PointF.Empty;
            int pointsUsedToComputeDirection = Math.Min(10, pointList.Count);
            
            if((modifiers & Keys.Shift) == Keys.Shift)
            {
                // Checks whether the mouse motion is more horizontal or vertical, and only keep this component of the motion.
                float dx = Math.Abs(point.X - pointList[pointList.Count - pointsUsedToComputeDirection].X);
                float dy = Math.Abs(point.Y - pointList[pointList.Count - pointsUsedToComputeDirection].Y);
                if(dx > dy)
                    newPoint = new PointF(point.X, pointList[pointList.Count - 1].Y);
                else
                    newPoint = new PointF(pointList[pointList.Count - 1].X, point.Y);
            }
            else
            {
                newPoint = point;
            }
            
            pointList.Add(newPoint);
        }
        private bool IsPointInObject(Point point, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath path = new GraphicsPath())
            {
                path.AddCurve(pointList.ToArray(), 0.5f);
                RectangleF bounds = path.GetBounds();
                if (bounds.IsEmpty)
                    return false;

                return HitTester.HitTest(path, point, m_StyleHelper.LineSize, false, transformer);
            }
        }
        #endregion
    }
}