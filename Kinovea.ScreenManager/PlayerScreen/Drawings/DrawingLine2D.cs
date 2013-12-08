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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
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
    [XmlType ("Line")]
    public class DrawingLine2D : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, ITrackable, IMeasurable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler ShowMeasurableInfoChanged;
        #endregion
        
        #region Properties
        public override string DisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolLine2D; }
        }
        public override int ContentHash
        {
            get 
            {
                int iHash = 0;
                iHash ^= m_StyleHelper.ContentHash;
                iHash ^= ShowMeasurableInfo.GetHashCode();
                iHash ^= m_InfosFading.ContentHash;
                return iHash;
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
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.Track; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                // Rebuild the menu to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                mnuShowMeasure.Text = ScreenManagerLang.mnuShowMeasure;
                mnuShowMeasure.Checked = ShowMeasurableInfo;
                mnuSealMeasure.Text = ScreenManagerLang.mnuCalibrate;
                
                contextMenu.Add(mnuShowMeasure);
                contextMenu.Add(mnuSealMeasure);
                
                return contextMenu; 
            }
        }
        
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private bool tracking;
        
        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private KeyframeLabel m_LabelMeasure;
        private InfosFading m_InfosFading;
        
        // Context menu
        private ToolStripMenuItem mnuShowMeasure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSealMeasure = new ToolStripMenuItem();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingLine2D(Point _start, Point _end, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            points["a"] = _start;
            points["b"] = _end;
            m_LabelMeasure = new KeyframeLabel(GetMiddlePoint(), Color.Black);
            
            // Decoration
            m_StyleHelper.Color = Color.DarkSlateGray;
            m_StyleHelper.LineSize = 1;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
            // Context menu
            mnuShowMeasure.Click += new EventHandler(mnuShowMeasure_Click);
            mnuShowMeasure.Image = Properties.Drawings.measure;
            mnuSealMeasure.Click += new EventHandler(mnuSealMeasure_Click);
            mnuSealMeasure.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingLine2D(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, Point.Empty, 0, 0, ToolManager.Line.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, IImageToViewportTransformer _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if(tracking)
                fOpacityFactor = 1.0;
            
            if(fOpacityFactor <= 0)
                return;
            
            Point start = _transformer.Transform(points["a"]);
            Point end = _transformer.Transform(points["b"]);
            
            using(Pen penEdges = m_StyleHelper.GetPen((int)(fOpacityFactor * 255), _transformer.Scale))
            {
                _canvas.DrawLine(penEdges, start, end);
                
                // Handlers
                penEdges.Width = _bSelected ? 2 : 1;
                if(m_StyleHelper.LineEnding.StartCap != LineCap.ArrowAnchor)
                    _canvas.DrawEllipse(penEdges, start.Box(3));
                
                if(m_StyleHelper.LineEnding.EndCap != LineCap.ArrowAnchor)
                    _canvas.DrawEllipse(penEdges, end.Box(3));
            }

            if(ShowMeasurableInfo)
            {
                // Text of the measure. (The helpers knows the unit)
                PointF a = new PointF(points["a"].X, points["a"].Y);
                PointF b = new PointF(points["b"].X, points["b"].Y);
                m_LabelMeasure.SetText(CalibrationHelper.GetLengthText(a, b, true, true));
                m_LabelMeasure.Draw(_canvas, _transformer, fOpacityFactor);
            }
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = m_InfosFading.GetOpacityFactor(currentTimestamp);
            int boxSide = transformer.Untransform(6);
            
            if (tracking || opacity > 0)
            {
                if(ShowMeasurableInfo && m_LabelMeasure.HitTest(point, transformer))
                    result = 3;
                else if (points["a"].Box(boxSide).Contains(point))
                    result = 1;
                else if (points["b"].Box(boxSide).Contains(point))
                    result = 2;
                else if (IsPointInObject(point))
                    result = 0;
            }
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            int constraintAngleSubdivisions = 8; // (Constraint by 45° steps).
            switch(handleNumber)
            {
                case 1:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["a"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["b"], point, constraintAngleSubdivisions);
                    else
                        points["a"] = point;

                    m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
                    SignalTrackablePointMoved("a");
                    break;
                case 2:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["b"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["a"], point, constraintAngleSubdivisions);
                    else
                        points["b"] = point;

                    m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
                    SignalTrackablePointMoved("b");
                    break;
                case 3:
                    // Move the center of the mini label to the mouse coord.
                    m_LabelMeasure.SetLabel(point);
                    break;
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys, bool zooming)
        {
            points["a"] = points["a"].Translate(dx, dy);
            points["b"] = points["b"].Translate(dx, dy);
            m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
            SignalAllTrackablePointsMoved();
        }
        #endregion

        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            if (_xmlReader.MoveToAttribute("id"))
                id = new Guid(_xmlReader.ReadContentAsString());

            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(_xmlReader.Name)
                {
                    case "Start":
                        {
                            PointF p = XmlHelper.ParsePointF(_xmlReader.ReadElementContentAsString());
                            points["a"] = p.Scale(_scale.X, _scale.Y);
                            break;
                        }
                    case "End":
                        {
                            PointF p = XmlHelper.ParsePointF(_xmlReader.ReadElementContentAsString());
                            points["b"] = p.Scale(_scale.X, _scale.Y);
                            break;
                        }
                    case "DrawingStyle":
                        m_Style = new DrawingStyle(_xmlReader);
                        BindStyle();
                        break;
                    case "InfosFading":
                        m_InfosFading.ReadXml(_xmlReader);
                        break;
                    case "MeasureVisible":
                        ShowMeasurableInfo = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = _xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            _xmlReader.ReadEndElement();
            
            m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
            SignalAllTrackablePointsMoved();
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteElementString("Start", String.Format(CultureInfo.InvariantCulture, "{0};{1}", points["a"].X, points["a"].Y));
            _xmlWriter.WriteElementString("End", String.Format(CultureInfo.InvariantCulture, "{0};{1}", points["b"].X, points["b"].Y));
            _xmlWriter.WriteElementString("MeasureVisible", ShowMeasurableInfo ? "true" : "false");
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();  

            if(ShowMeasurableInfo)
            {
                // Spreadsheet support.
                _xmlWriter.WriteStartElement("Measure");
                
                PointF a = CalibrationHelper.GetPoint(new PointF(points["a"].X, points["a"].Y));
                PointF b = CalibrationHelper.GetPoint(new PointF(points["b"].X, points["b"].Y));

                float len = GeometryHelper.GetDistance(a, b);
                string value = String.Format("{0:0.00}", len);
                string valueInvariant = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", len);

                _xmlWriter.WriteAttributeString("UserLength", value);
                _xmlWriter.WriteAttributeString("UserLengthInvariant", valueInvariant);
                _xmlWriter.WriteAttributeString("UserUnitLength", CalibrationHelper.GetLengthAbbreviation());
                
                _xmlWriter.WriteEndElement();
            }
        }
        #endregion
        
        #region IInitializable implementation
        public void ContinueSetup(PointF point, Keys modifiers)
        {
            MoveHandle(point, 2, modifiers);
        }
        #endregion
        
        #region ITrackable implementation and support.
        public TrackingProfile CustomTrackingProfile
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, PointF value)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            foreach(KeyValuePair<string, PointF> p in points)
                TrackablePointMoved(this, new TrackablePointMovedEventArgs(p.Key, p.Value));
        }
        private void SignalTrackablePointMoved(string name)
        {
            if(TrackablePointMoved == null || !points.ContainsKey(name))
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs(name, points[name]));
        }
        #endregion
        
        public float Length()
        {
            return GeometryHelper.GetDistance(points["a"], points["b"]);
        }
        
        #region Context menu
        private void mnuShowMeasure_Click(object sender, EventArgs e)
        {
            // Enable / disable the display of the measure for this line.
            ShowMeasurableInfo = !ShowMeasurableInfo;
            if(ShowMeasurableInfoChanged != null)
                ShowMeasurableInfoChanged(this, EventArgs.Empty);
            
            CallInvalidateFromMenu(sender);
        }
        
        private void mnuSealMeasure_Click(object sender, EventArgs e)
        {
            if(points["a"].X == points["b"].X && points["a"].Y == points["b"].Y)
                return;
            
            if(!ShowMeasurableInfo)
            {
                ShowMeasurableInfo = true;
                if(ShowMeasurableInfoChanged != null)
                    ShowMeasurableInfoChanged(this, EventArgs.Empty);
            }
            
            FormCalibrateLine fcm = new FormCalibrateLine(CalibrationHelper, this);
            FormsHelper.Locate(fcm);
            fcm.ShowDialog();
            fcm.Dispose();
            
            CallInvalidateFromMenu(sender);
        }
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "line size");
            m_Style.Bind(m_StyleHelper, "LineEnding", "arrows");
        }
        private bool IsPointInObject(Point _point)
        {
            bool hit = false;
            
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                if(points["a"] == points["b"])
                    areaPath.AddLine(points["a"].X, points["a"].Y, points["a"].X + 2, points["a"].Y + 2);
                else
                    areaPath.AddLine(points["a"], points["b"]);
            
                using(Pen areaPen = new Pen(Color.Black, 7))
                {
                    areaPath.Widen(areaPen);
                }
                using(Region r = new Region(areaPath))
                {
                    hit = r.IsVisible(_point);
                }
            }
            
            return hit;
        }
        private Point GetMiddlePoint()
        {
            // Used only to attach the measure.
            return GeometryHelper.GetMiddlePoint(points["a"], points["b"]).ToPoint();
        }
        
        #endregion
    }
}
