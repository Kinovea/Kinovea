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
using System.Drawing;
using System.Drawing.Drawing2D;
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
    [XmlType ("Label")]
    public class DrawingText : AbstractDrawing, IKvaSerializable, IDecorable
    {
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        public TextBox EditBox
        {
            get { return m_TextBox; }
            set { m_TextBox = value;}
        }
        public PictureBox ContainerScreen
        {
            get { return m_ContainerScreen; }
            set { m_ContainerScreen = value;}
        }
        public bool EditMode
        {
            get { return m_bEditMode; }
            set 
            { 
                m_bEditMode = value;

                //-----------------------------------------------------------
                // Activate or deactivate the ScreenManager Keyboard Handler, 
                // so we can use <space>, <return>, etc.
                //-----------------------------------------------------------
                DelegatesPool dp = DelegatesPool.Instance();
                if (m_bEditMode)
                {    
                    if (dp.DeactivateKeyboardHandler != null)
                    {
                        dp.DeactivateKeyboardHandler();
                    }

                    m_TextBox.Text = m_Text;
                    m_TextBox.Location = new Point(m_ContainerScreen.Left + m_LabelBackground.Location.X + 6, m_ContainerScreen.Top + m_LabelBackground.Location.Y + 3);
                    AutoSizeTextbox();
                    m_TextBox.Visible = true;
                    m_TextBox.Focus();
                    m_TextBox.Select(m_TextBox.Text.Length, 0);
                }
                else
                {
                    m_TextBox.Visible = false;

                    if (dp.ActivateKeyboardHandler != null)
                    {
                        dp.ActivateKeyboardHandler();
                    }
                } 
            }
        }
        #endregion

        #region Members
        private string m_Text;							// Actual text displayed.
        
      	private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
      	
        private InfosFading m_InfosFading;
        
      	private bool m_bEditMode;
      	private static readonly int m_iDefaultFontSize = 16;    		// will also be used for the text box.
		
      	private Point m_TopLeft;                         	// position (in image coords).
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        
        private LabelBackground m_LabelBackground = new LabelBackground();
        private SizeF m_BackgroundSize;				  	// Size of the background rectangle (scaled).
        
        private TextBox m_TextBox;
        private PictureBox m_ContainerScreen;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingText(int x, int y, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_Text = " ";
            m_TopLeft = new Point(x, y);
            m_BackgroundSize = new SizeF(100, 20);
            
            // Decoration & binding with editors
            m_StyleHelper.Bicolor = new Bicolor(Color.Black);
            m_StyleHelper.Font = new Font("Arial", m_iDefaultFontSize, FontStyle.Bold);
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
            
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            m_bEditMode = false;

            // Textbox initialization.
            m_TextBox = new TextBox();
            m_TextBox.Visible = false;
            m_TextBox.BackColor = Color.White;
            m_TextBox.BorderStyle = BorderStyle.None;
            m_TextBox.Text = m_Text;
            m_TextBox.Font = m_StyleHelper.GetFontDefaultSize(m_iDefaultFontSize);
            m_TextBox.Multiline = true;
            m_TextBox.TextChanged += new EventHandler(TextBox_TextChanged);
            
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public DrawingText(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(0,0,0,0, ToolManager.Label.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if (fOpacityFactor > 0)
            {
                // Rescale the points.
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                // This method may be used from worker threads and shouldn't touch UI controls.
                // -> The textbox will not be modified here.
                if (!m_bEditMode)
                {
                    DrawBackground(_canvas, fOpacityFactor);
                    
                    SolidBrush textBrush = m_StyleHelper.GetForegroundBrush((int)(fOpacityFactor * 255));
                    Font fontText = m_StyleHelper.GetFont((float)m_fStretchFactor);
                   	_canvas.DrawString(m_Text, fontText, textBrush, m_LabelBackground.TextLocation);
                   	textBrush.Dispose();
                   	fontText.Dispose();
                }
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object.

            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
            	if(GetHandleRectangle().Contains(_point))
            	{
            		iHitResult = 1;	
            	}
                else if (IsPointInObject(_point))
                {
                    iHitResult = 0;
                }
            }

            return iHitResult;
        }
        public override void MoveHandle(Point point, int handleNumber)
        {	
        	// Invisible handler to change font size.
            // Compare wanted mouse position with current bottom right.
            int wantedHeight = point.Y - m_TopLeft.Y;
            m_StyleHelper.ForceFontSize(wantedHeight, m_Text);
            
            //AutoSizeTextbox();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // Note: _delatX and _delatY are mouse delta already descaled.            
            m_TopLeft.X += _deltaX;
            m_TopLeft.Y += _deltaY;

            if (m_bEditMode && m_TextBox != null)
            {
                m_TextBox.Top += _deltaY;
                m_TextBox.Left += _deltaX;
            }
            
            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
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
					case "Text":
				        m_Text = _xmlReader.ReadElementContentAsString();
                        break;
					case "Position":
                        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        m_TopLeft = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
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
            
			RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            AutoSizeTextbox();
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("Text", m_Text);
            _xmlWriter.WriteElementString("Position", String.Format("{0};{1}", m_TopLeft.X, m_TopLeft.Y));
            
		    _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement(); 
		}
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            return ScreenManagerLang.ToolTip_DrawingToolText;
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_Text.GetHashCode();
            iHash ^= m_TopLeft.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }

        public void RelocateEditbox(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            m_fStretchFactor = _fStretchFactor;
            m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);

            RescaleCoordinates(_fStretchFactor, _DirectZoomTopLeft);
            m_TextBox.Location = new Point(m_ContainerScreen.Left + m_LabelBackground.Location.X + 6, m_ContainerScreen.Top + m_LabelBackground.Location.Y + 3);
        }

        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Bicolor", "back color");
            m_Style.Bind(m_StyleHelper, "Font", "font size");
        }
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            m_Text = m_TextBox.Text;
            AutoSizeTextbox();
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	m_LabelBackground.Location = new Point((int)((double)(m_TopLeft.X-_DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_TopLeft.Y-_DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private bool IsPointInObject(Point _point)
        {
            // Point coordinates are descaled.
            // We need to descale the hit area size for coherence.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddRectangle(new Rectangle(m_TopLeft.X, m_TopLeft.Y, descaledSize.Width, descaledSize.Height));

            // Create region from the path
            Region areaRegion = new Region(areaPath);

            bool hit = areaRegion.IsVisible(_point);
            return hit;
        }
        private void AutoSizeTextbox()
        {
            // We add a space at the end for when there is a new line with no characters in it.
            Button but = new Button();
            Graphics g = but.CreateGraphics();
           
            // Size of textbox, we don't use the actual font size (far too big)
            Font f = m_StyleHelper.GetFontDefaultSize(m_iDefaultFontSize);
            SizeF edSize = g.MeasureString(m_Text + " ", f);
            m_TextBox.Size = new Size((int)edSize.Width + 8, (int)edSize.Height);
            f.Dispose();
            g.Dispose();
        }
        private void DrawBackground(Graphics _canvas, double _fOpacityFactor)
        {
            // Draw background (Rounded rectangle)
            // The radius for rounding is based on font size.
            Font f = m_StyleHelper.GetFont((float)m_fStretchFactor);
            m_BackgroundSize = _canvas.MeasureString(m_Text + " ", f);
            int radius = (int)(f.Size / 2);
            f.Dispose();
            
            m_LabelBackground.Draw(_canvas, _fOpacityFactor, radius, (int)m_BackgroundSize.Width, (int)m_BackgroundSize.Height, m_StyleHelper.Bicolor.Background);
        }
        
        private Rectangle GetHandleRectangle()
        {
            // This function is only used for Hit Testing.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            return new Rectangle(m_TopLeft.X + descaledSize.Width - 10, m_TopLeft.Y + descaledSize.Height - 10, 20, 20);
        }
        #endregion
    }
}
