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
        }
        #endregion

        #region Members
        private string m_Text;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
      	private bool m_bEditMode;
      	private CoordinateSystem m_CoordinateSystem;
      	
      	private RoundedRectangle m_Background = new RoundedRectangle();
        private TextBox m_TextBox;
      	private PictureBox m_ContainerScreen;
        
        private const int m_iDefaultFontSize = 16;    		// will also be used for the text box.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingText(Point p, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_Text = " ";
            m_Background.Rectangle = new Rectangle(p, Size.Empty);
            
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

            m_TextBox = new TextBox() { 
      	         Visible = false, 
      	         BackColor = Color.White, 
      	         BorderStyle = BorderStyle.None, 
      	         Multiline = true,
      	         Text = m_Text,
      	         Font = m_StyleHelper.GetFontDefaultSize(m_iDefaultFontSize)
            };
            
            m_TextBox.TextChanged += new EventHandler(TextBox_TextChanged);
            
            UpdateLabelRectangle();
        }
        public DrawingText(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty,0,0, ToolManager.Label.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor <= 0 || m_bEditMode)
                return;
                
            using (SolidBrush brushBack = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor * 128)))
            using (SolidBrush brushText = m_StyleHelper.GetForegroundBrush((int)(fOpacityFactor * 255)))
            using (Font fontText = m_StyleHelper.GetFont((float)_transformer.Scale))
            {
                Rectangle rect = _transformer.Transform(m_Background.Rectangle);
                RoundedRectangle.Draw(_canvas, rect, brushBack, fontText.Height/4, false, false, null);
                _canvas.DrawString(m_Text, fontText, brushText, rect.Location);
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                iHitResult = m_Background.HitTest(_point, true);
            }

            return iHitResult;
        }
        public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
        {	
            // Invisible handler to change font size.
            int wantedHeight = point.Y - m_Background.Rectangle.Location.Y;
            m_StyleHelper.ForceFontSize(wantedHeight, m_Text);
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            m_Background.Move(_deltaX, _deltaY);
            RelocateEditbox();
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
                        Point location = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                        m_Background.Rectangle = new Rectangle(location, Size.Empty);
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
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("Text", m_Text);
            _xmlWriter.WriteElementString("Position", String.Format("{0};{1}", m_Background.Rectangle.X, m_Background.Rectangle.Y));
            
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
            return ScreenManagerLang.ToolTip_DrawingToolText;
        }
        public override int GetHashCode()
        {
            int iHash = m_Text.GetHashCode();
            iHash ^= m_Background.Rectangle.Location.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }
        public void SetEditMode(bool _bEdit, CoordinateSystem _transformer)
        {
            m_bEditMode = _bEdit;

            if(m_CoordinateSystem == null)
               m_CoordinateSystem = _transformer; 

            // Activate or deactivate the ScreenManager Keyboard Handler, 
            // so we can use <space>, <return>, etc.
            DelegatesPool dp = DelegatesPool.Instance();
            if (m_bEditMode)
            {    
                if (dp.DeactivateKeyboardHandler != null)
                    dp.DeactivateKeyboardHandler();

                RelocateEditbox(); // This is needed because the container top-left corner may have changed 
            }
            else
            {
                if (dp.ActivateKeyboardHandler != null)
                    dp.ActivateKeyboardHandler();
            }
            
            m_TextBox.Visible = m_bEditMode;
        }
        public void RelocateEditbox()
        {
            if(m_CoordinateSystem != null)
            {
                Rectangle rect =  m_CoordinateSystem.Transform(m_Background.Rectangle);
                m_TextBox.Location = rect.Location.Translate(m_ContainerScreen.Left, m_ContainerScreen.Top);
            }
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
            UpdateLabelRectangle();
        }
        private void UpdateLabelRectangle()
        {
            // Text changed or font size changed.
            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            using(Font f = m_StyleHelper.GetFont(1F))
            {
                SizeF textSize = g.MeasureString(m_Text, f);
                m_Background.Rectangle = new Rectangle(m_Background.Rectangle.Location, new Size((int)textSize.Width, (int)textSize.Height));
                
                // Also update the edit box size. (Use a fixed font though).
                // The extra space is to account for blank new lines.
                SizeF boxSize = g.MeasureString(m_Text + " ", m_TextBox.Font);
                m_TextBox.Size = new Size((int)boxSize.Width + 10, (int)boxSize.Height);
            }
        }
        #endregion
    }
}
